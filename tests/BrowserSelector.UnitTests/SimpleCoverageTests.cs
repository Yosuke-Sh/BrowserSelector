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
/// シンプルなカバレッジ向上テスト
/// </summary>
public class SimpleCoverageTests
{
    [Fact]
    public void CoreModels_ShouldBeInstantiable()
    {
        // Arrange & Act
        var appSettings = new AppSettings();
        var visualSettings = new VisualSettings();
        var browser = new Browser();
        var urlRule = new UrlRule();
        var customLanguageFile = new CustomLanguageFile();

        // Assert
        appSettings.Should().NotBeNull();
        visualSettings.Should().NotBeNull();
        browser.Should().NotBeNull();
        urlRule.Should().NotBeNull();
        customLanguageFile.Should().NotBeNull();
    }

    [Fact]
    public void BrowserModel_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var browser = new Browser();

        // Assert
        browser.Name.Should().BeEmpty();
        browser.ExecutablePath.Should().BeEmpty();
        browser.IconPath.Should().BeEmpty();
        browser.Arguments.Should().BeEmpty();
        browser.IsDefault.Should().BeFalse();
        browser.IsEnabled.Should().BeTrue();
        browser.DisplayOrder.Should().Be(0);
        browser.LastUsed.Should().Be(DateTime.MinValue);
        browser.UseCount.Should().Be(0);
        browser.Id.Should().NotBe(Guid.Empty);
        browser.Type.Should().Be(BrowserType.Custom);
        browser.IsValid.Should().BeFalse();
        browser.DisplayName.Should().Be("Unknown Browser");
    }

    [Fact]
    public void BrowserModel_ShouldIncrementUseCount()
    {
        // Arrange
        var browser = new Browser();

        // Act
        browser.IncrementUseCount();

        // Assert
        browser.UseCount.Should().Be(1);
        browser.LastUsed.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void BrowserModel_ShouldCloneCorrectly()
    {
        // Arrange
        var originalBrowser = new Browser
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
        var clonedBrowser = originalBrowser.Clone();

        // Assert
        clonedBrowser.Should().NotBeSameAs(originalBrowser);
        clonedBrowser.Id.Should().NotBe(originalBrowser.Id);
        clonedBrowser.Name.Should().Be(originalBrowser.Name);
        clonedBrowser.ExecutablePath.Should().Be(originalBrowser.ExecutablePath);
        clonedBrowser.IconPath.Should().Be(originalBrowser.IconPath);
        clonedBrowser.Arguments.Should().Be(originalBrowser.Arguments);
        clonedBrowser.IsDefault.Should().BeFalse(); // 複製時はfalse
        clonedBrowser.IsEnabled.Should().Be(originalBrowser.IsEnabled);
        clonedBrowser.DisplayOrder.Should().Be(originalBrowser.DisplayOrder);
        clonedBrowser.Type.Should().Be(originalBrowser.Type);
    }

    [Fact]
    public void UrlRuleModel_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var urlRule = new UrlRule();

        // Assert
        urlRule.Id.Should().NotBe(Guid.Empty);
        urlRule.Pattern.Should().BeEmpty();
        urlRule.BrowserName.Should().BeEmpty();
        urlRule.Priority.Should().Be(50);
        urlRule.IsEnabled.Should().BeTrue();
        urlRule.Description.Should().BeEmpty();
        urlRule.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        urlRule.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UrlRuleModel_ShouldMatchUrls()
    {
        // Arrange
        var urlRule = new UrlRule
        {
            Pattern = "*.example.com"
        };

        // Act & Assert
        urlRule.IsMatch("https://www.example.com").Should().BeTrue();
        urlRule.IsMatch("https://sub.example.com").Should().BeTrue();
        urlRule.IsMatch("https://www.different.com").Should().BeFalse();
        urlRule.IsMatch("").Should().BeFalse();
    }

    [Fact]
    public void UrlRuleModel_ShouldGenerateDisplayName()
    {
        // Arrange
        var urlRule = new UrlRule
        {
            Pattern = "*.example.com",
            BrowserName = "Test Browser",
            Priority = 75
        };

        // Act
        var displayName = urlRule.DisplayName;

        // Assert
        displayName.Should().Contain("*.example.com");
        displayName.Should().Contain("Test Browser");
        displayName.Should().Contain("75");
    }

    [Fact]
    public void UrlRuleModel_ShouldGenerateDetails()
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
        details.Should().Contain("*.example.com");
        details.Should().Contain("Test Browser");
        details.Should().Contain("75");
        details.Should().Contain("有効");
        details.Should().Contain("Test description");
    }

    [Fact]
    public void CustomLanguageFileModel_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var customLanguageFile = new CustomLanguageFile();

        // Assert
        customLanguageFile.CultureCode.Should().BeEmpty();
        customLanguageFile.DisplayName.Should().BeEmpty();
        customLanguageFile.Resources.Should().NotBeNull();
        customLanguageFile.Resources.Should().BeEmpty();
        customLanguageFile.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        customLanguageFile.UpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        customLanguageFile.Version.Should().Be("1.0");
        customLanguageFile.Description.Should().BeNull();
        customLanguageFile.Author.Should().BeNull();
    }

    [Fact]
    public void AppSettingsModel_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var appSettings = new AppSettings();

        // Assert
        appSettings.StartupMessage.Should().BeEmpty();
        appSettings.EnableLogging.Should().BeTrue();
        appSettings.LogLevel.Should().Be("Information");
        appSettings.CheckForUpdates.Should().BeTrue();
        appSettings.UpdateCheckInterval.Should().Be(24);
        appSettings.Language.Should().Be("en-US");
        appSettings.PortableMode.Should().BeFalse();
        appSettings.CustomProtocol.Should().Be("browserselector");
        appSettings.RegisterProtocol.Should().BeTrue();
        appSettings.CloseAfterUrlRuleMatch.Should().BeTrue();
    }

    [Fact]
    public void VisualSettingsModel_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var visualSettings = new VisualSettings();

        // Assert
        visualSettings.BackgroundColor.Should().Be(System.Windows.Media.Colors.White);
        visualSettings.UseBackgroundGradient.Should().BeFalse();
        visualSettings.GradientStartColor.Should().Be(System.Windows.Media.Colors.Transparent);
        visualSettings.GradientEndColor.Should().Be(System.Windows.Media.Colors.Transparent);
        visualSettings.GradientDirection.Should().Be(BrowserSelector.Core.Enums.GradientDirection.Vertical);
        visualSettings.IconScale.Should().Be(1.0);
        visualSettings.ShowFocusIndicator.Should().BeTrue();
        visualSettings.FocusColor.Should().Be(System.Windows.Media.Colors.Blue);
        visualSettings.FocusThickness.Should().Be(2.0);
        visualSettings.FocusWidth.Should().Be(100.0);
        visualSettings.InitialWindowWidth.Should().Be(800.0);
        visualSettings.InitialWindowHeight.Should().Be(600.0);
        visualSettings.ShowLogo.Should().BeTrue();
        visualSettings.ShowUrlInput.Should().BeTrue();
        visualSettings.BrowserButtonWidth.Should().Be(120.0);
        visualSettings.BrowserButtonHeight.Should().Be(90.0);
        visualSettings.BrowserButtonBackgroundColor.Should().Be(System.Windows.Media.Colors.Transparent);
        visualSettings.BrowserButtonForegroundColor.Should().Be(System.Windows.Media.Colors.Black);
        visualSettings.BrowserButtonOpacity.Should().Be(1.0);
        visualSettings.BrowserButtonCornerRadius.Should().Be(8.0);
        visualSettings.ShowBrowserName.Should().BeTrue();
        visualSettings.BrowserIconSize.Should().Be(32.0);
    }

    [Fact]
    public void InfrastructureServices_ShouldBeResolvable()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<Mock<IRegistryService>>().AddSingleton(provider => provider.GetRequiredService<Mock<IRegistryService>>().Object);
        services.AddSingleton<Mock<ILogService>>().AddSingleton(provider => provider.GetRequiredService<Mock<ILogService>>().Object);
        services.AddSingleton<IBrowserService, BrowserService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IUrlService, UrlService>();
        services.AddSingleton<IUrlRuleService, UrlRuleService>();
        services.AddSingleton<ICustomLanguageService, CustomLanguageService>();

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var browserService = serviceProvider.GetService<IBrowserService>();
        var settingsService = serviceProvider.GetService<ISettingsService>();
        var urlService = serviceProvider.GetService<IUrlService>();
        var urlRuleService = serviceProvider.GetService<IUrlRuleService>();
        var customLanguageService = serviceProvider.GetService<ICustomLanguageService>();

        browserService.Should().NotBeNull();
        settingsService.Should().NotBeNull();
        urlService.Should().NotBeNull();
        urlRuleService.Should().NotBeNull();
        customLanguageService.Should().NotBeNull();
    }

    [Fact]
    public async Task InfrastructureServices_ShouldExecuteBasicMethods()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<Mock<IRegistryService>>().AddSingleton(provider => provider.GetRequiredService<Mock<IRegistryService>>().Object);
        services.AddSingleton<Mock<ILogService>>().AddSingleton(provider => provider.GetRequiredService<Mock<ILogService>>().Object);
        services.AddSingleton<IBrowserService, BrowserService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IUrlService, UrlService>();
        services.AddSingleton<IUrlRuleService, UrlRuleService>();
        services.AddSingleton<ICustomLanguageService, CustomLanguageService>();

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var browserService = serviceProvider.GetRequiredService<IBrowserService>();
        var settingsService = serviceProvider.GetRequiredService<ISettingsService>();
        var urlService = serviceProvider.GetRequiredService<IUrlService>();
        var urlRuleService = serviceProvider.GetRequiredService<IUrlRuleService>();
        var customLanguageService = serviceProvider.GetRequiredService<ICustomLanguageService>();

        // 基本的なメソッドの実行
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
}
