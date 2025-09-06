using BrowserSelector.Core.Enums;
using BrowserSelector.Core.Services;

namespace BrowserSelector.Presentation.Helpers;

/// <summary>
/// 多言語化対応のログヘルパークラス
/// </summary>
public static class LocalizedLogHelper
{
    private static ILocalizationService? _localizationService;

    /// <summary>
    /// ローカライゼーションサービスを設定
    /// </summary>
    public static void SetLocalizationService(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    /// <summary>
    /// アプリケーション起動開始ログ
    /// </summary>
    public static void LogApplicationStart(ILogService logService, string category)
    {
        var message = _localizationService?.GetString("Log.ApplicationStart") ?? "Application startup started";
        logService.LogTrace(message, category);
    }

    /// <summary>
    /// アプリケーション起動完了ログ
    /// </summary>
    public static void LogApplicationStartComplete(ILogService logService, string category)
    {
        var message = _localizationService?.GetString("Log.ApplicationStartComplete") ?? "Application startup completed";
        logService.LogTrace(message, category);
    }

    /// <summary>
    /// ブラウザ検出開始ログ
    /// </summary>
    public static void LogBrowserDetectionStart(ILogService logService, string category)
    {
        var message = _localizationService?.GetString("Log.BrowserDetectionStart") ?? "Browser detection started";
        logService.LogTrace(message, category);
    }

    /// <summary>
    /// ブラウザ検出完了ログ
    /// </summary>
    public static void LogBrowserDetectionComplete(ILogService logService, string category, int count, string details)
    {
        var message = _localizationService?.GetString("Log.BrowserDetectionComplete", count, details) 
                     ?? $"Browser detection completed: {count} browsers detected - {details}";
        logService.LogTrace(message, category);
    }

    /// <summary>
    /// 設定読み込み開始ログ
    /// </summary>
    public static void LogSettingsLoadStart(ILogService logService, string category)
    {
        var message = _localizationService?.GetString("Log.SettingsLoadStart") ?? "Settings loading started";
        logService.LogTrace(message, category);
    }

    /// <summary>
    /// 設定読み込み完了ログ
    /// </summary>
    public static void LogSettingsLoadComplete(ILogService logService, string category)
    {
        var message = _localizationService?.GetString("Log.SettingsLoadComplete") ?? "Settings loading completed";
        logService.LogTrace(message, category);
    }

    /// <summary>
    /// 言語変更ログ
    /// </summary>
    public static void LogLanguageChanged(ILogService logService, string category, string language)
    {
        var message = _localizationService?.GetString("Log.LanguageChanged", language) 
                     ?? $"Language changed to {language}";
        logService.LogInformation(message, category);
    }

    /// <summary>
    /// ブラウザ起動開始ログ
    /// </summary>
    public static void LogBrowserLaunchStart(ILogService logService, string category, string browserName)
    {
        var message = _localizationService?.GetString("Log.BrowserLaunchStart", browserName) 
                     ?? $"Launching {browserName}...";
        logService.LogInformation(message, category);
    }

    /// <summary>
    /// ブラウザ起動完了ログ
    /// </summary>
    public static void LogBrowserLaunchComplete(ILogService logService, string category, string browserName)
    {
        var message = _localizationService?.GetString("Log.BrowserLaunchComplete", browserName) 
                     ?? $"Launched {browserName}";
        logService.LogInformation(message, category);
    }

    /// <summary>
    /// ブラウザ起動エラーログ
    /// </summary>
    public static void LogBrowserLaunchError(ILogService logService, string category, string error)
    {
        var message = _localizationService?.GetString("Log.BrowserLaunchError", error) 
                     ?? $"Failed to launch browser: {error}";
        logService.LogError(message, category);
    }
}
