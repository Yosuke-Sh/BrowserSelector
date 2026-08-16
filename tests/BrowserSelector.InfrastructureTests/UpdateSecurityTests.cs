using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.Updates;
using BrowserSelector.InfrastructureTests.TestDoubles;
using FluentAssertions;
using Moq;

namespace BrowserSelector.InfrastructureTests;

/// <summary>
/// Phase H-11: 自動アップデート機能のセキュリティテスト.
/// コード署名が無いため、ホスト検証・チェックサム検証・Zip Slip対策が
/// 「掴まされた成果物が本物か」を保証する唯一の手段になる.
/// </summary>
public sealed class UpdateSecurityTests : IDisposable
{
    private const string InstallerName = "BrowserSelector-Setup-v0.9.0.exe";

    private static readonly byte[] InstallerBytes = Encoding.UTF8.GetBytes("fake installer payload");

    private readonly string _workDirectory;

    public UpdateSecurityTests()
    {
        _workDirectory = Path.Combine(Path.GetTempPath(), $"BSSecurityTest_{Guid.NewGuid():N}");
        _ = Directory.CreateDirectory(_workDirectory);
    }

    // %LOCALAPPDATA%\BrowserSelector\updates\{version}はバージョン番号がそのままディレクトリ名になるため、
    // 他のテストクラス（UpdateServiceDownloadTests等）と同じ固定バージョンを使うと並列実行時に競合する。
    // テストクラスごとに一意なバージョンを用いて衝突を避ける（暗号目的ではないためGuid由来の値で十分）。
    private readonly Version _testVersion = new(9, 9, Math.Abs(Guid.NewGuid().GetHashCode() % 9000) + 1000);

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_workDirectory))
            {
                Directory.Delete(_workDirectory, recursive: true);
            }

            string versionDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BrowserSelector",
                "updates",
                _testVersion.ToString());
            if (Directory.Exists(versionDirectory))
            {
                Directory.Delete(versionDirectory, recursive: true);
            }
        }
        catch (IOException)
        {
            // テスト用の一時ディレクトリなので削除失敗は無視してよい。
        }
    }

    // --- UpdateHostValidator: ホスト検証 ---

    [Theory]
    [InlineData("http://api.github.com/repos/foo/bar/releases/latest")] // 平文HTTPは拒否
    [InlineData("https://evil.com/repos/foo/bar/releases/latest")] // 未知ホスト
    [InlineData("https://evil.com/?x=api.github.com")] // クエリ文字列にホスト名を混入
    [InlineData("https://api.github.com.evil.com/releases/latest")] // サフィックス偽装（前方一致狙い）
    [InlineData("https://evil-githubusercontent.com/foo/bar.zip")] // githubusercontent.comのサフィックス偽装
    [InlineData("https://githubusercontent.com.evil.com/foo/bar.zip")]
    public void IsAllowedHost_ShouldRejectUntrustedOrSpoofedHosts(string url)
    {
        var uri = new Uri(url);

        UpdateHostValidator.IsAllowedHost(uri).Should().BeFalse();
    }

    [Theory]
    [InlineData("https://api.github.com/repos/foo/bar/releases/latest")]
    [InlineData("https://github.com/foo/bar/releases/download/v1.0.0/asset.zip")]
    [InlineData("https://objects.githubusercontent.com/foo/bar.zip")]
    [InlineData("https://release-assets.githubusercontent.com/foo/bar.zip")]
    public void IsAllowedHost_ShouldAcceptTrustedHosts(string url)
    {
        var uri = new Uri(url);

        UpdateHostValidator.IsAllowedHost(uri).Should().BeTrue();
    }

    [Fact]
    public void IsAllowedHost_ShouldRejectNullUri()
    {
        UpdateHostValidator.IsAllowedHost(null).Should().BeFalse();
    }

    [Fact]
    public async Task DownloadUpdateAsync_ShouldRejectRedirectToUntrustedHost()
    {
        // レスポンス自体はGitHub URLへのGETに対して返るが、RequestMessage.RequestUriが
        // リダイレクト後の不正ホストを指している状態を模す（実HttpClientの挙動を模倣）。
        using var handler = new StubHttpMessageHandler(request =>
        {
            if (request.RequestUri!.ToString().Contains("SHA256SUMS", StringComparison.Ordinal))
            {
                string hash = Convert.ToHexStringLower(SHA256.HashData(InstallerBytes));
                var checksumResponse = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent($"{hash}  {InstallerName}\n", Encoding.UTF8, "text/plain"),
                };
                return checksumResponse;
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(InstallerBytes),
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://evil.com/redirected-payload.exe"),
            };
            return response;
        });
        using UpdateService service = CreateService(handler);

        UpdateDownloadResult result = await service.DownloadUpdateAsync(CreateUpdateInfo(), UpdateChannel.Installer);

        result.Success.Should().BeFalse();
        result.Failure.Should().Be(UpdateDownloadFailure.Network);
    }

    // --- ZipExtractor: Zip Slip対策 ---

    [Theory]
    [InlineData("../../evil.exe")]
    [InlineData("..\\..\\evil.exe")]
    [InlineData("../outside.txt")]
    public void IsEntryNameSafe_ShouldRejectParentDirectoryTraversal(string entryName)
    {
        ZipExtractor.IsEntryNameSafe(entryName).Should().BeFalse();
    }

    [Theory]
    [InlineData("C:\\Windows\\System32\\evil.exe")]
    [InlineData("\\\\server\\share\\evil.exe")]
    [InlineData("\\evil.exe")]
    public void IsEntryNameSafe_ShouldRejectAbsolutePaths(string entryName)
    {
        ZipExtractor.IsEntryNameSafe(entryName).Should().BeFalse();
    }

    [Fact]
    public void IsEntryNameSafe_ShouldRejectAlternateDataStream()
    {
        ZipExtractor.IsEntryNameSafe("BrowserSelector.exe:hidden.exe").Should().BeFalse();
    }

    [Theory]
    [InlineData("BrowserSelector.exe")]
    [InlineData("lib\\dependency.dll")]
    [InlineData("lib/dependency.dll")]
    public void IsEntryNameSafe_ShouldAcceptNormalRelativePaths(string entryName)
    {
        ZipExtractor.IsEntryNameSafe(entryName).Should().BeTrue();
    }

    [Fact]
    public void TryExtract_ShouldAbortOnZipSlipEntry()
    {
        string zipPath = Path.Combine(_workDirectory, "malicious.zip");
        string destination = Path.Combine(_workDirectory, "extracted");

        using (FileStream zipStream = File.Create(zipPath))
        using (ZipArchive archive = new(zipStream, ZipArchiveMode.Create))
        {
            ZipArchiveEntry entry = archive.CreateEntry("../../escaped.exe");
            using StreamWriter writer = new(entry.Open());
            writer.Write("malicious payload");
        }

        bool result = ZipExtractor.TryExtract(zipPath, destination, out string? failureReason);

        result.Should().BeFalse();
        failureReason.Should().NotBeNullOrEmpty();
        File.Exists(Path.Combine(_workDirectory, "escaped.exe")).Should().BeFalse();
    }

    [Fact]
    public void TryExtract_ShouldAbortWhenEntryCountExceedsLimit()
    {
        string zipPath = Path.Combine(_workDirectory, "zipbomb-entries.zip");
        string destination = Path.Combine(_workDirectory, "extracted-entries");

        using (FileStream zipStream = File.Create(zipPath))
        using (ZipArchive archive = new(zipStream, ZipArchiveMode.Create))
        {
            for (int i = 0; i < ZipExtractor.MaxEntryCount + 1; i++)
            {
                _ = archive.CreateEntry($"file{i}.txt");
            }
        }

        bool result = ZipExtractor.TryExtract(zipPath, destination, out string? failureReason);

        result.Should().BeFalse();
        failureReason.Should().Contain("エントリ数");
    }

    [Fact]
    public void TryExtract_ShouldFailWhenRequiredExecutableIsMissing()
    {
        string zipPath = Path.Combine(_workDirectory, "no-exe.zip");
        string destination = Path.Combine(_workDirectory, "extracted-no-exe");

        using (FileStream zipStream = File.Create(zipPath))
        using (ZipArchive archive = new(zipStream, ZipArchiveMode.Create))
        {
            ZipArchiveEntry entry = archive.CreateEntry("readme.txt");
            using StreamWriter writer = new(entry.Open());
            writer.Write("no executable here");
        }

        bool result = ZipExtractor.TryExtract(zipPath, destination, out string? failureReason);

        result.Should().BeFalse();
        failureReason.Should().Contain(ZipExtractor.RequiredExecutableName);
    }

    // --- チェックサム不一致時に実行ファイルが残らないこと ---

    [Fact]
    public async Task DownloadUpdateAsync_ShouldNotLeaveExecutableWhenChecksumMismatches()
    {
        using var handler = new StubHttpMessageHandler(request =>
        {
            if (request.RequestUri!.ToString().Contains("SHA256SUMS", StringComparison.Ordinal))
            {
                string tamperedHash = new('a', 64);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent($"{tamperedHash}  {InstallerName}\n", Encoding.UTF8, "text/plain"),
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(InstallerBytes),
            };
        });
        using UpdateService service = CreateService(handler);

        UpdateDownloadResult result = await service.DownloadUpdateAsync(CreateUpdateInfo(), UpdateChannel.Installer);

        result.Success.Should().BeFalse();

        string versionDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BrowserSelector",
            "updates",
            _testVersion.ToString());
        if (Directory.Exists(versionDirectory))
        {
            Directory.GetFiles(versionDirectory, "*.exe", SearchOption.AllDirectories).Should().BeEmpty();
        }
    }

    private UpdateInfo CreateUpdateInfo() => new()
    {
        Version = _testVersion,
        TagName = $"v{_testVersion}",
        InstallerAsset = new UpdateAsset(
            InstallerName,
            new Uri($"https://github.com/Yosuke-Sh/BrowserSelector/releases/download/v{_testVersion}/{InstallerName}"),
            InstallerBytes.Length),
        ChecksumsAsset = new UpdateAsset(
            "SHA256SUMS.txt",
            new Uri($"https://github.com/Yosuke-Sh/BrowserSelector/releases/download/v{_testVersion}/SHA256SUMS.txt"),
            100),
    };

    private UpdateService CreateService(HttpMessageHandler handler)
    {
        var settingsService = new Mock<ISettingsService>();
        _ = settingsService.Setup(s => s.LoadAppSettingsAsync()).ReturnsAsync(new AppSettings());

        return new UpdateService(
            TestHttpClientFactory.Create(UpdateService.HttpClientName, handler),
            settingsService.Object,
            Mock.Of<ILogService>(),
            Path.Combine(_workDirectory, "etag.json"),
            new Version(0, 3, 0),
            _workDirectory,
            new RecordingProcessLauncher());
    }
}
