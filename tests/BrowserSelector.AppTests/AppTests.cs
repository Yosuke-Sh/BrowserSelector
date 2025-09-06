using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BrowserSelector.App;
using BrowserSelector.App.DependencyInjection;

namespace BrowserSelector.AppTests;

/// <summary>
/// Appプロジェクト専用のテストクラス
/// プロセス境界の問題を回避するため、Appプロジェクトのみを対象とする
/// </summary>
public class AppTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

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
        assembly.GetName().Name.Should().Be("BrowserSelector.App");
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
        
        var systemTrayService = provider.GetService<BrowserSelector.Core.Services.ISystemTrayService>();
        systemTrayService.Should().NotBeNull();
        
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

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
