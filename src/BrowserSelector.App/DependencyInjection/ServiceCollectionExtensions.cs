using System.Net;
using System.Net.Http;
using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.Localization;
using BrowserSelector.Infrastructure.Services;
using BrowserSelector.Infrastructure.SystemIntegration;
using BrowserSelector.Infrastructure.Updates;
using BrowserSelector.Presentation.Services;
using BrowserSelector.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BrowserSelector.App.DependencyInjection;

/// <summary>
/// 依存性注入のためのサービスコレクション拡張メソッドを提供します.
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// BrowserSelectorアプリケーションに必要なすべてのサービスをサービスコレクションに追加します.
    /// </summary>
    /// <param name="services">サービスコレクション.</param>
    /// <returns>設定されたサービスコレクション.</returns>
    public static IServiceCollection AddBrowserSelectorServices(this IServiceCollection services)
    {
        // Core Services
        // WPFはルートのDIプロバイダから解決するため、Scopedは実質Singletonと同じ生存期間になり誤解を招く。
        // アプリ生存期間で共有する意図を明確にするためSingletonとして登録する。
        _ = services.AddSingleton<IBrowserService, BrowserService>();
        _ = services.AddSingleton<ISettingsService>(provider =>
            new SettingsService(provider.GetRequiredService<ILogService>()));
        _ = services.AddSingleton<ICustomLanguageService>(provider =>
            new CustomLanguageService(provider.GetRequiredService<ILogService>()));
        _ = services.AddSingleton<ILocalizationService>(provider =>
            new LocalizationService(provider.GetRequiredService<ICustomLanguageService>(), provider.GetRequiredService<ILogService>()));
        _ = services.AddSingleton<IUrlService>(provider =>
            new UrlService(provider.GetRequiredService<ISettingsService>(), provider.GetRequiredService<ILogService>()));
        _ = services.AddSingleton<IUrlRuleService>(provider =>
            new UrlRuleService(provider.GetRequiredService<ILogService>()));

        // Infrastructure Services
        _ = services.AddSingleton<ILogService, BrowserSelector.Infrastructure.Logging.LogService>();
        _ = services.AddSingleton<IIconCacheService>(provider =>
            new IconCacheService(provider.GetRequiredService<ILogService>()));
        _ = services.AddSingleton<IRegistryService>(provider =>
            new WindowsRegistryService(provider.GetRequiredService<ILogService>()));
        _ = services.AddSingleton<IProtocolHandler, ProtocolHandler>();
        // アップデート用の名前付きHttpClient（Phase H-3）。
        // Singleton HttpClientではなくIHttpClientFactoryを使うのは、テストで
        // ConfigurePrimaryHttpMessageHandlerにより本番と同じ経路へスタブを差し込めるようにするため。
        _ = services.AddHttpClient(UpdateService.HttpClientName, client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"BrowserSelector/{Core.AppInfo.CurrentVersion}");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 5,
            AutomaticDecompression = DecompressionMethods.All,
        });

        _ = services.AddSingleton<IUpdateService>(provider =>
            new UpdateService(
                provider.GetRequiredService<IHttpClientFactory>(),
                provider.GetRequiredService<ISettingsService>(),
                provider.GetRequiredService<ILogService>()));
        _ = services.AddSingleton<IExternalLinkService>(provider =>
            new ExternalLinkService(provider.GetRequiredService<IBrowserService>(), provider.GetRequiredService<ILogService>()));

        // トレイ常駐時の自動終了をトレイ格納に差し替えるための抽象化（SystemIntegration.ShellCloseService）。
        // 具象型でも解決できるよう単一インスタンスとして両方登録し、App.SetupTrayIcon側で
        // TrayIconManager確定後にAttachTrayIconを呼び出す。
        _ = services.AddSingleton<SystemIntegration.ShellCloseService>();
        _ = services.AddSingleton<IShellCloseService>(provider =>
            provider.GetRequiredService<SystemIntegration.ShellCloseService>());

        // Presentation Services (WPFのApplication.Currentに依存するため型はPresentation層)
        _ = services.AddSingleton<IThemeService>(provider =>
            new ThemeService(provider.GetRequiredService<ILogService>()));

        // Presentation Services
        _ = services.AddTransient<MainViewModel>();
        _ = services.AddTransient<SettingsViewModel>();
        _ = services.AddTransient<LanguageManagementViewModel>();

        return services;
    }
}
