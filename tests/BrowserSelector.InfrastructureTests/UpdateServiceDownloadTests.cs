using System.IO;
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
/// Phase H-4: ダウンロードとSHA256による完全性検証のテスト.
/// コード署名が無いため、ここの検証が唯一の「掴まされた成果物が本物か」の判断材料になる.
/// </summary>
public sealed class UpdateServiceDownloadTests : IDisposable
{
    private const string InstallerName = "BrowserSelector-Setup-v0.9.0.exe";
    private const string InstallerUrl = "https://github.com/Yosuke-Sh/BrowserSelector/releases/download/v0.9.0/BrowserSelector-Setup-v0.9.0.exe";
    private const string ChecksumsUrl = "https://github.com/Yosuke-Sh/BrowserSelector/releases/download/v0.9.0/SHA256SUMS.txt";

    private static readonly byte[] InstallerBytes = Encoding.UTF8.GetBytes("fake installer payload");

    private readonly string _workDirectory;

    public UpdateServiceDownloadTests()
    {
        _workDirectory = Path.Combine(Path.GetTempPath(), $"BSDownloadTest_{Guid.NewGuid():N}");
        _ = Directory.CreateDirectory(_workDirectory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_workDirectory))
            {
                Directory.Delete(_workDirectory, recursive: true);
            }

