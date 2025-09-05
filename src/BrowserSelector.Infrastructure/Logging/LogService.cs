using System.Text;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using BrowserSelector.Core.Enums;
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;

namespace BrowserSelector.Infrastructure.Logging;

/// <summary>
/// ログサービスの実装
/// </summary>
public class LogService : ILogService
{
    private readonly object _lockObject = new();
    private LogSettings _settings;
    private readonly string _defaultLogFolder;
    private int _eventCounter = 0;

    public LogService()
    {
        _settings = new LogSettings();
        _defaultLogFolder = LogSettings.GetDefaultLogFolder();
        _settings.LogOutputFolder = _defaultLogFolder;
        
        // デフォルトはLogSettingsのレベル（Information）を使用
        
        // ログフォルダが存在しない場合は作成
        EnsureLogDirectoryExists();
        
        // 起動時のログ（INFO）
        LogInformation("LogService初期化完了", "LogService");
    }

    /// <summary>
    /// トレースレベルのログを出力
    /// </summary>
    public void LogTrace(string message, string? category = null, Exception? exception = null)
    {
        Log(LogLevel.Trace, message, category, exception);
    }

    /// <summary>
    /// デバッグレベルのログを出力
    /// </summary>
    public void LogDebug(string message, string? category = null, Exception? exception = null)
    {
        Log(LogLevel.Debug, message, category, exception);
    }

    /// <summary>
    /// 情報レベルのログを出力
    /// </summary>
    public void LogInformation(string message, string? category = null, Exception? exception = null)
    {
        Log(LogLevel.Information, message, category, exception);
    }

    /// <summary>
    /// 警告レベルのログを出力
    /// </summary>
    public void LogWarning(string message, string? category = null, Exception? exception = null)
    {
        Log(LogLevel.Warning, message, category, exception);
    }

    /// <summary>
    /// エラーレベルのログを出力
    /// </summary>
    public void LogError(string message, string? category = null, Exception? exception = null)
    {
        Log(LogLevel.Error, message, category, exception);
    }

    /// <summary>
    /// 致命的エラーレベルのログを出力
    /// </summary>
    public void LogCritical(string message, string? category = null, Exception? exception = null)
    {
        Log(LogLevel.Critical, message, category, exception);
    }

    /// <summary>
    /// 指定されたレベルのログを出力
    /// </summary>
    public void Log(LogLevel level, string message, string? category = null, Exception? exception = null)
    {
        LogDetailed(level, message, category, null, null, null, null, null, null, exception);
    }

    /// <summary>
    /// 詳細情報付きのログを出力
    /// </summary>
    public void LogDetailed(LogLevel level, string message, string? category = null, 
                           string? eventId = null, string? requestTarget = null, string? userInfo = null,
                           string? processTarget = null, string? processAction = null, string? processResult = null,
                           Exception? exception = null)
    {
        if (!_settings.EnableLogging || level < _settings.LogLevel)
        {
            return;
        }

        try
        {
            var logMessage = FormatDetailedLogMessage(level, message, category, eventId, requestTarget, userInfo, processTarget, processAction, processResult, exception);
            
            // コンソール出力
            if (_settings.EnableConsoleLogging)
            {
                WriteToConsole(level, logMessage);
            }
            
            // ファイル出力
            if (_settings.EnableFileLogging)
            {
                WriteToFile(logMessage);
            }
        }
        catch (Exception)
        {
            // ログ出力中のエラーは無視
        }
    }

    /// <summary>
    /// ログ設定を更新
    /// </summary>
    public void UpdateSettings(LogSettings settings)
    {
        lock (_lockObject)
        {
            _settings = settings;
            
            // ログフォルダが存在しない場合は作成
            EnsureLogDirectoryExists();
            
            LogInformation("ログ設定を更新しました", "LogService");
        }
    }

