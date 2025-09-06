using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.Localization;
using BrowserSelector.Infrastructure.Services;
using BrowserSelector.Infrastructure.SystemIntegration;
using BrowserSelector.Infrastructure.Updates;
using BrowserSelector.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BrowserSelector.App.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBrowserSelectorServices(this IServiceCollection services)
    {
        // Core Services
        services.AddScoped<IBrowserService, BrowserService>();
        services.AddScoped<ISettingsService>(provider => 
            new SettingsService(provider.GetRequiredService<ILogService>()));
        services.AddScoped<ICustomLanguageService>(provider => 
            new CustomLanguageService(provider.GetRequiredService<ILogService>()));
        services.AddScoped<ILocalizationService>(provider => 
            new LocalizationService(provider.GetRequiredService<ICustomLanguageService>(), provider.GetRequiredService<ILogService>()));
        services.AddScoped<IUrlService>(provider => 
            new UrlService(provider.GetRequiredService<ISettingsService>(), provider.GetRequiredService<ILogService>()));
        services.AddScoped<IUrlRuleService>(provider => 
            new UrlRuleService(provider.GetRequiredService<ILogService>()));

        // Infrastructure Services
        services.AddSingleton<ILogService, BrowserSelector.Infrastructure.Logging.LogService>();
        services.AddScoped<IRegistryService>(provider => 
            new WindowsRegistryService(provider.GetRequiredService<ILogService>()));
        services.AddScoped<ISystemTrayService, SystemTrayService>();
        services.AddScoped<IProtocolHandler, ProtocolHandler>();
        services.AddScoped<IUpdateService>(provider =>
            new UpdateService("https://api.github.com/repos/your-repo/releases/latest", "1.0.0"));

        // Presentation Services
        services.AddTransient<MainViewModel>();
        services.AddTransient<SettingsViewModel>();

        return services;
    }
}
