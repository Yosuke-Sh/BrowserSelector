using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BrowserSelector.UnitTests;

/// <summary>
/// Core・Infrastructureプロジェクトのカバレッジ向上のためのテスト.
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

        ServiceCollection services = new();
        _ = services.AddLogging();
        _ = services.AddSingleton(_mockRegistryService.Object);
        _ = services.AddSingleton(_mockLogService.Object);
        _ = services.AddSingleton<IBrowserService, BrowserService>();
        _ = services.AddSingleton<ISettingsService, SettingsService>();
        _ = services.AddSingleton<IUrlService, UrlService>();
        _ = services.AddSingleton<IUrlRuleService, UrlRuleService>();
        _ = services.AddSingleton<ICustomLanguageService, CustomLanguageService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void CoreModels_ShouldBeAccessible()
    {
        // Arrange & Act
        AppSettings appSettings = new();
        VisualSettings visualSettings = new();
        Browser browser = new();
        UrlRule urlRule = new();
        CustomLanguageFile customLanguageFile = new();

        // Assert - これらがアクセス可能であることを確認
        _ = appSettings.Should().NotBeNull();
        _ = visualSettings.Should().NotBeNull();
        _ = browser.Should().NotBeNull();
        _ = urlRule.Should().NotBeNull();
        _ = customLanguageFile.Should().NotBeNull();
    }

    [Fact]
    public void CoreServices_ShouldBeResolvable()
    {
        // Arrange & Act
        IBrowserService? browserService = _serviceProvider.GetService<IBrowserService>();
        ISettingsService? settingsService = _serviceProvider.GetService<ISettingsService>();
        IUrlService? urlService = _serviceProvider.GetService<IUrlService>();
        IUrlRuleService? urlRuleService = _serviceProvider.GetService<IUrlRuleService>();
        ICustomLanguageService? customLanguageService = _serviceProvider.GetService<ICustomLanguageService>();

        // Assert
        _ = browserService.Should().NotBeNull();
        _ = settingsService.Should().NotBeNull();
        _ = urlService.Should().NotBeNull();
        _ = urlRuleService.Should().NotBeNull();
        _ = customLanguageService.Should().NotBeNull();
    }

    [Fact]
    public async Task InfrastructureServices_ShouldExecuteMethods()
    {
        // Arrange
        IBrowserService browserService = _serviceProvider.GetRequiredService<IBrowserService>();
        ISettingsService settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
        IUrlService urlService = _serviceProvider.GetRequiredService<IUrlService>();
        IUrlRuleService urlRuleService = _serviceProvider.GetRequiredService<IUrlRuleService>();
        ICustomLanguageService customLanguageService = _serviceProvider.GetRequiredService<ICustomLanguageService>();

        // Act & Assert - 各サービスのメソッドを実行してカバレッジを向上
        IEnumerable<Browser> browsers = await browserService.DetectBrowsersAsync();
        _ = browsers.Should().NotBeNull();

        AppSettings appSettings = await settingsService.LoadAppSettingsAsync();
        _ = appSettings.Should().NotBeNull();

        VisualSettings visualSettings = await settingsService.LoadVisualSettingsAsync();
        _ = visualSettings.Should().NotBeNull();

        string normalizedUrl = await urlService.NormalizeUrlAsync("https://example.com");
        _ = normalizedUrl.Should().NotBeNullOrEmpty();

        bool isValidUrl = await urlService.ValidateUrlAsync("https://example.com");
        _ = isValidUrl.Should().BeTrue();

        string domain = urlService.ExtractDomain("https://www.example.com/path");
        _ = domain.Should().Be("www.example.com");

        string urlWithProtocol = urlService.AddProtocolIfNeeded("example.com");
        _ = urlWithProtocol.Should().Be("https://example.com");

        IEnumerable<UrlRule> urlRules = await urlRuleService.GetAllRulesAsync();
        _ = urlRules.Should().NotBeNull();

        IEnumerable<UrlRule> enabledRules = await urlRuleService.GetEnabledRulesAsync();
        _ = enabledRules.Should().NotBeNull();

        IEnumerable<LanguageInfo> availableLanguages = await customLanguageService.GetAvailableLanguagesAsync();
        _ = availableLanguages.Should().NotBeNull();

        string customLanguageFolder = customLanguageService.GetCustomLanguageFolder();
        _ = customLanguageFolder.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void BrowserModel_ShouldHaveAllProperties()
    {
        // Arrange
        Browser browser = new()
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
        _ = browser.Name.Should().Be("Test Browser");
        _ = browser.ExecutablePath.Should().Be(@"C:\Program Files\TestBrowser\browser.exe");
        _ = browser.IconPath.Should().Be(@"C:\Program Files\TestBrowser\icon.ico");
        _ = browser.Arguments.Should().Be("--test-arg");
        _ = browser.IsDefault.Should().BeTrue();
        _ = browser.IsEnabled.Should().BeTrue();
        _ = browser.DisplayOrder.Should().Be(1);
        _ = browser.LastUsed.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        _ = browser.UseCount.Should().Be(5);
        _ = browser.Type.Should().Be(BrowserType.Chrome);
        _ = browser.IsValid.Should().BeTrue();
        _ = browser.DisplayName.Should().Be("Test Browser");

        // メソッドの実行
        browser.IncrementUseCount();
        _ = browser.UseCount.Should().Be(6);

        Browser clonedBrowser = browser.Clone();
        _ = clonedBrowser.Should().NotBeSameAs(browser);
        _ = clonedBrowser.Name.Should().Be(browser.Name);
    }

    [Fact]
    public void UrlRuleModel_ShouldHaveAllProperties()
    {
        // Arrange
        UrlRule urlRule = new()
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
        _ = urlRule.Pattern.Should().Be("*.example.com");
        _ = urlRule.BrowserName.Should().Be("Test Browser");
        _ = urlRule.Priority.Should().Be(75);
        _ = urlRule.IsEnabled.Should().BeTrue();
        _ = urlRule.Description.Should().Be("Test rule");
        _ = urlRule.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        _ = urlRule.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        _ = urlRule.Id.Should().NotBe(Guid.Empty);

        // メソッドの実行
        bool isMatch = urlRule.IsMatch("https://www.example.com");
        _ = isMatch.Should().BeTrue();

        string displayName = urlRule.DisplayName;
        _ = displayName.Should().Contain("*.example.com");
        _ = displayName.Should().Contain("Test Browser");

        string details = urlRule.GetDetails();
        _ = details.Should().Contain("*.example.com");
        _ = details.Should().Contain("Test Browser");
        _ = details.Should().Contain("75");
    }

    [Fact]
    public void CustomLanguageFileModel_ShouldHaveAllProperties()
    {
        // Arrange
        CustomLanguageFile customLanguageFile = new()
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
        _ = customLanguageFile.CultureCode.Should().Be("ja-JP");
        _ = customLanguageFile.DisplayName.Should().Be("日本語");
        _ = customLanguageFile.Resources.Should().HaveCount(2);
        _ = customLanguageFile.Resources["key1"].Should().Be("value1");
        _ = customLanguageFile.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        _ = customLanguageFile.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        _ = customLanguageFile.Version.Should().Be("1.1");
        _ = customLanguageFile.Description.Should().Be("Japanese language file");
        _ = customLanguageFile.Author.Should().Be("Test Author");
    }

    [Fact]
    public void AppSettingsModel_ShouldHaveAllProperties()
    {
        // Arrange
        AppSettings appSettings = new()
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
        _ = appSettings.StartupMessage.Should().Be("Test startup message");
        _ = appSettings.EnableLogging.Should().BeFalse();
        _ = appSettings.LogLevel.Should().Be("Debug");
        _ = appSettings.CheckForUpdates.Should().BeFalse();
        _ = appSettings.UpdateCheckInterval.Should().Be(12);
        _ = appSettings.Language.Should().Be("ja-JP");
        _ = appSettings.PortableMode.Should().BeTrue();
        _ = appSettings.CustomProtocol.Should().Be("test-protocol");
        _ = appSettings.RegisterProtocol.Should().BeFalse();
        _ = appSettings.CloseAfterUrlRuleMatch.Should().BeFalse();
    }

    [Fact]
    public void VisualSettingsModel_ShouldHaveAllProperties()
    {
        // Arrange
        VisualSettings visualSettings = new()
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
        _ = visualSettings.BackgroundColor.Should().Be(System.Windows.Media.Colors.Red);
        _ = visualSettings.UseBackgroundGradient.Should().BeTrue();
        _ = visualSettings.GradientStartColor.Should().Be(System.Windows.Media.Colors.Blue);
        _ = visualSettings.GradientEndColor.Should().Be(System.Windows.Media.Colors.Green);
        _ = visualSettings.GradientDirection.Should().Be(BrowserSelector.Core.Enums.GradientDirection.Horizontal);
        _ = visualSettings.IconScale.Should().Be(1.5);
        _ = visualSettings.ShowFocusIndicator.Should().BeFalse();
        _ = visualSettings.FocusColor.Should().Be(System.Windows.Media.Colors.Yellow);
        _ = visualSettings.FocusThickness.Should().Be(3.0);
        _ = visualSettings.FocusWidth.Should().Be(150.0);
        _ = visualSettings.InitialWindowWidth.Should().Be(1000.0);
        _ = visualSettings.InitialWindowHeight.Should().Be(800.0);
        _ = visualSettings.ShowLogo.Should().BeFalse();
        _ = visualSettings.ShowUrlInput.Should().BeFalse();
        _ = visualSettings.BrowserButtonWidth.Should().Be(150.0);
        _ = visualSettings.BrowserButtonHeight.Should().Be(100.0);
        _ = visualSettings.BrowserButtonBackgroundColor.Should().Be(System.Windows.Media.Colors.White);
        _ = visualSettings.BrowserButtonForegroundColor.Should().Be(System.Windows.Media.Colors.Blue);
        _ = visualSettings.BrowserButtonOpacity.Should().Be(0.8);
        _ = visualSettings.BrowserButtonCornerRadius.Should().Be(10.0);
        _ = visualSettings.ShowBrowserName.Should().BeFalse();
        _ = visualSettings.BrowserIconSize.Should().Be(48.0);
    }

    [Fact]
    public async Task InfrastructureServices_ShouldHandleEdgeCases()
    {
        // Arrange
        _ = _serviceProvider.GetRequiredService<IBrowserService>();
        ISettingsService settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
        IUrlService urlService = _serviceProvider.GetRequiredService<IUrlService>();
        _ = _serviceProvider.GetRequiredService<IUrlRuleService>();
        ICustomLanguageService customLanguageService = _serviceProvider.GetRequiredService<ICustomLanguageService>();

        // Act & Assert - エッジケースのテスト
        string emptyUrl = await urlService.NormalizeUrlAsync("");
        _ = emptyUrl.Should().BeEmpty();

        string whitespaceUrl = await urlService.NormalizeUrlAsync("   ");
        _ = whitespaceUrl.Should().BeEmpty();

        bool invalidUrl = await urlService.ValidateUrlAsync("invalid-url");
        _ = invalidUrl.Should().BeFalse();

        string emptyDomain = urlService.ExtractDomain("");
        _ = emptyDomain.Should().BeEmpty();

        string urlWithProtocol = urlService.AddProtocolIfNeeded("http://example.com");
        _ = urlWithProtocol.Should().Be("http://example.com");

        // 設定のリセット
        _ = await settingsService.ResetSettingsAsync();
        AppSettings resetAppSettings = await settingsService.LoadAppSettingsAsync();
        _ = resetAppSettings.Should().NotBeNull();

        // カスタム言語ファイルの検証
        bool isValidFile = await customLanguageService.ValidateLanguageFileAsync("nonexistent.json");
        _ = isValidFile.Should().BeFalse();

        // カスタム言語の削除
        bool removeResult = await customLanguageService.RemoveCustomLanguageAsync("nonexistent");
        _ = removeResult.Should().BeFalse();
    }
}
