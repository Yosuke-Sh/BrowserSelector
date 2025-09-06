using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.Services;
using BrowserSelector.Infrastructure.SystemIntegration;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BrowserSelector.UnitTests;

/// <summary>
/// Core・Infrastructureプロジェクトのカバレッジ向上のためのテスト
/// </summary>
public class CoreProjectCoverageTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Mock<IRegistryService> _mockRegistryService;
    private readonly Mock<ILogService> _mockLogService;

    public CoreProjectCoverageTests()
    {
        _mockRegistryService = new Mock<IRegistryService>();
        _mockLogService = new Mock<ILogService>();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(_mockRegistryService.Object);
        services.AddSingleton(_mockLogService.Object);
        services.AddSingleton<IBrowserService, BrowserService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IUrlService, UrlService>();
        services.AddSingleton<IUrlRuleService, UrlRuleService>();
        services.AddSingleton<ICustomLanguageService, CustomLanguageService>();
        
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void CoreModels_ShouldBeAccessible()
    {
        // Arrange & Act
        var appSettings = new AppSettings();
        var visualSettings = new VisualSettings();
        var browser = new Browser();
        var urlRule = new UrlRule();
        var customLanguageFile = new CustomLanguageFile();

        // Assert - これらがアクセス可能であることを確認
        appSettings.Should().NotBeNull();
        visualSettings.Should().NotBeNull();
        browser.Should().NotBeNull();
        urlRule.Should().NotBeNull();
        customLanguageFile.Should().NotBeNull();
    }

    [Fact]
    public void CoreServices_ShouldBeResolvable()
    {
        // Arrange & Act
        var browserService = _serviceProvider.GetService<IBrowserService>();
        var settingsService = _serviceProvider.GetService<ISettingsService>();
        var urlService = _serviceProvider.GetService<IUrlService>();
        var urlRuleService = _serviceProvider.GetService<IUrlRuleService>();
        var customLanguageService = _serviceProvider.GetService<ICustomLanguageService>();

        // Assert
        browserService.Should().NotBeNull();
        settingsService.Should().NotBeNull();
        urlService.Should().NotBeNull();
        urlRuleService.Should().NotBeNull();
        customLanguageService.Should().NotBeNull();
    }

    [Fact]
    public async Task InfrastructureServices_ShouldExecuteMethods()
    {
        // Arrange
        var browserService = _serviceProvider.GetRequiredService<IBrowserService>();
        var settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
        var urlService = _serviceProvider.GetRequiredService<IUrlService>();
        var urlRuleService = _serviceProvider.GetRequiredService<IUrlRuleService>();
        var customLanguageService = _serviceProvider.GetRequiredService<ICustomLanguageService>();

        // Act & Assert - 各サービスのメソッドを実行してカバレッジを向上
        var browsers = await browserService.DetectBrowsersAsync();
        browsers.Should().NotBeNull();

        var appSettings = await settingsService.LoadAppSettingsAsync();
        appSettings.Should().NotBeNull();

        var visualSettings = await settingsService.LoadVisualSettingsAsync();
        visualSettings.Should().NotBeNull();

        var normalizedUrl = await urlService.NormalizeUrlAsync("https://example.com");
        normalizedUrl.Should().NotBeNullOrEmpty();

        var isValidUrl = await urlService.ValidateUrlAsync("https://example.com");
        isValidUrl.Should().BeTrue();

        var domain = urlService.ExtractDomain("https://www.example.com/path");
        domain.Should().Be("www.example.com");

        var urlWithProtocol = urlService.AddProtocolIfNeeded("example.com");
        urlWithProtocol.Should().Be("https://example.com");

        var urlRules = await urlRuleService.GetAllRulesAsync();
        urlRules.Should().NotBeNull();

        var enabledRules = await urlRuleService.GetEnabledRulesAsync();
        enabledRules.Should().NotBeNull();

        var availableLanguages = await customLanguageService.GetAvailableLanguagesAsync();
        availableLanguages.Should().NotBeNull();

        var customLanguageFolder = customLanguageService.GetCustomLanguageFolder();
        customLanguageFolder.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void BrowserModel_ShouldHaveAllProperties()
    {
        // Arrange
        var browser = new Browser
        {
            Name = "Test Browser",
            ExecutablePath = @"C:\Program Files\TestBrowser\browser.exe",
            IconPath = @"C:\Program Files\TestBrowser\icon.ico",
            Arguments = "--test-arg",
            IsDefault = true,
            IsEnabled = true,
            DisplayOrder = 1,
            LastUsed = DateTime.Now,
            UseCount = 5,
            Type = BrowserType.Chrome
        };

        // Act & Assert
        browser.Name.Should().Be("Test Browser");
        browser.ExecutablePath.Should().Be(@"C:\Program Files\TestBrowser\browser.exe");
        browser.IconPath.Should().Be(@"C:\Program Files\TestBrowser\icon.ico");
        browser.Arguments.Should().Be("--test-arg");
        browser.IsDefault.Should().BeTrue();
        browser.IsEnabled.Should().BeTrue();
        browser.DisplayOrder.Should().Be(1);
        browser.LastUsed.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        browser.UseCount.Should().Be(5);
        browser.Type.Should().Be(BrowserType.Chrome);
        browser.IsValid.Should().BeTrue();
        browser.DisplayName.Should().Be("Test Browser");

        // メソッドの実行
        browser.IncrementUseCount();
        browser.UseCount.Should().Be(6);

        var clonedBrowser = browser.Clone();
        clonedBrowser.Should().NotBeSameAs(browser);
        clonedBrowser.Name.Should().Be(browser.Name);
    }

    [Fact]
    public void UrlRuleModel_ShouldHaveAllProperties()
    {
        // Arrange
        var urlRule = new UrlRule
        {
            Pattern = "*.example.com",
            BrowserName = "Test Browser",
            Priority = 75,
            IsEnabled = true,
            Description = "Test rule",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        // Act & Assert
        urlRule.Pattern.Should().Be("*.example.com");
        urlRule.BrowserName.Should().Be("Test Browser");
        urlRule.Priority.Should().Be(75);
        urlRule.IsEnabled.Should().BeTrue();
        urlRule.Description.Should().Be("Test rule");
        urlRule.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        urlRule.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        urlRule.Id.Should().NotBe(Guid.Empty);

        // メソッドの実行
        var isMatch = urlRule.IsMatch("https://www.example.com");
        isMatch.Should().BeTrue();

        var displayName = urlRule.DisplayName;
        displayName.Should().Contain("*.example.com");
        displayName.Should().Contain("Test Browser");

        var details = urlRule.GetDetails();
        details.Should().Contain("*.example.com");
        details.Should().Contain("Test Browser");
        details.Should().Contain("75");
    }

    [Fact]
    public void CustomLanguageFileModel_ShouldHaveAllProperties()
    {
        // Arrange
        var customLanguageFile = new CustomLanguageFile
        {
            CultureCode = "ja-JP",
            DisplayName = "日本語",
            Resources = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" }
            },
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Version = "1.1",
            Description = "Japanese language file",
            Author = "Test Author"
        };

        // Act & Assert
        customLanguageFile.CultureCode.Should().Be("ja-JP");
        customLanguageFile.DisplayName.Should().Be("日本語");
        customLanguageFile.Resources.Should().HaveCount(2);
        customLanguageFile.Resources["key1"].Should().Be("value1");
        customLanguageFile.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        customLanguageFile.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        customLanguageFile.Version.Should().Be("1.1");
        customLanguageFile.Description.Should().Be("Japanese language file");
        customLanguageFile.Author.Should().Be("Test Author");
    }

    [Fact]
    public void AppSettingsModel_ShouldHaveAllProperties()
    {
        // Arrange
        var appSettings = new AppSettings
        {
            StartupMessage = "Test startup message",
            EnableLogging = false,
            LogLevel = "Debug",
            CheckForUpdates = false,
            UpdateCheckInterval = 12,
            Language = "ja-JP",
            PortableMode = true,
            CustomProtocol = "test-protocol",
            RegisterProtocol = false,
            CloseAfterUrlRuleMatch = false
        };

        // Act & Assert
        appSettings.StartupMessage.Should().Be("Test startup message");
        appSettings.EnableLogging.Should().BeFalse();
        appSettings.LogLevel.Should().Be("Debug");
        appSettings.CheckForUpdates.Should().BeFalse();
        appSettings.UpdateCheckInterval.Should().Be(12);
        appSettings.Language.Should().Be("ja-JP");
        appSettings.PortableMode.Should().BeTrue();
        appSettings.CustomProtocol.Should().Be("test-protocol");
        appSettings.RegisterProtocol.Should().BeFalse();
        appSettings.CloseAfterUrlRuleMatch.Should().BeFalse();
    }

    [Fact]
    public void VisualSettingsModel_ShouldHaveAllProperties()
    {
        // Arrange
        var visualSettings = new VisualSettings
        {
            BackgroundColor = System.Windows.Media.Colors.Red,
            UseBackgroundGradient = true,
            GradientStartColor = System.Windows.Media.Colors.Blue,
            GradientEndColor = System.Windows.Media.Colors.Green,
            GradientDirection = BrowserSelector.Core.Enums.GradientDirection.Horizontal,
            IconScale = 1.5,
            ShowFocusIndicator = false,
            FocusColor = System.Windows.Media.Colors.Yellow,
            FocusThickness = 3.0,
            FocusWidth = 150.0,
            InitialWindowWidth = 1000.0,
            InitialWindowHeight = 800.0,
            ShowLogo = false,
            ShowUrlInput = false,
            BrowserButtonWidth = 150.0,
            BrowserButtonHeight = 100.0,
            BrowserButtonBackgroundColor = System.Windows.Media.Colors.White,
            BrowserButtonForegroundColor = System.Windows.Media.Colors.Blue,
            BrowserButtonOpacity = 0.8,
            BrowserButtonCornerRadius = 10.0,
            ShowBrowserName = false,
            BrowserIconSize = 48.0
        };

        // Act & Assert
        visualSettings.BackgroundColor.Should().Be(System.Windows.Media.Colors.Red);
        visualSettings.UseBackgroundGradient.Should().BeTrue();
        visualSettings.GradientStartColor.Should().Be(System.Windows.Media.Colors.Blue);
        visualSettings.GradientEndColor.Should().Be(System.Windows.Media.Colors.Green);
        visualSettings.GradientDirection.Should().Be(BrowserSelector.Core.Enums.GradientDirection.Horizontal);
        visualSettings.IconScale.Should().Be(1.5);
        visualSettings.ShowFocusIndicator.Should().BeFalse();
        visualSettings.FocusColor.Should().Be(System.Windows.Media.Colors.Yellow);
        visualSettings.FocusThickness.Should().Be(3.0);
        visualSettings.FocusWidth.Should().Be(150.0);
        visualSettings.InitialWindowWidth.Should().Be(1000.0);
        visualSettings.InitialWindowHeight.Should().Be(800.0);
        visualSettings.ShowLogo.Should().BeFalse();
        visualSettings.ShowUrlInput.Should().BeFalse();
        visualSettings.BrowserButtonWidth.Should().Be(150.0);
        visualSettings.BrowserButtonHeight.Should().Be(100.0);
        visualSettings.BrowserButtonBackgroundColor.Should().Be(System.Windows.Media.Colors.White);
        visualSettings.BrowserButtonForegroundColor.Should().Be(System.Windows.Media.Colors.Blue);
        visualSettings.BrowserButtonOpacity.Should().Be(0.8);
        visualSettings.BrowserButtonCornerRadius.Should().Be(10.0);
        visualSettings.ShowBrowserName.Should().BeFalse();
        visualSettings.BrowserIconSize.Should().Be(48.0);
    }

    [Fact]
    public async Task InfrastructureServices_ShouldHandleEdgeCases()
    {
        // Arrange
        var browserService = _serviceProvider.GetRequiredService<IBrowserService>();
        var settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
        var urlService = _serviceProvider.GetRequiredService<IUrlService>();
        var urlRuleService = _serviceProvider.GetRequiredService<IUrlRuleService>();
        var customLanguageService = _serviceProvider.GetRequiredService<ICustomLanguageService>();

        // Act & Assert - エッジケースのテスト
        var emptyUrl = await urlService.NormalizeUrlAsync("");
        emptyUrl.Should().BeEmpty();

        var whitespaceUrl = await urlService.NormalizeUrlAsync("   ");
        whitespaceUrl.Should().BeEmpty();

        var invalidUrl = await urlService.ValidateUrlAsync("invalid-url");
        invalidUrl.Should().BeFalse();

        var emptyDomain = urlService.ExtractDomain("");
        emptyDomain.Should().BeEmpty();

        var urlWithProtocol = urlService.AddProtocolIfNeeded("http://example.com");
        urlWithProtocol.Should().Be("http://example.com");

        // 設定のリセット
        await settingsService.ResetSettingsAsync();
        var resetAppSettings = await settingsService.LoadAppSettingsAsync();
        resetAppSettings.Should().NotBeNull();

        // カスタム言語ファイルの検証
        var isValidFile = await customLanguageService.ValidateLanguageFileAsync("nonexistent.json");
        isValidFile.Should().BeFalse();

        // カスタム言語の削除
        var removeResult = await customLanguageService.RemoveCustomLanguageAsync("nonexistent");
        removeResult.Should().BeFalse();
    }
}
