using BrowserSelector.Core.Enums;
using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// ログサービスのインターフェース
/// </summary>
public interface ILogService
{
    /// <summary>
    /// トレースレベルのログを出力
    /// </summary>
    void LogTrace(string message, string? category = null, Exception? exception = null);

    /// <summary>
    /// デバッグレベルのログを出力
    /// </summary>
    void LogDebug(string message, string? category = null, Exception? exception = null);

    /// <summary>
    /// 情報レベルのログを出力
    /// </summary>
    void LogInformation(string message, string? category = null, Exception? exception = null);

    /// <summary>
    /// 警告レベルのログを出力
    /// </summary>
    void LogWarning(string message, string? category = null, Exception? exception = null);

    /// <summary>
    /// エラーレベルのログを出力
    /// </summary>
    void LogError(string message, string? category = null, Exception? exception = null);

    /// <summary>
    /// 致命的エラーレベルのログを出力
    /// </summary>
    void LogCritical(string message, string? category = null, Exception? exception = null);

    /// <summary>
    /// 指定されたレベルのログを出力
    /// </summary>
    void Log(LogLevel level, string message, string? category = null, Exception? exception = null);

    /// <summary>
    /// 詳細情報付きのログを出力
    /// </summary>
    void LogDetailed(LogLevel level, string message, string? category = null, 
                    string? eventId = null, string? requestTarget = null, string? userInfo = null,
                    string? processTarget = null, string? processAction = null, string? processResult = null,
                    Exception? exception = null);

    /// <summary>
    /// ログ設定を更新
    /// </summary>
    void UpdateSettings(LogSettings settings);

    /// <summary>
    /// ログファイルをクリア
    /// </summary>
    void ClearLogs();

    /// <summary>
    /// 古いログファイルを削除
    /// </summary>
    void CleanupOldLogs();

    /// <summary>
    /// ログファイルの内容を取得
    /// </summary>
    string GetLogContent(int maxLines = 1000);

    /// <summary>
    /// ログファイルのパスを取得
    /// </summary>
    string GetLogFilePath();
}
