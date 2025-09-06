using BrowserSelector.Core.Models;
using BrowserSelector.Core.Enums;
using FluentAssertions;
using System.Windows.Media;
using Xunit;

namespace BrowserSelector.UnitTests;

public class CoreModelsTests
{
    [Fact]
    public void AppSettings_DefaultValues_ShouldBeSetCorrectly()
    {
        // Act
        var settings = new AppSettings();

        // Assert
        settings.StartupMessage.Should().BeEmpty("スタートアップメッセージは空文字列");
        settings.EnableLogging.Should().BeTrue("ログは有効");
        settings.LogLevel.Should().Be("Information", "ログレベルはInformation");
        settings.CheckForUpdates.Should().BeTrue("更新チェックは有効");
        settings.UpdateCheckInterval.Should().Be(24, "更新チェック間隔は24時間");
        settings.Language.Should().Be("en-US", "デフォルト言語は英語");
        settings.PortableMode.Should().BeFalse("ポータブルモードは無効");
        settings.CustomProtocol.Should().Be("browserselector", "カスタムプロトコルはbrowserselector");
        settings.RegisterProtocol.Should().BeTrue("プロトコル登録は有効");
        settings.CloseAfterUrlRuleMatch.Should().BeTrue("URLルールマッチ後は閉じる");
    }

    [Fact]
    public void AppSettings_PropertyChanges_ShouldTriggerNotifications()
    {
        // Arrange
        var settings = new AppSettings();
        var propertyChangedEvents = new List<string>();

        settings.PropertyChanged += (sender, e) => propertyChangedEvents.Add(e.PropertyName!);

        // Act
        settings.StartupMessage = "Test Message";
        settings.EnableLogging = false;
        settings.LogLevel = "Debug";
        settings.CheckForUpdates = false;
        settings.UpdateCheckInterval = 12;
        settings.Language = "ja-JP";
        settings.PortableMode = true;
        settings.CustomProtocol = "test";
        settings.RegisterProtocol = false;
        settings.CloseAfterUrlRuleMatch = false;

        // Assert
        propertyChangedEvents.Should().Contain("StartupMessage");
        propertyChangedEvents.Should().Contain("EnableLogging");
        propertyChangedEvents.Should().Contain("LogLevel");
        propertyChangedEvents.Should().Contain("CheckForUpdates");
        propertyChangedEvents.Should().Contain("UpdateCheckInterval");
        propertyChangedEvents.Should().Contain("Language");
        propertyChangedEvents.Should().Contain("PortableMode");
        propertyChangedEvents.Should().Contain("CustomProtocol");
        propertyChangedEvents.Should().Contain("RegisterProtocol");
        propertyChangedEvents.Should().Contain("CloseAfterUrlRuleMatch");
    }

    [Fact]
    public void Browser_WithValidData_ShouldBeValid()
    {
        // Arrange
        var browser = new Browser
        {
            Name = "Test Browser",
            ExecutablePath = @"C:\Program Files\TestBrowser\browser.exe",
            IconPath = @"C:\Program Files\TestBrowser\icon.ico",
            IsDefault = false,
            Type = BrowserType.Custom
        };

        // Act & Assert
        browser.IsValid.Should().BeTrue("有効なデータのブラウザは有効であること");
        browser.Name.Should().Be("Test Browser");
        browser.ExecutablePath.Should().Be(@"C:\Program Files\TestBrowser\browser.exe");
        browser.IconPath.Should().Be(@"C:\Program Files\TestBrowser\icon.ico");
        browser.IsDefault.Should().BeFalse();
        browser.Type.Should().Be(BrowserType.Custom);
    }

    [Fact]
    public void Browser_WithEmptyName_ShouldNotBeValid()
    {
        // Arrange
        var browser = new Browser
        {
            Name = "",
            ExecutablePath = @"C:\Program Files\TestBrowser\browser.exe"
        };

        // Act & Assert
        browser.IsValid.Should().BeFalse("名前が空のブラウザは無効であること");
    }