    /// <summary>
    /// ログファイルをクリア
    /// </summary>
    public void ClearLogs()
    {
        try
        {
            var logFilePath = _settings.GetLogFilePath();
            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
                LogInformation("ログファイルをクリアしました", "LogService");
            }
        }
        catch (Exception ex)
        {
            LogError($"ログファイルのクリアに失敗しました: {ex.Message}", "LogService", ex);
        }
    }

    /// <summary>
    /// 古いログファイルを削除
    /// </summary>
    public void CleanupOldLogs()
    {
        try
        {
            if (!Directory.Exists(_settings.LogOutputFolder))
                return;

            var cutoffDate = DateTime.Now.AddDays(-_settings.LogRetentionDays);
            var logFiles = Directory.GetFiles(_settings.LogOutputFolder, $"{_settings.LogFilePrefix}_*.{_settings.LogFileSuffix}");

            foreach (var logFile in logFiles)
            {
                var fileInfo = new FileInfo(logFile);
                if (fileInfo.CreationTime < cutoffDate)
                {
                    File.Delete(logFile);
                    LogInformation($"古いログファイルを削除しました: {fileInfo.Name}", "LogService");
                }
            }
        }
        catch (Exception ex)
        {
            LogError($"古いログファイルの削除に失敗しました: {ex.Message}", "LogService", ex);
        }
    }

    /// <summary>
    /// ログファイルの内容を取得
    /// </summary>
    public string GetLogContent(int maxLines = 1000)
    {
        try
        {
            var logFilePath = _settings.GetLogFilePath();
            if (!File.Exists(logFilePath))
                return "ログファイルが存在しません。";

            var lines = File.ReadAllLines(logFilePath);
            var recentLines = lines.TakeLast(maxLines).ToArray();
            
            return string.Join(Environment.NewLine, recentLines);
        }
        catch (Exception ex)
        {
            return $"ログファイルの読み込みに失敗しました: {ex.Message}";
        }
    }

    /// <summary>
    /// ログファイルのパスを取得
    /// </summary>
    public string GetLogFilePath()
    {
        return _settings.GetLogFilePath();
    }

    /// <summary>
    /// ログメッセージをフォーマット
    /// </summary>
    private string FormatLogMessage(LogLevel level, string message, string? category, Exception? exception)
    {
        return FormatDetailedLogMessage(level, message, category, null, null, null, null, null, null, exception);
    }

    /// <summary>
    /// 詳細ログメッセージをフォーマット
    /// </summary>
    private string FormatDetailedLogMessage(LogLevel level, string message, string? category, 
                                          string? eventId, string? requestTarget, string? userInfo,
                                          string? processTarget, string? processAction, string? processResult,
                                          Exception? exception)
    {
        var timestamp = DateTime.Now.ToString(_settings.TimestampFormat);
        var levelText = GetLogLevelShortName(level);
        var categoryText = string.IsNullOrEmpty(category) ? "General" : category;
        var eventIdText = string.IsNullOrEmpty(eventId) ? GetNextEventId() : eventId;
        // 空値は出力しない（N/Aは使わない）
        var requestTargetText = string.IsNullOrWhiteSpace(requestTarget) ? string.Empty : requestTarget;
        var userInfoText = string.IsNullOrWhiteSpace(userInfo) ? string.Empty : userInfo;
        var processTargetText = string.IsNullOrWhiteSpace(processTarget) ? string.Empty : processTarget;
        var processActionText = string.IsNullOrWhiteSpace(processAction) ? string.Empty : processAction;
        var processResultText = string.IsNullOrWhiteSpace(processResult) ? string.Empty : processResult;
        
        var logMessage = _settings.LogMessageTemplate
            .Replace("{Timestamp}", timestamp)
            .Replace("{Level}", levelText)
            .Replace("{EventId}", eventIdText)
            .Replace("{Category}", categoryText)
            .Replace("{RequestTarget}", requestTargetText)
            .Replace("{UserInfo}", userInfoText)
            .Replace("{ProcessTarget}", processTargetText)
            .Replace("{ProcessAction}", processActionText)
            .Replace("{ProcessResult}", processResultText)
            .Replace("{Message}", message);

        if (exception != null)
        {
            logMessage += $"{Environment.NewLine}例外: {exception.Message}";
            if (!string.IsNullOrEmpty(exception.StackTrace))
            {
                logMessage += $"{Environment.NewLine}スタックトレース: {exception.StackTrace}";
            }
        }

        // 余分な空白を正規化（連続スペースを1つに）
        logMessage = Regex.Replace(logMessage, @"\s{2,}", " ").Trim();
        return logMessage;
    }

    /// <summary>
    /// 次のイベントIDを取得
    /// </summary>
    private string GetNextEventId()
    {
        return $"EVT{Interlocked.Increment(ref _eventCounter):D6}";
    }

    /// <summary>
    /// ログレベルの短縮形を取得
    /// </summary>
    private string GetLogLevelShortName(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => "TRACE",
            LogLevel.Debug => "DEBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "ERROR",
            LogLevel.Critical => "FATAL",
            _ => level.ToString().ToUpper()
        };
    }

    /// <summary>
    /// コンソールにログを出力
    /// </summary>
    private void WriteToConsole(LogLevel level, string message)
    {
        var originalColor = Console.ForegroundColor;
        
        try
        {
            Console.ForegroundColor = level switch
            {
                LogLevel.Trace => ConsoleColor.Gray,
                LogLevel.Debug => ConsoleColor.DarkGray,
                LogLevel.Information => ConsoleColor.White,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Critical => ConsoleColor.DarkRed,
                _ => ConsoleColor.White
            };
            
            Console.WriteLine(message);
        }
        finally
        {
            Console.ForegroundColor = originalColor;
        }
    }

    /// <summary>
    /// ファイルにログを出力
    /// </summary>
    private void WriteToFile(string message)
    {
        try
        {
            var logFilePath = _settings.GetLogFilePath();
            var logMessage = message + Environment.NewLine;
            
            // ファイルサイズチェック
            CheckAndRotateLogFile(logFilePath);
            
            // ログファイルに追記
            File.AppendAllText(logFilePath, logMessage, Encoding.UTF8);
        }
        catch (Exception)
        {
            // ログファイル出力エラーは無視
        }
    }

    /// <summary>
    /// ログファイルのサイズチェックとローテーション
    /// </summary>
    private void CheckAndRotateLogFile(string logFilePath)
    {
        try
        {
            if (!File.Exists(logFilePath))
                return;

            var fileInfo = new FileInfo(logFilePath);
            var maxSizeBytes = _settings.MaxLogFileSize * 1024 * 1024; // MB to bytes

            if (fileInfo.Length > maxSizeBytes)
            {
                var backupPath = logFilePath.Replace($".{_settings.LogFileSuffix}", 
                    $"_{DateTime.Now:yyyyMMdd_HHmmss}.{_settings.LogFileSuffix}");
                
                File.Move(logFilePath, backupPath);
                LogInformation($"ログファイルをローテーションしました: {backupPath}", "LogService");
            }
        }
        catch (Exception)
        {
            // ログファイルローテーションエラーは無視
        }
    }

    /// <summary>
    /// ログディレクトリの存在確認と作成
    /// </summary>
    private void EnsureLogDirectoryExists()
    {
        try
        {
            if (!Directory.Exists(_settings.LogOutputFolder))
            {
                Directory.CreateDirectory(_settings.LogOutputFolder);
            }
        }
        catch (Exception)
        {
            // ログディレクトリ作成エラーは無視
        }
    }
}
