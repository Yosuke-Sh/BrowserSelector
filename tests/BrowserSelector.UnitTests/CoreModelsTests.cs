using BrowserSelector.Core.Enums;
using BrowserSelector.Core.Models;
using FluentAssertions;
using System.Windows.Media;

namespace BrowserSelector.UnitTests;

public class CoreModelsTests
{
    [Fact]
    public void AppSettings_DefaultValues_ShouldBeSetCorrectly()
    {
        // Act
        AppSettings settings = new();

        // Assert
        _ = settings.EnableLogging.Should().BeTrue("ログは有効");
        _ = settings.LogLevel.Should().Be("Information", "ログレベルはInformation");
        _ = settings.CheckForUpdates.Should().BeTrue("更新チェックは有効");
        _ = settings.UpdateCheckInterval.Should().Be(24, "更新チェック間隔は24時間");
        _ = settings.Language.Should().Be("en-US", "デフォルト言語は英語");
        _ = settings.CustomProtocol.Should().Be("browserselector", "カスタムプロトコルはbrowserselector");
        _ = settings.RegisterProtocol.Should().BeTrue("プロトコル登録は有効");
        _ = settings.CloseAfterUrlRuleMatch.Should().BeTrue("URLルールマッチ後は閉じる");
    }

    [Fact]
    public void AppSettings_PropertyChanges_ShouldTriggerNotifications()
    {
        // Arrange
        AppSettings settings = new();
        List<string> propertyChangedEvents = [];

        settings.PropertyChanged += (sender, e) => propertyChangedEvents.Add(e.PropertyName!);

        // Act
        settings.EnableLogging = false;
        settings.LogLevel = "Debug";
        settings.CheckForUpdates = false;
        settings.UpdateCheckInterval = 12;
        settings.Language = "ja-JP";
        settings.CustomProtocol = "test";
        settings.RegisterProtocol = false;
        settings.CloseAfterUrlRuleMatch = false;

        // Assert
        _ = propertyChangedEvents.Should().Contain("EnableLogging");
        _ = propertyChangedEvents.Should().Contain("LogLevel");
        _ = propertyChangedEvents.Should().Contain("CheckForUpdates");
        _ = propertyChangedEvents.Should().Contain("UpdateCheckInterval");
        _ = propertyChangedEvents.Should().Contain("Language");
        _ = propertyChangedEvents.Should().Contain("CustomProtocol");
        _ = propertyChangedEvents.Should().Contain("RegisterProtocol");
        _ = propertyChangedEvents.Should().Contain("CloseAfterUrlRuleMatch");
    }

    [Fact]
    public void Browser_WithValidData_ShouldBeValid()
    {
        // Arrange
        Browser browser = new()
        {
            Name = "Test Browser",
            ExecutablePath = @"C:\Program Files\TestBrowser\browser.exe",
            IconPath = @"C:\Program Files\TestBrowser\icon.ico",
            IsDefault = false,
            Type = BrowserType.Custom
        };

        // Act & Assert
        _ = browser.IsValid.Should().BeTrue("有効なデータのブラウザは有効であること");
        _ = browser.Name.Should().Be("Test Browser");
        _ = browser.ExecutablePath.Should().Be(@"C:\Program Files\TestBrowser\browser.exe");
        _ = browser.IconPath.Should().Be(@"C:\Program Files\TestBrowser\icon.ico");
        _ = browser.IsDefault.Should().BeFalse();
        _ = browser.Type.Should().Be(BrowserType.Custom);
    }

    [Fact]
    public void Browser_WithEmptyName_ShouldNotBeValid()
    {
        // Arrange
        Browser browser = new()
        {
            Name = "",
            ExecutablePath = @"C:\Program Files\TestBrowser\browser.exe"
        };

        // Act & Assert
        _ = browser.IsValid.Should().BeFalse("名前が空のブラウザは無効であること");
    }

    [Fact]
    public void Browser_WithEmptyExecutablePath_ShouldNotBeValid()
    {
        // Arrange
        Browser browser = new()
        {
            Name = "Test Browser",
            ExecutablePath = ""
        };

        // Act & Assert
        _ = browser.IsValid.Should().BeFalse("実行ファイルパスが空のブラウザは無効であること");
    }

