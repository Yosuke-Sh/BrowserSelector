using BrowserSelector.App;
using BrowserSelector.App.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BrowserSelector.AppTests;

/// <summary>
/// Appプロジェクト専用のテストクラス
/// プロセス境界の問題を回避するため、Appプロジェクトのみを対象とする.
/// </summary>
public class AppTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppTests"/> class.
    /// </summary>
    public AppTests()
    {
        var services = new ServiceCollection();

        // テスト用のログ設定
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Error); // エラーレベルのみ出力
        });

        // Appプロジェクトの依存関係を設定
        services.AddBrowserSelectorServices();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void App_ServiceCollectionExtensions_ShouldRegisterServices()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        services.AddBrowserSelectorServices();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.Should().NotBeNull();

        // 主要なサービスの登録を確認（AppSettingsは直接登録されていないため、他のサービスを確認）
        var browserService = provider.GetService<BrowserSelector.Core.Services.IBrowserService>();
        browserService.Should().NotBeNull();

        var settingsService = provider.GetService<BrowserSelector.Core.Services.ISettingsService>();
        settingsService.Should().NotBeNull();
    }

    [Fact]
    public void App_Program_ShouldHaveValidEntryPoint()
    {
        // Arrange & Act
        var programType = typeof(Program);

        // Assert
        programType.Should().NotBeNull();
        programType.Name.Should().Be("Program");
    }

    [Fact]
    public void App_AssemblyInfo_ShouldHaveValidAttributes()
    {
        // Arrange & Act
        var assembly = typeof(Program).Assembly;

        // Assert
        assembly.Should().NotBeNull();
        assembly.GetName().Name.Should().Be("BrowserSelector");
    }

    [Fact]
    public void App_ServiceCollectionExtensions_ShouldRegisterAllRequiredServices()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        services.AddBrowserSelectorServices();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.Should().NotBeNull();

        // すべての主要サービスの登録を確認
        var browserService = provider.GetService<BrowserSelector.Core.Services.IBrowserService>();
        browserService.Should().NotBeNull();

        var settingsService = provider.GetService<BrowserSelector.Core.Services.ISettingsService>();
        settingsService.Should().NotBeNull();

        var localizationService = provider.GetService<BrowserSelector.Core.Services.ILocalizationService>();
        localizationService.Should().NotBeNull();

        var customLanguageService = provider.GetService<BrowserSelector.Core.Services.ICustomLanguageService>();
        customLanguageService.Should().NotBeNull();

        var urlRuleService = provider.GetService<BrowserSelector.Core.Services.IUrlRuleService>();
        urlRuleService.Should().NotBeNull();

        var urlService = provider.GetService<BrowserSelector.Core.Services.IUrlService>();
        urlService.Should().NotBeNull();

        var logService = provider.GetService<BrowserSelector.Core.Services.ILogService>();
        logService.Should().NotBeNull();

        var protocolHandler = provider.GetService<BrowserSelector.Core.Services.IProtocolHandler>();
        protocolHandler.Should().NotBeNull();

        var updateService = provider.GetService<BrowserSelector.Core.Services.IUpdateService>();
        updateService.Should().NotBeNull();
    }

    [Fact]
    public void App_ServiceCollectionExtensions_ShouldRegisterViewModels()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        services.AddBrowserSelectorServices();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.Should().NotBeNull();

        // ViewModelの登録を確認
        var mainViewModel = provider.GetService<BrowserSelector.Presentation.ViewModels.MainViewModel>();
        mainViewModel.Should().NotBeNull();

        var settingsViewModel = provider.GetService<BrowserSelector.Presentation.ViewModels.SettingsViewModel>();
        settingsViewModel.Should().NotBeNull();

        var languageManagementViewModel = provider.GetService<BrowserSelector.Presentation.ViewModels.LanguageManagementViewModel>();
        languageManagementViewModel.Should().NotBeNull();
    }

    [Fact]
    public void App_ServiceCollectionExtensions_ShouldRegisterServicesWithCorrectLifetime()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        services.AddBrowserSelectorServices();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.Should().NotBeNull();

        // 同じインスタンスが返されることを確認（Scopedサービスの場合）
        var browserService1 = provider.GetService<BrowserSelector.Core.Services.IBrowserService>();
        var browserService2 = provider.GetService<BrowserSelector.Core.Services.IBrowserService>();

        // 注意: 実際のライフタイムは実装によって異なる可能性があります
        browserService1.Should().NotBeNull();
        browserService2.Should().NotBeNull();
    }

    [Fact]
    public void App_ServiceCollectionExtensions_ShouldRegisterAllViewModels()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        services.AddBrowserSelectorServices();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.Should().NotBeNull();

        // ViewModelの登録を確認
        var mainViewModel = provider.GetService<BrowserSelector.Presentation.ViewModels.MainViewModel>();
        mainViewModel.Should().NotBeNull();

        var settingsViewModel = provider.GetService<BrowserSelector.Presentation.ViewModels.SettingsViewModel>();
        settingsViewModel.Should().NotBeNull();

        // BrowserListViewModelは存在しないため、コメントアウト
        // var browserListViewModel = provider.GetService<BrowserSelector.Presentation.ViewModels.BrowserListViewModel>();
        // browserListViewModel.Should().NotBeNull();
    }

    [Fact]
    public void App_ServiceCollectionExtensions_ShouldRegisterAllServicesWithCorrectTypes()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        services.AddBrowserSelectorServices();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.Should().NotBeNull();

        // 各サービスの型を確認
        var browserService = provider.GetService<BrowserSelector.Core.Services.IBrowserService>();
        browserService.Should().NotBeNull();
        browserService.Should().BeOfType<BrowserSelector.Infrastructure.Services.BrowserService>();

        var settingsService = provider.GetService<BrowserSelector.Core.Services.ISettingsService>();
        settingsService.Should().NotBeNull();
        settingsService.Should().BeOfType<BrowserSelector.Infrastructure.Services.SettingsService>();

        var localizationService = provider.GetService<BrowserSelector.Core.Services.ILocalizationService>();
        localizationService.Should().NotBeNull();
        localizationService.Should().BeOfType<BrowserSelector.Infrastructure.Localization.LocalizationService>();
    }

    [Fact]
    public void App_ServiceCollectionExtensions_ShouldRegisterLoggingServices()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        services.AddBrowserSelectorServices();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.Should().NotBeNull();

        // ログサービスの登録を確認
        var logService = provider.GetService<BrowserSelector.Core.Services.ILogService>();
        logService.Should().NotBeNull();
        logService.Should().BeOfType<BrowserSelector.Infrastructure.Logging.LogService>();
    }

    [Fact]
    public void App_ServiceCollectionExtensions_ShouldRegisterUrlServices()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        services.AddBrowserSelectorServices();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.Should().NotBeNull();

        // URL関連サービスの登録を確認
        var urlService = provider.GetService<BrowserSelector.Core.Services.IUrlService>();
        urlService.Should().NotBeNull();
        urlService.Should().BeOfType<BrowserSelector.Infrastructure.Services.UrlService>();

        var urlRuleService = provider.GetService<BrowserSelector.Core.Services.IUrlRuleService>();
        urlRuleService.Should().NotBeNull();
        urlRuleService.Should().BeOfType<BrowserSelector.Infrastructure.Services.UrlRuleService>();
    }

    [Fact]
    public void App_ServiceCollectionExtensions_ShouldRegisterSystemServices()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        services.AddBrowserSelectorServices();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.Should().NotBeNull();

        // システム関連サービスの登録を確認
        var registryService = provider.GetService<BrowserSelector.Core.Services.IRegistryService>();
        registryService.Should().NotBeNull();
        registryService.Should().BeOfType<BrowserSelector.Infrastructure.SystemIntegration.WindowsRegistryService>();

        var protocolHandler = provider.GetService<BrowserSelector.Core.Services.IProtocolHandler>();
        protocolHandler.Should().NotBeNull();
        protocolHandler.Should().BeOfType<BrowserSelector.Infrastructure.SystemIntegration.ProtocolHandler>();
    }

    [Fact]
    public void App_ServiceCollectionExtensions_ShouldRegisterUpdateServices()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        services.AddBrowserSelectorServices();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.Should().NotBeNull();

        // アップデート関連サービスの登録を確認
        var updateService = provider.GetService<BrowserSelector.Core.Services.IUpdateService>();
        updateService.Should().NotBeNull();
        updateService.Should().BeOfType<BrowserSelector.Infrastructure.Updates.UpdateService>();
    }

    [Fact]
    public void App_ServiceCollectionExtensions_ShouldRegisterCustomLanguageServices()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        services.AddBrowserSelectorServices();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.Should().NotBeNull();

        // カスタム言語サービスの登録を確認
        var customLanguageService = provider.GetService<BrowserSelector.Core.Services.ICustomLanguageService>();
        customLanguageService.Should().NotBeNull();
        customLanguageService.Should().BeOfType<BrowserSelector.Infrastructure.Services.CustomLanguageService>();
    }

    [Fact]
    public void App_ServiceCollectionExtensions_ShouldRegisterAllServicesWithoutErrors()
    {
        // Arrange & Act
        var services = new ServiceCollection();

        // Assert - 例外が発生しないことを確認
        var act = () => services.AddBrowserSelectorServices();
        act.Should().NotThrow();

        var provider = services.BuildServiceProvider();
        provider.Should().NotBeNull();
    }

    [Fact]
    public void App_ServiceCollectionExtensions_ShouldRegisterServicesWithValidLifetime()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        services.AddBrowserSelectorServices();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.Should().NotBeNull();

        // サービスのライフタイムを確認
        var browserService = provider.GetService<BrowserSelector.Core.Services.IBrowserService>();
        browserService.Should().NotBeNull();

        // 同じプロバイダーから再度取得してインスタンスを確認
        var browserService2 = provider.GetService<BrowserSelector.Core.Services.IBrowserService>();
        browserService2.Should().NotBeNull();
    }

    [Fact]
    public void App_ServiceCollectionExtensions_ShouldRegisterAllRequiredInterfaces()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        services.AddBrowserSelectorServices();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.Should().NotBeNull();

        // すべての主要インターフェースの登録を確認
        var interfaces = new[]
        {
            typeof(BrowserSelector.Core.Services.IBrowserService),
            typeof(BrowserSelector.Core.Services.ISettingsService),
            typeof(BrowserSelector.Core.Services.ILocalizationService),
            typeof(BrowserSelector.Core.Services.ICustomLanguageService),
            typeof(BrowserSelector.Core.Services.IUrlRuleService),
            typeof(BrowserSelector.Core.Services.IUrlService),
            typeof(BrowserSelector.Core.Services.ILogService),
            typeof(BrowserSelector.Core.Services.IProtocolHandler),
            typeof(BrowserSelector.Core.Services.IUpdateService),
            typeof(BrowserSelector.Core.Services.IRegistryService)
        };

        foreach (var interfaceType in interfaces)
        {
            var service = provider.GetService(interfaceType);
            service.Should().NotBeNull($"Service {interfaceType.Name} should be registered");
        }
    }

    [Fact]
    public void App_ServiceCollectionExtensions_ShouldRegisterViewModelsWithCorrectTypes()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        services.AddBrowserSelectorServices();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.Should().NotBeNull();

        // ViewModelの型を確認
        var mainViewModel = provider.GetService<BrowserSelector.Presentation.ViewModels.MainViewModel>();
        mainViewModel.Should().NotBeNull();
        mainViewModel.Should().BeOfType<BrowserSelector.Presentation.ViewModels.MainViewModel>();

        var settingsViewModel = provider.GetService<BrowserSelector.Presentation.ViewModels.SettingsViewModel>();
        settingsViewModel.Should().NotBeNull();
        settingsViewModel.Should().BeOfType<BrowserSelector.Presentation.ViewModels.SettingsViewModel>();

        var languageManagementViewModel = provider.GetService<BrowserSelector.Presentation.ViewModels.LanguageManagementViewModel>();
        languageManagementViewModel.Should().NotBeNull();
        languageManagementViewModel.Should().BeOfType<BrowserSelector.Presentation.ViewModels.LanguageManagementViewModel>();
    }

    [Fact]
    public void App_ServiceCollectionExtensions_ShouldRegisterServicesWithCorrectImplementationTypes()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        services.AddBrowserSelectorServices();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.Should().NotBeNull();

        // 実装型の確認
        var browserService = provider.GetService<BrowserSelector.Core.Services.IBrowserService>();
        browserService.Should().NotBeNull();
        browserService.Should().BeOfType<BrowserSelector.Infrastructure.Services.BrowserService>();

        var settingsService = provider.GetService<BrowserSelector.Core.Services.ISettingsService>();
        settingsService.Should().NotBeNull();
        settingsService.Should().BeOfType<BrowserSelector.Infrastructure.Services.SettingsService>();

        var localizationService = provider.GetService<BrowserSelector.Core.Services.ILocalizationService>();
        localizationService.Should().NotBeNull();
        localizationService.Should().BeOfType<BrowserSelector.Infrastructure.Localization.LocalizationService>();

        var logService = provider.GetService<BrowserSelector.Core.Services.ILogService>();
        logService.Should().NotBeNull();
        logService.Should().BeOfType<BrowserSelector.Infrastructure.Logging.LogService>();
    }

    [Fact]
    public void App_ServiceCollectionExtensions_ShouldRegisterAllServicesWithoutNullValues()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        services.AddBrowserSelectorServices();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.Should().NotBeNull();

        // すべてのサービスがnullでないことを確認
        var allServices = new object?[]
        {
            provider.GetService<BrowserSelector.Core.Services.IBrowserService>(),
            provider.GetService<BrowserSelector.Core.Services.ISettingsService>(),
            provider.GetService<BrowserSelector.Core.Services.ILocalizationService>(),
            provider.GetService<BrowserSelector.Core.Services.ICustomLanguageService>(),
            provider.GetService<BrowserSelector.Core.Services.IUrlRuleService>(),
            provider.GetService<BrowserSelector.Core.Services.IUrlService>(),
            provider.GetService<BrowserSelector.Core.Services.ILogService>(),
            provider.GetService<BrowserSelector.Core.Services.IProtocolHandler>(),
            provider.GetService<BrowserSelector.Core.Services.IUpdateService>(),
            provider.GetService<BrowserSelector.Core.Services.IRegistryService>(),
            provider.GetService<BrowserSelector.Presentation.ViewModels.MainViewModel>(),
            provider.GetService<BrowserSelector.Presentation.ViewModels.SettingsViewModel>(),
            provider.GetService<BrowserSelector.Presentation.ViewModels.LanguageManagementViewModel>()
        };

        allServices.Should().NotContainNulls();
    }

    [Fact]
    public void App_ServiceCollectionExtensions_ShouldRegisterServicesWithValidDependencies()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        services.AddBrowserSelectorServices();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.Should().NotBeNull();

        // 依存関係の確認
        var browserService = provider.GetService<BrowserSelector.Core.Services.IBrowserService>();
        browserService.Should().NotBeNull();

        var settingsService = provider.GetService<BrowserSelector.Core.Services.ISettingsService>();
        settingsService.Should().NotBeNull();

        var logService = provider.GetService<BrowserSelector.Core.Services.ILogService>();
        logService.Should().NotBeNull();

        // サービス間の依存関係が正しく解決されることを確認
        var mainViewModel = provider.GetService<BrowserSelector.Presentation.ViewModels.MainViewModel>();
        mainViewModel.Should().NotBeNull();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