            // UpdatePathsは%LOCALAPPDATA%固定のため、テストが作った版数ディレクトリを掃除する。
            string versionDirectory = UpdatePathsVersionDirectory();
            if (Directory.Exists(versionDirectory))
            {
                Directory.Delete(versionDirectory, recursive: true);
            }
        }
        catch (IOException)
        {
        }
    }

    [Fact]
    public async Task DownloadUpdateAsync_ShouldSaveWithOriginalFileNameAndExtension()
    {
        using var handler = new StubHttpMessageHandler(Respond());
        using UpdateService service = CreateService(handler);
        UpdateInfo info = CreateUpdateInfo();

        UpdateDownloadResult result = await service.DownloadUpdateAsync(info, UpdateChannel.Installer);

        result.Success.Should().BeTrue();

        // v0.2.0はPath.GetRandomFileName()で拡張子なしに保存していたため、
        // インストーラをProcess.Startしても起動しなかった。その回帰防止。
        Path.GetFileName(result.FilePath).Should().Be(InstallerName);
        File.Exists(result.FilePath).Should().BeTrue();
    }

    [Fact]
    public async Task DownloadUpdateAsync_ShouldSetLocalFilePathAndIsDownloaded()
    {
        using var handler = new StubHttpMessageHandler(Respond());
        using UpdateService service = CreateService(handler);
        UpdateInfo info = CreateUpdateInfo();

        _ = await service.DownloadUpdateAsync(info, UpdateChannel.Installer);

        info.IsDownloaded.Should().BeTrue();
        info.LocalFilePath.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DownloadUpdateAsync_ShouldDeleteFileWhenChecksumMismatches()
    {
        // 改竄・破損したファイルを実行可能な状態で残さないこと。
        using var handler = new StubHttpMessageHandler(Respond(checksumOverride: new string('a', 64)));
        using UpdateService service = CreateService(handler);
        UpdateInfo info = CreateUpdateInfo();

        UpdateDownloadResult result = await service.DownloadUpdateAsync(info, UpdateChannel.Installer);

        result.Success.Should().BeFalse();
        result.Failure.Should().Be(UpdateDownloadFailure.ChecksumMismatch);
        result.FilePath.Should().BeNull();

        File.Exists(Path.Combine(UpdatePathsVersionDirectory(), InstallerName)).Should().BeFalse();
        info.IsDownloaded.Should().BeFalse();
    }

    [Fact]
    public async Task DownloadUpdateAsync_ShouldFailWhenChecksumFileCannotBeDownloaded()
    {
        // 署名が無い以上、検証を省略して続行する選択肢は取らない。
        using var handler = new StubHttpMessageHandler(request =>
            request.RequestUri!.ToString().Contains("SHA256SUMS", StringComparison.Ordinal)
                ? new HttpResponseMessage(HttpStatusCode.NotFound)
                : BinaryResponse(InstallerBytes));
        using UpdateService service = CreateService(handler);

        UpdateDownloadResult result = await service.DownloadUpdateAsync(CreateUpdateInfo(), UpdateChannel.Installer);

        result.Failure.Should().Be(UpdateDownloadFailure.ChecksumUnavailable);
    }

    [Fact]
    public async Task DownloadUpdateAsync_ShouldFailWhenChecksumEntryIsMissing()
    {
        using var handler = new StubHttpMessageHandler(Respond(checksumsBody: $"{new string('b', 64)}  other-file.exe\n"));
        using UpdateService service = CreateService(handler);

        UpdateDownloadResult result = await service.DownloadUpdateAsync(CreateUpdateInfo(), UpdateChannel.Installer);

        result.Failure.Should().Be(UpdateDownloadFailure.ChecksumUnavailable);
    }

    [Fact]
    public async Task DownloadUpdateAsync_ShouldFailWhenChecksumsAssetIsAbsent()
    {
        using var handler = new StubHttpMessageHandler(Respond());
        using UpdateService service = CreateService(handler);
        UpdateInfo info = CreateUpdateInfo();
        info.ChecksumsAsset = null;

        UpdateDownloadResult result = await service.DownloadUpdateAsync(info, UpdateChannel.Installer);

        result.Failure.Should().Be(UpdateDownloadFailure.ChecksumUnavailable);
    }

    [Fact]
    public async Task DownloadUpdateAsync_ShouldFailWhenRequestedChannelAssetIsAbsent()
    {
        using var handler = new StubHttpMessageHandler(Respond());
        using UpdateService service = CreateService(handler);
        UpdateInfo info = CreateUpdateInfo();

        // PortableAssetを持たないリリースにPortableを要求した場合。
        UpdateDownloadResult result = await service.DownloadUpdateAsync(info, UpdateChannel.Portable);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task DownloadUpdateAsync_ShouldReportProgress()
    {
        using var handler = new StubHttpMessageHandler(Respond());
        using UpdateService service = CreateService(handler);

        List<int> reported = [];
        var progress = new Progress<int>(reported.Add);

        _ = await service.DownloadUpdateAsync(CreateUpdateInfo(), UpdateChannel.Installer, progress);

        // Progress<T>は同期コンテキスト経由で非同期に呼ばれるため、少し待ってから確認する。
        await Task.Delay(200);
        reported.Should().NotBeEmpty();
        reported.Should().OnlyContain(p => p >= 0 && p <= 100);
    }

    [Fact]
    public async Task DownloadUpdateAsync_ShouldReturnCanceledWhenTokenIsCanceled()
    {
        using var handler = new StubHttpMessageHandler(Respond());
        using UpdateService service = CreateService(handler);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        UpdateDownloadResult result = await service.DownloadUpdateAsync(
            CreateUpdateInfo(), UpdateChannel.Installer, progress: null, cts.Token);

        result.Failure.Should().Be(UpdateDownloadFailure.Canceled);
    }

    [Fact]
    public async Task DownloadUpdateAsync_ShouldReturnNetworkFailureOnHttpError()
    {
        using var handler = new StubHttpMessageHandler(request =>
            request.RequestUri!.ToString().Contains("SHA256SUMS", StringComparison.Ordinal)
                ? TextResponse(ValidChecksumsBody())
                : new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        using UpdateService service = CreateService(handler);

        UpdateDownloadResult result = await service.DownloadUpdateAsync(CreateUpdateInfo(), UpdateChannel.Installer);

        result.Failure.Should().Be(UpdateDownloadFailure.Network);
    }

    [Fact]
    public async Task DownloadUpdateAsync_ShouldThrowForNullUpdateInfo()
    {
        using var handler = new StubHttpMessageHandler(Respond());
        using UpdateService service = CreateService(handler);

        Func<Task> act = async () => await service.DownloadUpdateAsync(null!, UpdateChannel.Installer);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    private static string ValidChecksumsBody()
    {
        // release.ymlは小文字hexで出力するため、意図的に小文字で組み立てて
        // 大文字小文字を問わず照合できることを確認する（CA1308回避のためToHexStringLowerを使用）。
        string hash = Convert.ToHexStringLower(SHA256.HashData(InstallerBytes));
        return $"{hash}  {InstallerName}\n";
    }

    private static Func<HttpRequestMessage, HttpResponseMessage> Respond(
        string? checksumOverride = null,
        string? checksumsBody = null)
    {
        return request =>
        {
            if (request.RequestUri!.ToString().Contains("SHA256SUMS", StringComparison.Ordinal))
            {
                string body = checksumsBody
                    ?? (checksumOverride != null
                        ? $"{checksumOverride}  {InstallerName}\n"
                        : ValidChecksumsBody());
                return TextResponse(body);
            }

            return BinaryResponse(InstallerBytes);
        };
    }

    private static HttpResponseMessage TextResponse(string body) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(body, Encoding.UTF8, "text/plain"),
    };

    private static HttpResponseMessage BinaryResponse(byte[] bytes) => new(HttpStatusCode.OK)
    {
        Content = new ByteArrayContent(bytes),
    };

    private static UpdateInfo CreateUpdateInfo() => new()
    {
        Version = new Version(0, 9, 0),
        TagName = "v0.9.0",
        InstallerAsset = new UpdateAsset(InstallerName, new Uri(InstallerUrl), InstallerBytes.Length),
        ChecksumsAsset = new UpdateAsset("SHA256SUMS.txt", new Uri(ChecksumsUrl), 100),
    };

    private static string UpdatePathsVersionDirectory() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BrowserSelector",
        "updates",
        "0.9.0");

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
            _workDirectory);
    }
}
