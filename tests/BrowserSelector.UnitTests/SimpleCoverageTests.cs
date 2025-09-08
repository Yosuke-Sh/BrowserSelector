using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BrowserSelector.UnitTests;

/// <summary>
/// シンプルなカバレッジ向上テスト
/// </summary>
public class SimpleCoverageTests
{
    [Fact]
    public void CoreModels_ShouldBeInstantiable()
    {
        // Arrange & Act
        AppSettings appSettings = new();
        VisualSettings visualSettings = new();
        Browser browser = new();
        UrlRule urlRule = new();
        CustomLanguageFile customLanguageFile = new();

        // Assert
        _ = appSettings.Should().NotBeNull();
        _ = visualSettings.Should().NotBeNull();
        _ = browser.Should().NotBeNull();
        _ = urlRule.Should().NotBeNull();
        _ = customLanguageFile.Should().NotBeNull();
    }

    [Fact]
    public void BrowserModel_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        Browser browser = new();

        // Assert
        _ = browser.Name.Should().BeEmpty();
        _ = browser.ExecutablePath.Should().BeEmpty();
        _ = browser.IconPath.Should().BeEmpty();
        _ = browser.Arguments.Should().BeEmpty();
        _ = browser.IsDefault.Should().BeFalse();
        _ = browser.IsEnabled.Should().BeTrue();
        _ = browser.DisplayOrder.Should().Be(0);
        _ = browser.LastUsed.Should().Be(DateTime.MinValue);
        _ = browser.UseCount.Should().Be(0);
        _ = browser.Id.Should().NotBe(Guid.Empty);
        _ = browser.Type.Should().Be(BrowserType.Custom);
        _ = browser.IsValid.Should().BeFalse();
        _ = browser.DisplayName.Should().Be("Unknown Browser");
    }

    [Fact]
    public void BrowserModel_ShouldIncrementUseCount()
    {
        // Arrange
        Browser browser = new();

        // Act
        browser.IncrementUseCount();

        // Assert
        _ = browser.UseCount.Should().Be(1);
        _ = browser.LastUsed.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void BrowserModel_ShouldCloneCorrectly()
    {
        // Arrange
        Browser originalBrowser = new()
        {
            Name = "Test Browser",
            ExecutablePath = @"C:\test\browser.exe",
            IconPath = @"C:\test\icon.ico",
            Arguments = "--test",
            IsDefault = true,
            IsEnabled = false,
            DisplayOrder = 5,
            Type = BrowserType.Chrome
        };

        // Act
        Browser clonedBrowser = originalBrowser.Clone();

        // Assert
        _ = clonedBrowser.Should().NotBeSameAs(originalBrowser);
        _ = clonedBrowser.Id.Should().NotBe(originalBrowser.Id);
        _ = clonedBrowser.Name.Should().Be(originalBrowser.Name);
        _ = clonedBrowser.ExecutablePath.Should().Be(originalBrowser.ExecutablePath);
        _ = clonedBrowser.IconPath.Should().Be(originalBrowser.IconPath);
        _ = clonedBrowser.Arguments.Should().Be(originalBrowser.Arguments);
        _ = clonedBrowser.IsDefault.Should().BeFalse(); // 複製時はfalse
        _ = clonedBrowser.IsEnabled.Should().Be(originalBrowser.IsEnabled);
        _ = clonedBrowser.DisplayOrder.Should().Be(originalBrowser.DisplayOrder);
        _ = clonedBrowser.Type.Should().Be(originalBrowser.Type);
    }

    [Fact]
    public void UrlRuleModel_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        UrlRule urlRule = new();

        // Assert
        _ = urlRule.Id.Should().NotBe(Guid.Empty);
        _ = urlRule.Pattern.Should().BeEmpty();
        _ = urlRule.BrowserName.Should().BeEmpty();
        _ = urlRule.Priority.Should().Be(50);
        _ = urlRule.IsEnabled.Should().BeTrue();
        _ = urlRule.Description.Should().BeEmpty();
        _ = urlRule.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        _ = urlRule.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UrlRuleModel_ShouldMatchUrls()
    {
        // Arrange
        UrlRule urlRule = new()
        {
            Pattern = "*.example.com"
        };

        // Act & Assert
        _ = urlRule.IsMatch("https://www.example.com").Should().BeTrue();
        _ = urlRule.IsMatch("https://sub.example.com").Should().BeTrue();
        _ = urlRule.IsMatch("https://www.different.com").Should().BeFalse();
        _ = urlRule.IsMatch("").Should().BeFalse();
    }

    [Fact]
    public void UrlRuleModel_ShouldGenerateDisplayName()
    {
        // Arrange
        UrlRule urlRule = new()
        {
            Pattern = "*.example.com",
            BrowserName = "Test Browser",
            Priority = 75
        };

        // Act
        string displayName = urlRule.DisplayName;

        // Assert
        _ = displayName.Should().Contain("*.example.com");
        _ = displayName.Should().Contain("Test Browser");
        _ = displayName.Should().Contain("75");
    }

    [Fact]
    public void UrlRuleModel_ShouldGenerateDetails()
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
        _ = details.Should().Contain("*.example.com");
        _ = details.Should().Contain("Test Browser");
        _ = details.Should().Contain("75");
        _ = details.Should().Contain("有効");
        _ = details.Should().Contain("Test description");
    }

    [Fact]
    public void CustomLanguageFileModel_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        CustomLanguageFile customLanguageFile = new();

        // Assert
        _ = customLanguageFile.CultureCode.Should().BeEmpty();
        _ = customLanguageFile.DisplayName.Should().BeEmpty();
        _ = customLanguageFile.Resources.Should().NotBeNull();
        _ = customLanguageFile.Resources.Should().BeEmpty();
        _ = customLanguageFile.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        _ = customLanguageFile.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        _ = customLanguageFile.Version.Should().Be("1.0");
        _ = customLanguageFile.Description.Should().BeNull();
        _ = customLanguageFile.Author.Should().BeNull();
    }

    [Fact]
    public void AppSettingsModel_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        AppSettings appSettings = new();

        // Assert
        _ = appSettings.StartupMessage.Should().BeEmpty();
        _ = appSettings.EnableLogging.Should().BeTrue();
        _ = appSettings.LogLevel.Should().Be("Information");
        _ = appSettings.CheckForUpdates.Should().BeTrue();
        _ = appSettings.UpdateCheckInterval.Should().Be(24);
        _ = appSettings.Language.Should().Be("en-US");
        _ = appSettings.PortableMode.Should().BeFalse();
        _ = appSettings.CustomProtocol.Should().Be("browserselector");
        _ = appSettings.RegisterProtocol.Should().BeTrue();
        _ = appSettings.CloseAfterUrlRuleMatch.Should().BeTrue();
    }

    [Fact]
    public void VisualSettingsModel_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        VisualSettings visualSettings = new();

        // Assert
        _ = visualSettings.BackgroundColor.Should().Be(System.Windows.Media.Colors.White);
        _ = visualSettings.UseBackgroundGradient.Should().BeFalse();
        _ = visualSettings.GradientStartColor.Should().Be(System.Windows.Media.Colors.Transparent);
        _ = visualSettings.GradientEndColor.Should().Be(System.Windows.Media.Colors.Transparent);
        _ = visualSettings.GradientDirection.Should().Be(BrowserSelector.Core.Enums.GradientDirection.Vertical);
        _ = visualSettings.IconScale.Should().Be(1.0);
        _ = visualSettings.ShowFocusIndicator.Should().BeTrue();
        _ = visualSettings.FocusColor.Should().Be(System.Windows.Media.Colors.Blue);
        _ = visualSettings.FocusThickness.Should().Be(2.0);
        _ = visualSettings.FocusWidth.Should().Be(100.0);
        _ = visualSettings.InitialWindowWidth.Should().Be(800.0);
        _ = visualSettings.InitialWindowHeight.Should().Be(600.0);
        _ = visualSettings.ShowLogo.Should().BeTrue();
        _ = visualSettings.ShowUrlInput.Should().BeTrue();
        _ = visualSettings.BrowserButtonWidth.Should().Be(120.0);
        _ = visualSettings.BrowserButtonHeight.Should().Be(90.0);
        _ = visualSettings.BrowserButtonBackgroundColor.Should().Be(System.Windows.Media.Colors.Transparent);
        _ = visualSettings.BrowserButtonForegroundColor.Should().Be(System.Windows.Media.Colors.Black);
        _ = visualSettings.BrowserButtonOpacity.Should().Be(1.0);
        _ = visualSettings.BrowserButtonCornerRadius.Should().Be(8.0);
        _ = visualSettings.ShowBrowserName.Should().BeTrue();
        _ = visualSettings.BrowserIconSize.Should().Be(32.0);
    }

    [Fact]
    public void InfrastructureServices_ShouldBeResolvable()
    {
        // Arrange
        ServiceCollection services = new();
        _ = services.AddLogging();
        _ = services.AddSingleton<Mock<IRegistryService>>().AddSingleton(provider => provider.GetRequiredService<Mock<IRegistryService>>().Object);
        _ = services.AddSingleton<Mock<ILogService>>().AddSingleton(provider => provider.GetRequiredService<Mock<ILogService>>().Object);
        _ = services.AddSingleton<IBrowserService, BrowserService>();
        _ = services.AddSingleton<ISettingsService, SettingsService>();
        _ = services.AddSingleton<IUrlService, UrlService>();
        _ = services.AddSingleton<IUrlRuleService, UrlRuleService>();
        _ = services.AddSingleton<ICustomLanguageService, CustomLanguageService>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        IBrowserService? browserService = serviceProvider.GetService<IBrowserService>();
        ISettingsService? settingsService = serviceProvider.GetService<ISettingsService>();
        IUrlService? urlService = serviceProvider.GetService<IUrlService>();
        IUrlRuleService? urlRuleService = serviceProvider.GetService<IUrlRuleService>();
        ICustomLanguageService? customLanguageService = serviceProvider.GetService<ICustomLanguageService>();

        _ = browserService.Should().NotBeNull();
        _ = settingsService.Should().NotBeNull();
        _ = urlService.Should().NotBeNull();
        _ = urlRuleService.Should().NotBeNull();
        _ = customLanguageService.Should().NotBeNull();
    }

    [Fact]
    public async Task InfrastructureServices_ShouldExecuteBasicMethods()
    {
        // Arrange
        ServiceCollection services = new();
        _ = services.AddLogging();
        _ = services.AddSingleton<Mock<IRegistryService>>().AddSingleton(provider => provider.GetRequiredService<Mock<IRegistryService>>().Object);
        _ = services.AddSingleton<Mock<ILogService>>().AddSingleton(provider => provider.GetRequiredService<Mock<ILogService>>().Object);
        _ = services.AddSingleton<IBrowserService, BrowserService>();
        _ = services.AddSingleton<ISettingsService, SettingsService>();
        _ = services.AddSingleton<IUrlService, UrlService>();
        _ = services.AddSingleton<IUrlRuleService, UrlRuleService>();
        _ = services.AddSingleton<ICustomLanguageService, CustomLanguageService>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        IBrowserService browserService = serviceProvider.GetRequiredService<IBrowserService>();
        ISettingsService settingsService = serviceProvider.GetRequiredService<ISettingsService>();
        IUrlService urlService = serviceProvider.GetRequiredService<IUrlService>();
        IUrlRuleService urlRuleService = serviceProvider.GetRequiredService<IUrlRuleService>();
        ICustomLanguageService customLanguageService = serviceProvider.GetRequiredService<ICustomLanguageService>();

        // 基本的なメソッドの実行
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
}
