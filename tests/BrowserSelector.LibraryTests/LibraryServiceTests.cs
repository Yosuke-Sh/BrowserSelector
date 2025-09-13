using BrowserSelector.Core.Enums;
using BrowserSelector.Core.Models;
using BrowserSelector.Library.Core.Services;
using BrowserSelector.Library.Infrastructure.Services;
using FluentAssertions;

namespace BrowserSelector.LibraryTests;

/// <summary>
/// LibraryServiceのテストクラス.
/// </summary>
public class LibraryServiceTests
{
    /// <summary>
    /// GetLibraryMessageメソッドが正しいメッセージを返すことをテスト.
    /// </summary>
    [Fact]
    public void LibraryService_GetLibraryMessage_ShouldReturnCorrectMessage()
    {
        // Arrange
        LibraryService libraryService = new LibraryService();

        // Act
        string message = libraryService.GetLibraryMessage();

        // Assert
        message.Should().Be("Hello from BrowserSelector.Library.Infrastructure!");
    }

    /// <summary>
    /// 有効なブラウザでValidateBrowserAsyncがtrueを返すことをテスト.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous unit test.</placeholder></returns>
    [Fact]
    public async Task ValidateBrowserAsync_WithValidBrowser_ShouldReturnTrue()
    {
        // Arrange
        var libraryService = new LibraryService();
        var browser = new Browser
        {
            Name = "Google Chrome",
            ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            Type = BrowserType.Chrome
        };

        // Act
        var result = await libraryService.ValidateBrowserAsync(browser);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// 空の名前を持つValidateBrowserAsyncがfalseを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task ValidateBrowserAsync_WithEmptyName_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        var browser = new Browser
        {
            Name = "",
            ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            Type = BrowserType.Chrome
        };

        // Act
        var result = await libraryService.ValidateBrowserAsync(browser);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// 空の実行パスでValidateBrowserAsyncがfalseを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task ValidateBrowserAsync_WithEmptyExecutablePath_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        var browser = new Browser
        {
            Name = "Google Chrome",
            ExecutablePath = "",
            Type = BrowserType.Chrome
        };

        // Act
        var result = await libraryService.ValidateBrowserAsync(browser);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// 無効な拡張子でValidateBrowserAsyncがfalseを返すことをテスト.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous unit test.</placeholder></returns>
    [Fact]
    public async Task ValidateBrowserAsync_WithInvalidExtension_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        var browser = new Browser
        {
            Name = "Google Chrome",
            ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.txt",
            Type = BrowserType.Chrome
        };

        // Act
        var result = await libraryService.ValidateBrowserAsync(browser);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// 有効なURLを持つNormalizeUrlAsyncが正規化されたURLを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task NormalizeUrlAsync_WithValidUrl_ShouldReturnNormalizedUrl()
    {
        // Arrange
        var libraryService = new LibraryService();
        string inputUrl = "example.com";
        string expectedUrl = "https://example.com";

        // Act
        var result = await libraryService.NormalizeUrlAsync(inputUrl);

        // Assert
        result.Should().Be(expectedUrl);
    }

    /// <summary>
    /// HTTP URLを持つNormalizeUrlAsyncが同じURLを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task NormalizeUrlAsync_WithHttpUrl_ShouldReturnSameUrl()
    {
        // Arrange
        var libraryService = new LibraryService();
        string inputUrl = "http://example.com";

        // Act
        var result = await libraryService.NormalizeUrlAsync(inputUrl);

        // Assert
        result.Should().Be(inputUrl);
    }

    /// <summary>
    /// HTTPS URLを持つNormalizeUrlAsyncが同じURLを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task NormalizeUrlAsync_WithHttpsUrl_ShouldReturnSameUrl()
    {
        // Arrange
        var libraryService = new LibraryService();
        string inputUrl = "https://example.com";

        // Act
        var result = await libraryService.NormalizeUrlAsync(inputUrl);

        // Assert
        result.Should().Be(inputUrl);
    }

    /// <summary>
    /// 空のURLを持つNormalizeUrlAsyncが空の文字列を返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task NormalizeUrlAsync_WithEmptyUrl_ShouldReturnEmptyString()
    {
        // Arrange
        var libraryService = new LibraryService();
        string inputUrl = "";

        // Act
        var result = await libraryService.NormalizeUrlAsync(inputUrl);

        // Assert
        result.Should().Be("");
    }

    /// <summary>
    /// 無効なURLを持つNormalizeUrlAsyncが元のURLを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task NormalizeUrlAsync_WithInvalidUrl_ShouldReturnOriginalUrl()
    {
        // Arrange
        var libraryService = new LibraryService();
        string inputUrl = "invalid-url-with-spaces and special chars!@#";

        // Act
        var result = await libraryService.NormalizeUrlAsync(inputUrl);

        // Assert
        result.Should().Be(inputUrl);
    }

    /// <summary>
    /// 有効な設定を持つValidateSettingsAsyncがtrueを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task ValidateSettingsAsync_WithValidSettings_ShouldReturnTrue()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new AppSettings
        {
            Language = "en-US",
            CustomProtocol = "browserselector",
            CloseAfterUrlRuleMatch = true
        };

        // Act
        var result = await libraryService.ValidateSettingsAsync(settings);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// 無効な言語を持つValidateSettingsAsyncがfalseを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task ValidateSettingsAsync_WithInvalidLanguage_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new AppSettings
        {
            Language = "invalid-language",
            CustomProtocol = "browserselector"
        };

        // Act
        var result = await libraryService.ValidateSettingsAsync(settings);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// 無効なプロトコルを持つValidateSettingsAsyncがfalseを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task ValidateSettingsAsync_WithInvalidProtocol_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new AppSettings
        {
            Language = "en-US",
            CustomProtocol = "invalid-protocol-with-numbers123"
        };

        // Act
        var result = await libraryService.ValidateSettingsAsync(settings);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// 有効な設定を持つValidateVisualSettingsAsyncがtrueを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task ValidateVisualSettingsAsync_WithValidSettings_ShouldReturnTrue()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new VisualSettings
        {
            InitialWindowWidth = 800,
            InitialWindowHeight = 600,
            BackgroundColor = System.Windows.Media.Colors.White
        };

        // Act
        var result = await libraryService.ValidateVisualSettingsAsync(settings);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// 無効な幅でValidateVisualSettingsAsyncがfalseを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task ValidateVisualSettingsAsync_WithInvalidWidth_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new VisualSettings
        {
            InitialWindowWidth = 200, // 最小値400未満
            InitialWindowHeight = 600
        };

        // Act
        var result = await libraryService.ValidateVisualSettingsAsync(settings);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// 無効な高さでValidateVisualSettingsAsyncがfalseを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task ValidateVisualSettingsAsync_WithInvalidHeight_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new VisualSettings
        {
            InitialWindowWidth = 800,
            InitialWindowHeight = 200 // 最小値300未満
        };

        // Act
        var result = await libraryService.ValidateVisualSettingsAsync(settings);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// 同じグラデーション色でValidateVisualSettingsAsyncがfalseを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task ValidateVisualSettingsAsync_WithSameGradientColors_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        var color = System.Windows.Media.Colors.Blue;
        var settings = new VisualSettings
        {
            InitialWindowWidth = 800,
            InitialWindowHeight = 600,
            UseBackgroundGradient = true,
            GradientStartColor = color,
            GradientEndColor = color // 同じ色
        };

        // Act
        var result = await libraryService.ValidateVisualSettingsAsync(settings);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// 有効なルールを持つValidateUrlRuleAsyncがtrueを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task ValidateUrlRuleAsync_WithValidRule_ShouldReturnTrue()
    {
        // Arrange
        var libraryService = new LibraryService();
        var rule = new UrlRule
        {
            Pattern = ".*\\.google\\.com.*",
            BrowserName = "Google Chrome",
            Priority = 50,
            IsEnabled = true
        };

        // Act
        var result = await libraryService.ValidateUrlRuleAsync(rule);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// 無効なパターンを持つValidateUrlRuleAsyncがfalseを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task ValidateUrlRuleAsync_WithInvalidPattern_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        var rule = new UrlRule
        {
            Pattern = "[invalid-regex", // 無効な正規表現
            BrowserName = "Google Chrome",
            Priority = 50
        };

        // Act
        var result = await libraryService.ValidateUrlRuleAsync(rule);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// 無効な優先度を持つValidateUrlRuleAsyncがfalseを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task ValidateUrlRuleAsync_WithInvalidPriority_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        var rule = new UrlRule
        {
            Pattern = ".*\\.google\\.com.*",
            BrowserName = "Google Chrome",
            Priority = 150 // 最大値100を超える
        };

        // Act
        var result = await libraryService.ValidateUrlRuleAsync(rule);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// 有効な設定でValidateLogSettingsAsyncがtrueを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task ValidateLogSettingsAsync_WithValidSettings_ShouldReturnTrue()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new LogSettings
        {
            LogLevel = LogLevel.Information,
            MaxLogFileSize = 10,
            LogRetentionDays = 30,
            EnableFileLogging = true
        };

        // Act
        var result = await libraryService.ValidateLogSettingsAsync(settings);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// 無効なファイルサイズでValidateLogSettingsAsyncがfalseを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task ValidateLogSettingsAsync_WithInvalidFileSize_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new LogSettings
        {
            LogLevel = LogLevel.Information,
            MaxLogFileSize = 2000, // 最大値1000を超える
            LogRetentionDays = 30
        };

        // Act
        var result = await libraryService.ValidateLogSettingsAsync(settings);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// 無効な保持日数でValidateLogSettingsAsyncがfalseを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task ValidateLogSettingsAsync_WithInvalidRetentionDays_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new LogSettings
        {
            LogLevel = LogLevel.Information,
            MaxLogFileSize = 10,
            LogRetentionDays = 500 // 最大値365を超える
        };

        // Act
        var result = await libraryService.ValidateLogSettingsAsync(settings);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ValidateBrowserAsync_WithNullBrowser_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        Browser browser = null!;

        // Act
        var result = await libraryService.ValidateBrowserAsync(browser);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// null設定を持つValidateSettingsAsyncがfalseを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task ValidateSettingsAsync_WithNullSettings_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        AppSettings settings = null!;

        // Act
        var result = await libraryService.ValidateSettingsAsync(settings);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// null設定を持つValidateVisualSettingsAsyncがfalseを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task ValidateVisualSettingsAsync_WithNullSettings_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        VisualSettings settings = null!;

        // Act
        var result = await libraryService.ValidateVisualSettingsAsync(settings);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// nullルールを持つValidateUrlRuleAsyncがfalseを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task ValidateUrlRuleAsync_WithNullRule_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        UrlRule rule = null!;

        // Act
        var result = await libraryService.ValidateUrlRuleAsync(rule);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// null設定でValidateLogSettingsAsyncがfalseを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task ValidateLogSettingsAsync_WithNullSettings_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        LogSettings settings = null!;

        // Act
        var result = await libraryService.ValidateLogSettingsAsync(settings);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ValidateBrowserAsync_WithNonExistentFile_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        var browser = new Browser
        {
            Name = "Non-existent Browser",
            ExecutablePath = @"C:\NonExistent\Browser.exe",
            Type = BrowserType.Custom
        };

        // Act
        var result = await libraryService.ValidateBrowserAsync(browser);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// null実行パスでValidateBrowserAsyncがfalseを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task ValidateBrowserAsync_WithNullExecutablePath_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        var browser = new Browser
        {
            Name = "Test Browser",
            ExecutablePath = null!,
            Type = BrowserType.Custom
        };

        // Act
        var result = await libraryService.ValidateBrowserAsync(browser);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// 空白名でValidateBrowserAsyncがfalseを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task ValidateBrowserAsync_WithWhitespaceName_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        var browser = new Browser
        {
            Name = "   ",
            ExecutablePath = @"C:\Program Files\Test\test.exe",
            Type = BrowserType.Custom
        };

        // Act
        var result = await libraryService.ValidateBrowserAsync(browser);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// 空白URLでNormalizeUrlAsyncが空文字列を返すことをテスト.
    /// </summary>
    [Fact]
    public async Task NormalizeUrlAsync_WithWhitespaceUrl_ShouldReturnEmptyString()
    {
        // Arrange
        var libraryService = new LibraryService();
        string inputUrl = "   ";

        // Act
        var result = await libraryService.NormalizeUrlAsync(inputUrl);

        // Assert
        result.Should().Be("");
    }

    /// <summary>
    /// FTPURLでNormalizeUrlAsyncが同じURLを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task NormalizeUrlAsync_WithFtpUrl_ShouldReturnSameUrl()
    {
        // Arrange
        var libraryService = new LibraryService();
        string inputUrl = "ftp://example.com";

        // Act
        var result = await libraryService.NormalizeUrlAsync(inputUrl);

        // Assert
        result.Should().Be(inputUrl);
    }

    /// <summary>
    /// ファイルURLでNormalizeUrlAsyncが同じURLを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task NormalizeUrlAsync_WithFileUrl_ShouldReturnSameUrl()
    {
        // Arrange
        var libraryService = new LibraryService();
        string inputUrl = "file:///C:/test.html";

        // Act
        var result = await libraryService.NormalizeUrlAsync(inputUrl);

        // Assert
        result.Should().Be(inputUrl);
    }

    /// <summary>
    /// 空の言語でValidateSettingsAsyncがfalseを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task ValidateSettingsAsync_WithEmptyLanguage_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new AppSettings
        {
            Language = "",
            CustomProtocol = "browserselector"
        };

        // Act
        var result = await libraryService.ValidateSettingsAsync(settings);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// 空白言語でValidateSettingsAsyncがfalseを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task ValidateSettingsAsync_WithWhitespaceLanguage_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new AppSettings
        {
            Language = "   ",
            CustomProtocol = "browserselector"
        };

        // Act
        var result = await libraryService.ValidateSettingsAsync(settings);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// 有効な言語でValidateSettingsAsyncがtrueを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task ValidateSettingsAsync_WithValidLanguage_ShouldReturnTrue()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new AppSettings
        {
            Language = "ja-JP",
            CustomProtocol = "browserselector"
        };

        // Act
        var result = await libraryService.ValidateSettingsAsync(settings);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// 空のプロトコルでValidateSettingsAsyncがtrueを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task ValidateSettingsAsync_WithEmptyProtocol_ShouldReturnTrue()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new AppSettings
        {
            Language = "en-US",
            CustomProtocol = ""
        };

        // Act
        var result = await libraryService.ValidateSettingsAsync(settings);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// nullプロトコルでValidateSettingsAsyncがtrueを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task ValidateSettingsAsync_WithNullProtocol_ShouldReturnTrue()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new AppSettings
        {
            Language = "en-US",
            CustomProtocol = null!
        };

        // Act
        var result = await libraryService.ValidateSettingsAsync(settings);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// 有効なグラデーション色でValidateVisualSettingsAsyncがtrueを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task ValidateVisualSettingsAsync_WithValidGradientColors_ShouldReturnTrue()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new VisualSettings
        {
            InitialWindowWidth = 800,
            InitialWindowHeight = 600,
            UseBackgroundGradient = true,
            GradientStartColor = System.Windows.Media.Colors.Blue,
            GradientEndColor = System.Windows.Media.Colors.Red
        };

        // Act
        var result = await libraryService.ValidateVisualSettingsAsync(settings);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// 無効なグラデーションでValidateVisualSettingsAsyncがtrueを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task ValidateVisualSettingsAsync_WithDisabledGradient_ShouldReturnTrue()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new VisualSettings
        {
            InitialWindowWidth = 800,
            InitialWindowHeight = 600,
            UseBackgroundGradient = false,
            GradientStartColor = System.Windows.Media.Colors.Blue,
            GradientEndColor = System.Windows.Media.Colors.Blue // 同じ色でもグラデーション無効ならOK
        };

        // Act
        var result = await libraryService.ValidateVisualSettingsAsync(settings);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ValidateVisualSettingsAsync_WithMaximumWidth_ShouldReturnTrue()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new VisualSettings
        {
            InitialWindowWidth = 2000,
            InitialWindowHeight = 600
        };

        // Act
        var result = await libraryService.ValidateVisualSettingsAsync(settings);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ValidateVisualSettingsAsync_WithMaximumHeight_ShouldReturnTrue()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new VisualSettings
        {
            InitialWindowWidth = 800,
            InitialWindowHeight = 1500
        };

        // Act
        var result = await libraryService.ValidateVisualSettingsAsync(settings);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ValidateVisualSettingsAsync_WithMinimumWidth_ShouldReturnTrue()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new VisualSettings
        {
            InitialWindowWidth = 400,
            InitialWindowHeight = 600
        };

        // Act
        var result = await libraryService.ValidateVisualSettingsAsync(settings);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ValidateVisualSettingsAsync_WithMinimumHeight_ShouldReturnTrue()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new VisualSettings
        {
            InitialWindowWidth = 800,
            InitialWindowHeight = 300
        };

        // Act
        var result = await libraryService.ValidateVisualSettingsAsync(settings);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// 空のブラウザ名を持つValidateUrlRuleAsyncがfalseを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task ValidateUrlRuleAsync_WithEmptyBrowserName_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        var rule = new UrlRule
        {
            Pattern = ".*\\.google\\.com.*",
            BrowserName = "",
            Priority = 50
        };

        // Act
        var result = await libraryService.ValidateUrlRuleAsync(rule);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// 空白ブラウザ名を持つValidateUrlRuleAsyncがfalseを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task ValidateUrlRuleAsync_WithWhitespaceBrowserName_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        var rule = new UrlRule
        {
            Pattern = ".*\\.google\\.com.*",
            BrowserName = "   ",
            Priority = 50
        };

        // Act
        var result = await libraryService.ValidateUrlRuleAsync(rule);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// 最小優先度を持つValidateUrlRuleAsyncがtrueを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task ValidateUrlRuleAsync_WithMinimumPriority_ShouldReturnTrue()
    {
        // Arrange
        var libraryService = new LibraryService();
        var rule = new UrlRule
        {
            Pattern = ".*\\.google\\.com.*",
            BrowserName = "Google Chrome",
            Priority = 1
        };

        // Act
        var result = await libraryService.ValidateUrlRuleAsync(rule);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// 最大優先度を持つValidateUrlRuleAsyncがtrueを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task ValidateUrlRuleAsync_WithMaximumPriority_ShouldReturnTrue()
    {
        // Arrange
        var libraryService = new LibraryService();
        var rule = new UrlRule
        {
            Pattern = ".*\\.google\\.com.*",
            BrowserName = "Google Chrome",
            Priority = 100
        };

        // Act
        var result = await libraryService.ValidateUrlRuleAsync(rule);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// ゼロ優先度を持つValidateUrlRuleAsyncがfalseを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task ValidateUrlRuleAsync_WithZeroPriority_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        var rule = new UrlRule
        {
            Pattern = ".*\\.google\\.com.*",
            BrowserName = "Google Chrome",
            Priority = 0
        };

        // Act
        var result = await libraryService.ValidateUrlRuleAsync(rule);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// 最小ファイルサイズでValidateLogSettingsAsyncがtrueを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task ValidateLogSettingsAsync_WithMinimumFileSize_ShouldReturnTrue()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new LogSettings
        {
            LogLevel = LogLevel.Information,
            MaxLogFileSize = 1,
            LogRetentionDays = 30
        };

        // Act
        var result = await libraryService.ValidateLogSettingsAsync(settings);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// 最大ファイルサイズでValidateLogSettingsAsyncがtrueを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task ValidateLogSettingsAsync_WithMaximumFileSize_ShouldReturnTrue()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new LogSettings
        {
            LogLevel = LogLevel.Information,
            MaxLogFileSize = 1000,
            LogRetentionDays = 30
        };

        // Act
        var result = await libraryService.ValidateLogSettingsAsync(settings);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ValidateLogSettingsAsync_WithMinimumRetentionDays_ShouldReturnTrue()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new LogSettings
        {
            LogLevel = LogLevel.Information,
            MaxLogFileSize = 10,
            LogRetentionDays = 1
        };

        // Act
        var result = await libraryService.ValidateLogSettingsAsync(settings);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task ValidateLogSettingsAsync_WithMaximumRetentionDays_ShouldReturnTrue()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new LogSettings
        {
            LogLevel = LogLevel.Information,
            MaxLogFileSize = 10,
            LogRetentionDays = 365
        };

        // Act
        var result = await libraryService.ValidateLogSettingsAsync(settings);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// ゼロファイルサイズでValidateLogSettingsAsyncがfalseを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task ValidateLogSettingsAsync_WithZeroFileSize_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new LogSettings
        {
            LogLevel = LogLevel.Information,
            MaxLogFileSize = 0,
            LogRetentionDays = 30
        };

        // Act
        var result = await libraryService.ValidateLogSettingsAsync(settings);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// ゼロ保持日数でValidateLogSettingsAsyncがfalseを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task ValidateLogSettingsAsync_WithZeroRetentionDays_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new LogSettings
        {
            LogLevel = LogLevel.Information,
            MaxLogFileSize = 10,
            LogRetentionDays = 0
        };

        // Act
        var result = await libraryService.ValidateLogSettingsAsync(settings);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// TraceログレベルでValidateLogSettingsAsyncがtrueを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task ValidateLogSettingsAsync_WithTraceLogLevel_ShouldReturnTrue()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new LogSettings
        {
            LogLevel = LogLevel.Trace,
            MaxLogFileSize = 10,
            LogRetentionDays = 30
        };

        // Act
        var result = await libraryService.ValidateLogSettingsAsync(settings);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// CriticalログレベルでValidateLogSettingsAsyncがtrueを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task ValidateLogSettingsAsync_WithCriticalLogLevel_ShouldReturnTrue()
    {
        // Arrange
        var libraryService = new LibraryService();
        var settings = new LogSettings
        {
            LogLevel = LogLevel.Critical,
            MaxLogFileSize = 10,
            LogRetentionDays = 30
        };

        // Act
        var result = await libraryService.ValidateLogSettingsAsync(settings);

        // Assert
        result.Should().BeTrue();
    }

    // 追加のテストケース - 未カバー部分をカバー
    /// <summary>
    /// 存在しない実行ファイルパスを持つValidateBrowserAsyncがfalseを返すことを確認するテスト.
    /// </summary>
    [Fact]
    public async Task ValidateBrowserAsync_WithNonExistentExecutablePath_ShouldReturnFalse()
    {
        // Arrange
        var libraryService = new LibraryService();
        var browser = new Browser
        {
            Name = "Test Browser",
            ExecutablePath = @"C:\NonExistent\Browser.exe",
            Type = BrowserType.Chrome
        };

        // Act
        var result = await libraryService.ValidateBrowserAsync(browser);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// FTPURLでNormalizeUrlAsyncが元のURLを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task NormalizeUrlAsync_WithFtpUrl_ShouldReturnOriginalUrl()
    {
        // Arrange
        var libraryService = new LibraryService();
        var url = "ftp://example.com/file.txt";

        // Act
        var result = await libraryService.NormalizeUrlAsync(url);

        // Assert
        result.Should().Be(url);
    }

    /// <summary>
    /// ファイルURLでNormalizeUrlAsyncが元のURLを返すことをテスト.
    /// </summary>
    [Fact]
    public async Task NormalizeUrlAsync_WithFileUrl_ShouldReturnOriginalUrl()
    {
        // Arrange
        var libraryService = new LibraryService();
        var url = "file:///C:/path/to/file.txt";

        // Act
        var result = await libraryService.NormalizeUrlAsync(url);

        // Assert
        result.Should().Be(url);
    }
}