    [Fact]
    public void Browser_IncrementUseCount_ShouldIncreaseCount()
    {
        // Arrange
        Browser browser = new()
        {
            Name = "Test Browser",
            ExecutablePath = @"C:\Program Files\TestBrowser\browser.exe"
        };

        // Act
        browser.IncrementUseCount();

        // Assert
        _ = browser.UseCount.Should().Be(1, "使用回数が1増加すること");
        _ = browser.LastUsed.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1), "最終使用日時が更新されること");
    }

    [Fact]
    public void Browser_Clone_ShouldCreateNewInstance()
    {
        // Arrange
        Browser originalBrowser = new()
        {
            Name = "Test Browser",
            ExecutablePath = @"C:\Program Files\TestBrowser\browser.exe",
            IconPath = @"C:\Program Files\TestBrowser\icon.ico",
            IsDefault = true,
            Type = BrowserType.Chrome
        };

        // Act
        Browser clonedBrowser = originalBrowser.Clone();

        // Assert
        _ = clonedBrowser.Should().NotBeSameAs(originalBrowser, "クローンは別のインスタンスであること");
        _ = clonedBrowser.Id.Should().NotBe(originalBrowser.Id, "IDは異なること");
        _ = clonedBrowser.Name.Should().Be(originalBrowser.Name);
        _ = clonedBrowser.ExecutablePath.Should().Be(originalBrowser.ExecutablePath);
        _ = clonedBrowser.IconPath.Should().Be(originalBrowser.IconPath);
        _ = clonedBrowser.IsDefault.Should().BeFalse("複製時はデフォルトをfalseにする");
        _ = clonedBrowser.Type.Should().Be(originalBrowser.Type);
    }

    [Fact]
    public void Browser_DisplayName_ShouldReturnCorrectName()
    {
        // Arrange
        Browser browser = new()
        {
            Name = "Test Browser",
            ExecutablePath = @"C:\Program Files\TestBrowser\browser.exe"
        };

        // Act & Assert
        _ = browser.DisplayName.Should().Be("Test Browser", "表示名は名前と同じであること");
    }

    [Fact]
    public void Browser_DisplayName_WithEmptyName_ShouldReturnUnknown()
    {
        // Arrange
        Browser browser = new()
        {
            Name = "",
            ExecutablePath = @"C:\Program Files\TestBrowser\browser.exe"
        };

        // Act & Assert
        _ = browser.DisplayName.Should().Be("Unknown Browser", "名前が空の場合はUnknown Browserを返すこと");
    }

    [Fact]
    public void VisualSettings_DefaultValues_ShouldBeSetCorrectly()
    {
        // Act
        VisualSettings settings = new();

        // Assert
        _ = settings.BackgroundColor.Should().Be(Colors.White, "背景色は白");
        _ = settings.UseBackgroundGradient.Should().BeFalse("グラデーションは無効");
        _ = settings.GradientStartColor.Should().Be(Colors.Transparent, "グラデーション開始色は透明");
        _ = settings.GradientEndColor.Should().Be(Colors.Transparent, "グラデーション終了色は透明");
        _ = settings.GradientDirection.Should().Be(GradientDirection.Vertical, "グラデーション方向は縦");
        _ = settings.IconScale.Should().Be(1.0, "アイコンスケールは1.0");
        _ = settings.ShowFocusIndicator.Should().BeTrue("フォーカス表示は有効");
        _ = settings.FocusColor.Should().Be(Colors.Blue, "フォーカス色は青");
        _ = settings.FocusThickness.Should().Be(2.0, "フォーカス線幅は2.0");
        _ = settings.FocusWidth.Should().Be(100.0, "フォーカス幅は100.0");
        _ = settings.InitialWindowWidth.Should().Be(800.0, "初期ウィンドウ幅は800.0");
        _ = settings.InitialWindowHeight.Should().Be(600.0, "初期ウィンドウ高さは600.0");
        _ = settings.ShowLogo.Should().BeTrue("ロゴ表示は有効");
        _ = settings.ShowUrlInput.Should().BeTrue("URL入力表示は有効");
        _ = settings.BrowserButtonWidth.Should().Be(120.0, "ブラウザボタン幅は120.0");
        _ = settings.BrowserButtonHeight.Should().Be(90.0, "ブラウザボタン高さは90.0");
        _ = settings.BrowserButtonBackgroundColor.Should().Be(Colors.Transparent, "ブラウザボタン背景色は透明");
        _ = settings.BrowserButtonForegroundColor.Should().Be(Colors.Black, "ブラウザボタン前景色は黒");
        _ = settings.BrowserButtonOpacity.Should().Be(1.0, "ブラウザボタン透明度は1.0");
        _ = settings.BrowserButtonCornerRadius.Should().Be(8.0, "ブラウザボタン角丸は8.0");
        _ = settings.ShowBrowserName.Should().BeTrue("ブラウザ名表示は有効");
        _ = settings.BrowserIconSize.Should().Be(32.0, "ブラウザアイコンサイズは32.0");
    }

    [Fact]
    public void VisualSettings_PropertyChanges_ShouldTriggerNotifications()
    {
        // Arrange
        VisualSettings settings = new();
        List<string> propertyChangedEvents = [];

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
        _ = propertyChangedEvents.Should().Contain("BackgroundColor");
        _ = propertyChangedEvents.Should().Contain("UseBackgroundGradient");
        _ = propertyChangedEvents.Should().Contain("GradientStartColor");
        _ = propertyChangedEvents.Should().Contain("GradientEndColor");
        _ = propertyChangedEvents.Should().Contain("GradientDirection");
        _ = propertyChangedEvents.Should().Contain("IconScale");
        _ = propertyChangedEvents.Should().Contain("ShowFocusIndicator");
        _ = propertyChangedEvents.Should().Contain("FocusColor");
        _ = propertyChangedEvents.Should().Contain("FocusThickness");
        _ = propertyChangedEvents.Should().Contain("FocusWidth");
        _ = propertyChangedEvents.Should().Contain("InitialWindowWidth");
        _ = propertyChangedEvents.Should().Contain("InitialWindowHeight");
        _ = propertyChangedEvents.Should().Contain("ShowLogo");
        _ = propertyChangedEvents.Should().Contain("ShowUrlInput");
        _ = propertyChangedEvents.Should().Contain("BrowserButtonWidth");
        _ = propertyChangedEvents.Should().Contain("BrowserButtonHeight");
        _ = propertyChangedEvents.Should().Contain("BrowserButtonBackgroundColor");
        _ = propertyChangedEvents.Should().Contain("BrowserButtonForegroundColor");
        _ = propertyChangedEvents.Should().Contain("BrowserButtonOpacity");
        _ = propertyChangedEvents.Should().Contain("BrowserButtonCornerRadius");
        _ = propertyChangedEvents.Should().Contain("ShowBrowserName");
        _ = propertyChangedEvents.Should().Contain("BrowserIconSize");
    }

    [Fact]
    public void UrlRule_WithValidData_ShouldBeValid()
    {
        // Arrange
        UrlRule urlRule = new()
        {
            Pattern = "*.example.com",
            BrowserName = "Test Browser",
            IsEnabled = true
        };

        // Act & Assert
        _ = urlRule.Pattern.Should().Be("*.example.com");
        _ = urlRule.BrowserName.Should().Be("Test Browser");
        _ = urlRule.IsEnabled.Should().BeTrue();
        _ = urlRule.Priority.Should().Be(50, "デフォルト優先度は50");
        _ = urlRule.Id.Should().NotBe(Guid.Empty, "IDが設定されること");
    }

    [Fact]
    public void UrlRule_WithEmptyPattern_ShouldNotBeValid()
    {
        // Arrange
        UrlRule urlRule = new()
        {
            Pattern = "",
            BrowserName = "Test Browser"
        };

        // Act & Assert
        _ = urlRule.Pattern.Should().BeEmpty("パターンが空であること");
    }

    [Fact]
    public void UrlRule_WithEmptyBrowserName_ShouldNotBeValid()
    {
        // Arrange
        UrlRule urlRule = new()
        {
            Pattern = "*.example.com",
            BrowserName = ""
        };

        // Act & Assert
        _ = urlRule.BrowserName.Should().BeEmpty("ブラウザ名が空であること");
    }

    [Fact]
    public void UrlRule_IsMatch_WithValidPattern_ShouldReturnTrue()
    {
        // Arrange
        UrlRule urlRule = new()
        {
            Pattern = "*.example.com",
            BrowserName = "Test Browser"
        };

        // Act & Assert
        _ = urlRule.IsMatch(new Uri("https://www.example.com")).Should().BeTrue("マッチするURLはtrueを返すこと");
        _ = urlRule.IsMatch(new Uri("https://sub.example.com")).Should().BeTrue("サブドメインもマッチすること");
    }

    [Fact]
    public void UrlRule_IsMatch_WithInvalidPattern_ShouldReturnFalse()
    {
        // Arrange
        UrlRule urlRule = new()
        {
            Pattern = "*.example.com",
            BrowserName = "Test Browser"
        };

        // Act & Assert
        _ = urlRule.IsMatch(new Uri("https://www.different.com")).Should().BeFalse("マッチしないURLはfalseを返すこと");
        _ = urlRule.IsMatch(new Uri("https://www.other.com")).Should().BeFalse("マッチしないURLはfalseを返すこと");
    }

    [Fact]
    public void UrlRule_DisplayName_ShouldReturnCorrectFormat()
    {
        // Arrange
        UrlRule urlRule = new()
        {
            Pattern = "*.example.com",
            BrowserName = "Test Browser",
            Priority = 75
        };

        // Act & Assert
        _ = urlRule.DisplayName.Should().Be("*.example.com → Test Browser (優先度: 75)", "表示名が正しい形式であること");
    }

    [Fact]
    public void UrlRule_GetDetails_ShouldReturnDetailedInfo()
    {
        // Arrange
        UrlRule urlRule = new()
        {
            Pattern = "*.example.com",
            BrowserName = "Test Browser",
            Priority = 75,
            IsEnabled = true,
            Description = "Test description"
        };

        // Act
        string details = urlRule.GetDetails();

        // Assert
        _ = details.Should().Contain("*.example.com", "パターンが含まれること");
        _ = details.Should().Contain("Test Browser", "ブラウザ名が含まれること");
        _ = details.Should().Contain("75", "優先度が含まれること");
        _ = details.Should().Contain("有効", "状態が含まれること");
        _ = details.Should().Contain("Test description", "説明が含まれること");
    }
}
