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
        _ = services.AddScoped<IBrowserService, BrowserService>();
        _ = services.AddScoped<ISettingsService>(provider =>
            new SettingsService(provider.GetRequiredService<ILogService>()));
        _ = services.AddScoped<ICustomLanguageService>(provider =>
            new CustomLanguageService(provider.GetRequiredService<ILogService>()));
        _ = services.AddScoped<ILocalizationService>(provider =>
            new LocalizationService(provider.GetRequiredService<ICustomLanguageService>(), provider.GetRequiredService<ILogService>()));
        _ = services.AddScoped<IUrlService>(provider =>
            new UrlService(provider.GetRequiredService<ISettingsService>(), provider.GetRequiredService<ILogService>()));
        _ = services.AddScoped<IUrlRuleService>(provider =>
            new UrlRuleService(provider.GetRequiredService<ILogService>()));

        // Infrastructure Services
        _ = services.AddSingleton<ILogService, BrowserSelector.Infrastructure.Logging.LogService>();
        _ = services.AddScoped<IRegistryService>(provider =>
            new WindowsRegistryService(provider.GetRequiredService<ILogService>()));
        _ = services.AddScoped<ISystemTrayService, SystemTrayService>();
        _ = services.AddScoped<IProtocolHandler, ProtocolHandler>();
        _ = services.AddScoped<IUpdateService>(provider =>
            new UpdateService("https://api.github.com/repos/your-repo/releases/latest", "1.0.0"));

        // Presentation Services
        _ = services.AddTransient<MainViewModel>();
        _ = services.AddTransient<SettingsViewModel>();
        _ = services.AddTransient<LanguageManagementViewModel>();

        return services;
    }
}
