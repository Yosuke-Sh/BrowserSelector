using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.Services;
using BrowserSelector.Infrastructure.Logging;
using FluentAssertions;
using Moq;

namespace BrowserSelector.UnitTests;

public class BrowserServiceTests : IDisposable
{
    private readonly Mock<IRegistryService> _mockRegistryService;
    private readonly Mock<IUrlService> _mockUrlService;
    private readonly Mock<ILogService> _mockLogService;
    private readonly BrowserService _browserService;
    private readonly string _tempSettingsDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserServiceTests"/> class.
    /// </summary>
    public BrowserServiceTests()
    {
        _mockRegistryService = new Mock<IRegistryService>();
        _mockUrlService = new Mock<IUrlService>();
        _mockLogService = new Mock<ILogService>();

        // 既定コンストラクタは全インスタンス共通の%AppData%\BrowserSelector\browsers.jsonへ書き込むため、
        // 他のテストクラス（実BrowserServiceを構築するもの）と並列実行された際にファイル書き込みが競合し、
        // まれにAddBrowserAsync等がIOExceptionを握りつぶしてfalseを返すCIの間欠的失敗があった。
        // テストごとに一意な一時ディレクトリを使うことで競合を根本的に回避する。
        _tempSettingsDirectory = Path.Combine(Path.GetTempPath(), "BrowserSelectorTest", Guid.NewGuid().ToString());
        _browserService = new BrowserService(_mockRegistryService.Object, _mockUrlService.Object, _mockLogService.Object, _tempSettingsDirectory);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempSettingsDirectory))
            {
                Directory.Delete(_tempSettingsDirectory, recursive: true);
            }
        }
        catch (IOException)
        {
            // 削除に失敗しても後続のテストには影響しないため無視する。
        }
        catch (UnauthorizedAccessException)
        {
            // 削除に失敗しても後続のテストには影響しないため無視する。
        }
    }


    [Fact]
    public async Task DetectBrowsersAsync_WithNoBrowsers_ShouldReturnEmpty()
    {
        // Arrange
        _ = _mockRegistryService
            .Setup(x => x.DetectBrowsersFromRegistryAsync())
            .ReturnsAsync([]);

        // Act
        IEnumerable<Browser> result = await _browserService.DetectBrowsersAsync();

        // Assert
        _ = result.Should().BeEmpty();
    }

    [Fact]
    public async Task DetectBrowsersAsync_WithException_ShouldReturnEmpty()
    {
        // Arrange
        _ = _mockRegistryService
            .Setup(x => x.DetectBrowsersFromRegistryAsync())
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        IEnumerable<Browser> result = await _browserService.DetectBrowsersAsync();

        // Assert
        _ = result.Should().BeEmpty();
    }


    [Fact]
    public async Task AddBrowserAsync_WithInvalidBrowser_ShouldReturnFalse()
    {
        // Arrange
        Browser browser = new()
        {
            Name = "",
            ExecutablePath = ""
        };

        // Act
        bool result = await _browserService.AddBrowserAsync(browser);

        // Assert
        _ = result.Should().BeFalse();
    }

    [Fact]
    public async Task AddBrowserAsync_WithSameExecutablePathAndSameArguments_ShouldReturnFalse()
    {
        // Arrange
        Browser first = new()
        {
            Name = "Chrome",
            ExecutablePath = @"C:\Program Files\Chrome\chrome.exe",
            Arguments = "--incognito"
        };
        Browser duplicate = new()
        {
            Name = "Chrome (Duplicate)",
            ExecutablePath = @"C:\Program Files\Chrome\chrome.exe",
            Arguments = "--incognito"
        };

        // Act
        bool firstResult = await _browserService.AddBrowserAsync(first);
        bool duplicateResult = await _browserService.AddBrowserAsync(duplicate);

        // Assert
        _ = firstResult.Should().BeTrue();
        _ = duplicateResult.Should().BeFalse();
    }

    [Fact]
    public async Task AddBrowserAsync_WithSameExecutablePathButDifferentArguments_ShouldReturnTrue()
    {
        // Arrange
        Browser first = new()
        {
            Name = "Chrome",
            ExecutablePath = @"C:\Program Files\Chrome\chrome.exe",
            Arguments = "--incognito"
        };
        Browser second = new()
        {
            Name = "Chrome (Profile2)",
            ExecutablePath = @"C:\Program Files\Chrome\chrome.exe",
            Arguments = "--profile-directory=Profile2"
        };

        // Act
        bool firstResult = await _browserService.AddBrowserAsync(first);
        bool secondResult = await _browserService.AddBrowserAsync(second);

        // Assert
        _ = firstResult.Should().BeTrue();
        _ = secondResult.Should().BeTrue();
    }

    [Fact]
    public async Task LaunchBrowserAsync_WhenProcessExitsImmediately_ShouldReturnTrue()
    {
        // Arrange
        // cmd.exe /c exit は起動直後に終了するため、Chrome/Edge等がブートストラップ
        // プロセスを既存インスタンスへの引き渡し後すぐ終了させる状況を再現する。
        // 起動確認にProcess.GetProcessByIdを使うと、この即終了によりArgumentExceptionが
        // 発生して成功判定を握りつぶす不具合があった（該当コードは削除済み）。
        string systemRoot = Environment.GetEnvironmentVariable("SystemRoot") ?? @"C:\Windows";
        string cmdPath = System.IO.Path.Combine(systemRoot, "System32", "cmd.exe");
        Browser browser = new()
        {
            Name = "ImmediateExitProcess",
            ExecutablePath = cmdPath,
            Arguments = "/c exit"
        };

        _ = _mockUrlService
            .Setup(x => x.NormalizeUrlAsync(It.IsAny<Uri>()))
            .ReturnsAsync("https://example.com/");
        _ = _mockUrlService
            .Setup(x => x.ValidateUrlAsync(It.IsAny<Uri>()))
            .ReturnsAsync(true);

        // Act
        bool result = await _browserService.LaunchBrowserAsync(browser, "https://example.com/");

        // Assert
        _ = result.Should().BeTrue();
        _ = browser.UseCount.Should().Be(1);
    }

}

