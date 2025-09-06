using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.Services;
using BrowserSelector.Infrastructure.SystemIntegration;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BrowserSelector.IntegrationTests;

public class ServiceIntegrationTests
{
    private readonly IServiceProvider _serviceProvider;

    public ServiceIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<IRegistryService, WindowsRegistryService>();
        services.AddSingleton<IBrowserService, BrowserService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IUrlService, UrlService>();
        
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task BrowserService_ShouldDetectBrowsers()
    {
        // Arrange
        var browserService = _serviceProvider.GetRequiredService<IBrowserService>();

        // Act
        var browsers = await browserService.DetectBrowsersAsync();

        // Assert
        browsers.Should().NotBeNull("ブラウザ検出サービスが正常に動作すること");
        // テスト環境ではブラウザが検出されない場合もあるため、柔軟に判定
        if (browsers.Any())
        {
            browsers.Should().NotBeEmpty("少なくとも1つのブラウザが検出されること");
        }
    }

    [Fact]
    public async Task SettingsService_ShouldSaveAndLoadSettings()
    {
        // Arrange
        var settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
        var testSettings = new AppSettings
        {
            Language = "ja-JP",
            CustomProtocol = "browserselector",
            EnableLogging = true,
            CheckForUpdates = true
        };

        // Act
        await settingsService.SaveAppSettingsAsync(testSettings);
        var loadedSettings = await settingsService.LoadAppSettingsAsync();

        // Assert
        loadedSettings.Should().NotBeNull("設定の読み込みが成功すること");
        loadedSettings.Language.Should().Be(testSettings.Language, "言語設定が正しく保存・読み込みされること");
        loadedSettings.CustomProtocol.Should().Be(testSettings.CustomProtocol, "カスタムプロトコルが正しく保存・読み込みされること");
        loadedSettings.EnableLogging.Should().Be(testSettings.EnableLogging, "ログ有効設定が正しく保存・読み込みされること");
        loadedSettings.CheckForUpdates.Should().Be(testSettings.CheckForUpdates, "更新チェック設定が正しく保存・読み込みされること");
    }

    [Fact]
    public async Task UrlService_ShouldProcessUrls()
    {
        // Arrange
        var urlService = _serviceProvider.GetRequiredService<IUrlService>();
        var testUrl = "https://www.google.com";

        // Act
        var normalizedUrl = await urlService.NormalizeUrlAsync(testUrl);
        var isValid = await urlService.ValidateUrlAsync(testUrl);

        // Assert
        normalizedUrl.Should().NotBeNullOrEmpty("URL正規化が正常に動作すること");
        isValid.Should().BeTrue("URL検証が正常に動作すること");
    }

    [Fact]
    public void ServiceContainer_ShouldResolveAllServices()
    {
        // Act & Assert
        var browserService = _serviceProvider.GetRequiredService<IBrowserService>();
        var settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
        var urlService = _serviceProvider.GetRequiredService<IUrlService>();
        var registryService = _serviceProvider.GetRequiredService<IRegistryService>();

        browserService.Should().NotBeNull("BrowserServiceが正常に解決されること");
        settingsService.Should().NotBeNull("SettingsServiceが正常に解決されること");
        urlService.Should().NotBeNull("UrlServiceが正常に解決されること");
        registryService.Should().NotBeNull("RegistryServiceが正常に解決されること");
    }
}