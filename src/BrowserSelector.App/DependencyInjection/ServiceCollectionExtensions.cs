using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.Localization;
using BrowserSelector.Infrastructure.Services;
using BrowserSelector.Infrastructure.SystemIntegration;
using BrowserSelector.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BrowserSelector.App.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBrowserSelectorServices(this IServiceCollection services)
    {
        // Core Services
        services.AddScoped<IBrowserService, BrowserService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<ILocalizationService, LocalizationService>();
        services.AddScoped<IUrlService, UrlService>();
        
        // Infrastructure Services
        services.AddScoped<IRegistryService, WindowsRegistryService>();
        
        // Presentation Services
        services.AddTransient<MainViewModel>();
        
        return services;
    }
}
