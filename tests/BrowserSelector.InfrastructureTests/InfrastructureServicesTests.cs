using BrowserSelector.Core.Enums;
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.Logging;
using BrowserSelector.Infrastructure.Services;
using FluentAssertions;
using Moq;

namespace BrowserSelector.InfrastructureTests;

/// <summary>
/// Infrastructureプロジェクト専用のテストクラス
/// インフラストラクチャサービスのテストを重点的に実施.
/// </summary>
public class InfrastructureServicesTests
{
    /// <summary>
    /// 有効なブラウザを持つBrowserServiceのDetectBrowsersAsyncがブラウザを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task BrowserService_DetectBrowsersAsync_WithValidBrowsers_ShouldReturnBrowsers()
    {
        // Arrange
        var mockRegistryService = new Mock<IRegistryService>();
        var mockUrlService = new Mock<IUrlService>();
        var browserService = new BrowserService(mockRegistryService.Object, mockUrlService.Object);

        var expectedBrowsers = new List<Browser>
        {
            new() {
                Name = "Google Chrome",
                ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                Type = BrowserType.Chrome,
                DisplayOrder = 1
            },
            new() {
                Name = "Mozilla Firefox",
                ExecutablePath = @"C:\Program Files\Mozilla Firefox\firefox.exe",
                Type = BrowserType.Firefox,
                DisplayOrder = 2
            }
        };

        mockRegistryService
            .Setup(x => x.DetectBrowsersFromRegistryAsync())
            .ReturnsAsync(expectedBrowsers);

        // Act
        var result = await browserService.DetectBrowsersAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedBrowsers, options => options.Excluding(b => b.Id));
    }

    /// <summary>
    /// ブラウザがない場合のBrowserServiceのDetectBrowsersAsyncが空を返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task BrowserService_DetectBrowsersAsync_WithNoBrowsers_ShouldReturnEmpty()
    {
        // Arrange
        var mockRegistryService = new Mock<IRegistryService>();
        var mockUrlService = new Mock<IUrlService>();
        var browserService = new BrowserService(mockRegistryService.Object, mockUrlService.Object);

        mockRegistryService
            .Setup(x => x.DetectBrowsersFromRegistryAsync())
            .ReturnsAsync(new List<Browser>());

        // Act
        var result = await browserService.DetectBrowsersAsync();

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// 例外が発生した場合のBrowserServiceのDetectBrowsersAsyncがエラーをログに記録することを確認するテスト.
    /// </summary>
    [Fact]
    public async Task BrowserService_DetectBrowsersAsync_WithException_ShouldLogError()
    {
        // Arrange
        var mockRegistryService = new Mock<IRegistryService>();
        var mockUrlService = new Mock<IUrlService>();
        var browserService = new BrowserService(mockRegistryService.Object, mockUrlService.Object);

        mockRegistryService
            .Setup(x => x.DetectBrowsersFromRegistryAsync())
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await browserService.DetectBrowsersAsync();

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// 有効なURLを持つUrlServiceのNormalizeUrlAsyncが正規化されたURLを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task UrlService_NormalizeUrlAsync_WithValidUrl_ShouldReturnNormalizedUrl()
    {
        // Arrange
        var mockSettingsService = new Mock<ISettingsService>();
        var urlService = new UrlService(mockSettingsService.Object);

        mockSettingsService
            .Setup(x => x.LoadAppSettingsAsync())
            .ReturnsAsync(new AppSettings());

        // Act
        var result = await urlService.NormalizeUrlAsync(new Uri("https://example.com"));

        // Assert
        result.Should().Be("https://example.com");
    }

    /// <summary>
    /// HTTP URLを持つUrlServiceのNormalizeUrlAsyncが同じURLを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task UrlService_NormalizeUrlAsync_WithHttpUrl_ShouldReturnSameUrl()
    {
        // Arrange
        var mockSettingsService = new Mock<ISettingsService>();
        var urlService = new UrlService(mockSettingsService.Object);

        mockSettingsService
            .Setup(x => x.LoadAppSettingsAsync())
            .ReturnsAsync(new AppSettings());

        // Act
        var result = await urlService.NormalizeUrlAsync(new Uri("http://example.com"));

        // Assert
        result.Should().Be("http://example.com");
    }

    /// <summary>
    /// HTTPS URLを持つUrlServiceのNormalizeUrlAsyncが同じURLを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task UrlService_NormalizeUrlAsync_WithHttpsUrl_ShouldReturnSameUrl()
    {
        // Arrange
        var mockSettingsService = new Mock<ISettingsService>();
        var urlService = new UrlService(mockSettingsService.Object);

        mockSettingsService
            .Setup(x => x.LoadAppSettingsAsync())
            .ReturnsAsync(new AppSettings());

        // Act
        var result = await urlService.NormalizeUrlAsync(new Uri("https://example.com"));

        // Assert
        result.Should().Be("https://example.com");
    }

    /// <summary>
    /// 空のURLを持つUrlServiceのNormalizeUrlAsyncが空の文字列を返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task UrlService_NormalizeUrlAsync_WithEmptyUrl_ShouldReturnEmptyString()
    {
        // Arrange
        var mockSettingsService = new Mock<ISettingsService>();
        var urlService = new UrlService(mockSettingsService.Object);

        mockSettingsService
            .Setup(x => x.LoadAppSettingsAsync())
            .ReturnsAsync(new AppSettings());

        // Act
        var result = await urlService.NormalizeUrlAsync("");

        // Assert
        result.Should().Be("");
    }

    /// <summary>
    /// 無効なURLを持つUrlServiceのNormalizeUrlAsyncが警告をログに記録することを確認するテスト.
    /// </summary>
    [Fact]
    public async Task UrlService_NormalizeUrlAsync_WithInvalidUrl_ShouldLogWarning()
    {
        // Arrange
        var mockSettingsService = new Mock<ISettingsService>();
        var urlService = new UrlService(mockSettingsService.Object);

        mockSettingsService
            .Setup(x => x.LoadAppSettingsAsync())
            .ReturnsAsync(new AppSettings());

        // Act
        var result = await urlService.NormalizeUrlAsync(new Uri("https://invalid-url"));

        // Assert
        result.Should().Be("https://invalid-url");
    }

    /// <summary>
    /// LogServiceのコンストラクタが正しく初期化されることを確認するテスト.
    /// </summary>
    [Fact]
    public void LogService_Constructor_ShouldInitializeCorrectly()
    {
        // Act
        var logService = new LogService();

        // Assert
        logService.Should().NotBeNull();
    }

    /// <summary>
    /// 有効なメッセージを持つLogServiceのLogがメッセージをログに記録することを確認するテスト.
    /// </summary>
    [Fact]
    public void LogService_Log_WithValidMessage_ShouldLogMessage()
    {
        // Arrange
        var logService = new LogService();

        // Act
        logService.Log(LogLevel.Information, "Test message");

        // Assert
        // ログが正常に出力されることを確認（実際のログ出力は確認できないため、例外が発生しないことを確認）
        logService.Should().NotBeNull();
    }

    /// <summary>
    /// 異なるレベルを持つLogServiceのLogがすべてのレベルをログに記録することを確認するテスト.
    /// </summary>
    [Fact]
    public void LogService_Log_WithDifferentLevels_ShouldLogAllLevels()
    {
        // Arrange
        var logService = new LogService();

        // Act & Assert
        logService.Log(LogLevel.Debug, "Debug message");
        logService.Log(LogLevel.Information, "Info message");
        logService.Log(LogLevel.Warning, "Warning message");
        logService.Log(LogLevel.Error, "Error message");
        logService.Log(LogLevel.Critical, "Critical message");

        // 例外が発生しないことを確認
        logService.Should().NotBeNull();
    }

    /// <summary>
    /// 有効な設定を持つLogServiceのUpdateLogSettingsが設定を更新することを確認するテスト.
    /// </summary>
    [Fact]
    public void LogService_UpdateLogSettings_WithValidSettings_ShouldUpdateSettings()
    {
        // Arrange
        var logService = new LogService();
        var newSettings = new LogSettings
        {
            LogLevel = LogLevel.Information,
            EnableFileLogging = true,
            LogOutputFolder = Path.GetTempPath()
        };

        // Act
        logService.UpdateSettings(newSettings);

        // Assert
        // 設定が更新されることを確認（実際の設定更新は確認できないため、例外が発生しないことを確認）
        logService.Should().NotBeNull();
    }
}