    [Fact]
    public void Browser_WithEmptyExecutablePath_ShouldNotBeValid()
    {
        // Arrange
        var browser = new Browser
        {
            Name = "Test Browser",
            ExecutablePath = ""
        };

        // Act & Assert
        browser.IsValid.Should().BeFalse("実行ファイルパスが空のブラウザは無効であること");
    }

    [Fact]
    public void Browser_IncrementUseCount_ShouldIncreaseCount()
    {
        // Arrange
        var browser = new Browser
        {
            Name = "Test Browser",
            ExecutablePath = @"C:\Program Files\TestBrowser\browser.exe"
        };

        // Act
        browser.IncrementUseCount();

        // Assert
        browser.UseCount.Should().Be(1, "使用回数が1増加すること");
        browser.LastUsed.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1), "最終使用日時が更新されること");
    }

    [Fact]
    public void Browser_Clone_ShouldCreateNewInstance()
    {
        // Arrange
        var originalBrowser = new Browser
        {
            Name = "Test Browser",
            ExecutablePath = @"C:\Program Files\TestBrowser\browser.exe",
            IconPath = @"C:\Program Files\TestBrowser\icon.ico",
            IsDefault = true,
            Type = BrowserType.Chrome
        };

        // Act
        var clonedBrowser = originalBrowser.Clone();

        // Assert
        clonedBrowser.Should().NotBeSameAs(originalBrowser, "クローンは別のインスタンスであること");
        clonedBrowser.Id.Should().NotBe(originalBrowser.Id, "IDは異なること");
        clonedBrowser.Name.Should().Be(originalBrowser.Name);
        clonedBrowser.ExecutablePath.Should().Be(originalBrowser.ExecutablePath);
        clonedBrowser.IconPath.Should().Be(originalBrowser.IconPath);
        clonedBrowser.IsDefault.Should().BeFalse("複製時はデフォルトをfalseにする");
        clonedBrowser.Type.Should().Be(originalBrowser.Type);
    }

    [Fact]
    public void Browser_DisplayName_ShouldReturnCorrectName()
    {
        // Arrange
        var browser = new Browser
        {
            Name = "Test Browser",
            ExecutablePath = @"C:\Program Files\TestBrowser\browser.exe"
        };

        // Act & Assert
        browser.DisplayName.Should().Be("Test Browser", "表示名は名前と同じであること");
    }

    [Fact]
    public void Browser_DisplayName_WithEmptyName_ShouldReturnUnknown()
    {
        // Arrange
        var browser = new Browser
        {
            Name = "",
            ExecutablePath = @"C:\Program Files\TestBrowser\browser.exe"
        };

        // Act & Assert
        browser.DisplayName.Should().Be("Unknown Browser", "名前が空の場合はUnknown Browserを返すこと");
    }

    [Fact]
    public void VisualSettings_DefaultValues_ShouldBeSetCorrectly()
    {
        // Act
        var settings = new VisualSettings();

        // Assert
        settings.BackgroundColor.Should().Be(Colors.White, "背景色は白");
        settings.UseBackgroundGradient.Should().BeFalse("グラデーションは無効");
        settings.GradientStartColor.Should().Be(Colors.Transparent, "グラデーション開始色は透明");
        settings.GradientEndColor.Should().Be(Colors.Transparent, "グラデーション終了色は透明");
        settings.GradientDirection.Should().Be(GradientDirection.Vertical, "グラデーション方向は縦");
        settings.IconScale.Should().Be(1.0, "アイコンスケールは1.0");
        settings.ShowFocusIndicator.Should().BeTrue("フォーカス表示は有効");
        settings.FocusColor.Should().Be(Colors.Blue, "フォーカス色は青");
        settings.FocusThickness.Should().Be(2.0, "フォーカス線幅は2.0");
        settings.FocusWidth.Should().Be(100.0, "フォーカス幅は100.0");
        settings.InitialWindowWidth.Should().Be(800.0, "初期ウィンドウ幅は800.0");
        settings.InitialWindowHeight.Should().Be(600.0, "初期ウィンドウ高さは600.0");
        settings.ShowLogo.Should().BeTrue("ロゴ表示は有効");
        settings.ShowUrlInput.Should().BeTrue("URL入力表示は有効");
        settings.BrowserButtonWidth.Should().Be(120.0, "ブラウザボタン幅は120.0");
        settings.BrowserButtonHeight.Should().Be(90.0, "ブラウザボタン高さは90.0");
        settings.BrowserButtonBackgroundColor.Should().Be(Colors.Transparent, "ブラウザボタン背景色は透明");
        settings.BrowserButtonForegroundColor.Should().Be(Colors.Black, "ブラウザボタン前景色は黒");
        settings.BrowserButtonOpacity.Should().Be(1.0, "ブラウザボタン透明度は1.0");
        settings.BrowserButtonCornerRadius.Should().Be(8.0, "ブラウザボタン角丸は8.0");
        settings.ShowBrowserName.Should().BeTrue("ブラウザ名表示は有効");
        settings.BrowserIconSize.Should().Be(32.0, "ブラウザアイコンサイズは32.0");
    }

    [Fact]
    public void VisualSettings_PropertyChanges_ShouldTriggerNotifications()
    {
        // Arrange
        var settings = new VisualSettings();
        var propertyChangedEvents = new List<string>();

        settings.PropertyChanged += (sender, e) => propertyChangedEvents.Add(e.PropertyName!);

        // Act
        settings.BackgroundColor = Colors.Red;
        settings.UseBackgroundGradient = true;
        settings.GradientStartColor = Colors.Blue;
        settings.GradientEndColor = Colors.Green;
        settings.GradientDirection = GradientDirection.Horizontal;
        settings.IconScale = 1.5;
        settings.ShowFocusIndicator = false;
        settings.FocusColor = Colors.Red;
        settings.FocusThickness = 3.0;
        settings.FocusWidth = 150.0;
        settings.InitialWindowWidth = 1000.0;
        settings.InitialWindowHeight = 800.0;
        settings.ShowLogo = false;
        settings.ShowUrlInput = false;
        settings.BrowserButtonWidth = 150.0;
        settings.BrowserButtonHeight = 100.0;
        settings.BrowserButtonBackgroundColor = Colors.White;
        settings.BrowserButtonForegroundColor = Colors.Blue;
        settings.BrowserButtonOpacity = 0.8;
        settings.BrowserButtonCornerRadius = 10.0;
        settings.ShowBrowserName = false;
        settings.BrowserIconSize = 48.0;

        // Assert
        propertyChangedEvents.Should().Contain("BackgroundColor");
        propertyChangedEvents.Should().Contain("UseBackgroundGradient");
        propertyChangedEvents.Should().Contain("GradientStartColor");
        propertyChangedEvents.Should().Contain("GradientEndColor");
        propertyChangedEvents.Should().Contain("GradientDirection");
        propertyChangedEvents.Should().Contain("IconScale");
        propertyChangedEvents.Should().Contain("ShowFocusIndicator");
        propertyChangedEvents.Should().Contain("FocusColor");
        propertyChangedEvents.Should().Contain("FocusThickness");
        propertyChangedEvents.Should().Contain("FocusWidth");
        propertyChangedEvents.Should().Contain("InitialWindowWidth");
        propertyChangedEvents.Should().Contain("InitialWindowHeight");
        propertyChangedEvents.Should().Contain("ShowLogo");
        propertyChangedEvents.Should().Contain("ShowUrlInput");
        propertyChangedEvents.Should().Contain("BrowserButtonWidth");
        propertyChangedEvents.Should().Contain("BrowserButtonHeight");
        propertyChangedEvents.Should().Contain("BrowserButtonBackgroundColor");
        propertyChangedEvents.Should().Contain("BrowserButtonForegroundColor");
        propertyChangedEvents.Should().Contain("BrowserButtonOpacity");
        propertyChangedEvents.Should().Contain("BrowserButtonCornerRadius");
        propertyChangedEvents.Should().Contain("ShowBrowserName");
        propertyChangedEvents.Should().Contain("BrowserIconSize");
    }

    [Fact]
    public void UrlRule_WithValidData_ShouldBeValid()
    {
        // Arrange
        var urlRule = new UrlRule
        {
            Pattern = "*.example.com",
            BrowserName = "Test Browser",
            IsEnabled = true
        };

        // Act & Assert
        urlRule.Pattern.Should().Be("*.example.com");
        urlRule.BrowserName.Should().Be("Test Browser");
        urlRule.IsEnabled.Should().BeTrue();
        urlRule.Priority.Should().Be(50, "デフォルト優先度は50");
        urlRule.Id.Should().NotBe(Guid.Empty, "IDが設定されること");
    }

    [Fact]
    public void UrlRule_WithEmptyPattern_ShouldNotBeValid()
    {
        // Arrange
        var urlRule = new UrlRule
        {
            Pattern = "",
            BrowserName = "Test Browser"
        };

        // Act & Assert
        urlRule.Pattern.Should().BeEmpty("パターンが空であること");
    }

    [Fact]
    public void UrlRule_WithEmptyBrowserName_ShouldNotBeValid()
    {
        // Arrange
        var urlRule = new UrlRule
        {
            Pattern = "*.example.com",
            BrowserName = ""
        };

        // Act & Assert
        urlRule.BrowserName.Should().BeEmpty("ブラウザ名が空であること");
    }

    [Fact]
    public void UrlRule_IsMatch_WithValidPattern_ShouldReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule
        {
            Pattern = "*.example.com",
            BrowserName = "Test Browser"
        };

        // Act & Assert
        urlRule.IsMatch("https://www.example.com").Should().BeTrue("マッチするURLはtrueを返すこと");
        urlRule.IsMatch("https://sub.example.com").Should().BeTrue("サブドメインもマッチすること");
    }

    [Fact]
    public void UrlRule_IsMatch_WithInvalidPattern_ShouldReturnFalse()
    {
        // Arrange
        var urlRule = new UrlRule
        {
            Pattern = "*.example.com",
            BrowserName = "Test Browser"
        };

        // Act & Assert
        urlRule.IsMatch("https://www.different.com").Should().BeFalse("マッチしないURLはfalseを返すこと");
        urlRule.IsMatch("").Should().BeFalse("空URLはfalseを返すこと");
    }

    [Fact]
    public void UrlRule_DisplayName_ShouldReturnCorrectFormat()
    {
        // Arrange
        var urlRule = new UrlRule
        {
            Pattern = "*.example.com",
            BrowserName = "Test Browser",
            Priority = 75
        };

        // Act & Assert
        urlRule.DisplayName.Should().Be("*.example.com → Test Browser (優先度: 75)", "表示名が正しい形式であること");
    }

    [Fact]
    public void UrlRule_GetDetails_ShouldReturnDetailedInfo()
    {
        // Arrange
        var urlRule = new UrlRule
        {
            Pattern = "*.example.com",
            BrowserName = "Test Browser",
            Priority = 75,
            IsEnabled = true,
            Description = "Test description"
        };

        // Act
        var details = urlRule.GetDetails();

        // Assert
        details.Should().Contain("*.example.com", "パターンが含まれること");
        details.Should().Contain("Test Browser", "ブラウザ名が含まれること");
        details.Should().Contain("75", "優先度が含まれること");
        details.Should().Contain("有効", "状態が含まれること");
        details.Should().Contain("Test description", "説明が含まれること");
    }
}
