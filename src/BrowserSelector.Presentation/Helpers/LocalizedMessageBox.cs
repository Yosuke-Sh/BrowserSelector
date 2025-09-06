using BrowserSelector.Core.Services;
using System.Windows;

namespace BrowserSelector.Presentation.Helpers;

/// <summary>
/// 多言語化対応のMessageBoxヘルパークラス
/// </summary>
public static class LocalizedMessageBox
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
    /// 確認ダイアログを表示
    /// </summary>
    public static MessageBoxResult ShowConfirm(string message, string? title = null)
    {
        var localizedTitle = title ?? _localizationService?.GetString("MessageBox.Confirm") ?? "Confirm";
        return MessageBox.Show(message, localizedTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);
    }

    /// <summary>
    /// 情報ダイアログを表示
    /// </summary>
    public static MessageBoxResult ShowInformation(string message, string? title = null)
    {
        var localizedTitle = title ?? _localizationService?.GetString("MessageBox.Information") ?? "Information";
        return MessageBox.Show(message, localizedTitle, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// 警告ダイアログを表示
    /// </summary>
    public static MessageBoxResult ShowWarning(string message, string? title = null)
    {
        var localizedTitle = title ?? _localizationService?.GetString("MessageBox.Warning") ?? "Warning";
        return MessageBox.Show(message, localizedTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    /// <summary>
    /// エラーダイアログを表示
    /// </summary>
    public static MessageBoxResult ShowError(string message, string? title = null)
    {
        var localizedTitle = title ?? _localizationService?.GetString("MessageBox.Error") ?? "Error";
        return MessageBox.Show(message, localizedTitle, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>
    /// ログクリア確認ダイアログを表示
    /// </summary>
    public static MessageBoxResult ShowLogClearConfirm()
    {
        var message = _localizationService?.GetString("MessageBox.LogClearConfirm") ?? "Do you want to clear the log file?";
        var title = _localizationService?.GetString("MessageBox.Confirm") ?? "Confirm";
        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
    }

    /// <summary>
    /// ログクリア完了ダイアログを表示
    /// </summary>
    public static MessageBoxResult ShowLogClearComplete()
    {
        var message = _localizationService?.GetString("MessageBox.LogClearComplete") ?? "Log file has been cleared.";
        var title = _localizationService?.GetString("MessageBox.Information") ?? "Information";
        return MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// 古いログ削除確認ダイアログを表示
    /// </summary>
    public static MessageBoxResult ShowOldLogDeleteConfirm()
    {
        var message = _localizationService?.GetString("MessageBox.OldLogDeleteConfirm") ?? "Do you want to delete old log files?";
        var title = _localizationService?.GetString("MessageBox.Confirm") ?? "Confirm";
        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
    }

    /// <summary>
    /// 古いログ削除完了ダイアログを表示
    /// </summary>
    public static MessageBoxResult ShowOldLogDeleteComplete()
    {
        var message = _localizationService?.GetString("MessageBox.OldLogDeleteComplete") ?? "Old log files have been deleted.";
        var title = _localizationService?.GetString("MessageBox.Information") ?? "Information";
        return MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
