using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Library.Core.Services;
using System.Text.RegularExpressions;

namespace BrowserSelector.Library.Infrastructure.Services;

/// <summary>
/// ライブラリサービスの実装
/// テスト可能なビジネスロジックを提供
/// </summary>
public class LibraryService : ILibraryService
{
    private readonly ILogService? _logService;

    public LibraryService(ILogService? logService = null)
    {
        _logService = logService;
    }

    /// <summary>
    /// ライブラリメッセージの取得
    /// </summary>
    public string GetLibraryMessage()
    {
        return "Hello from BrowserSelector.Library.Infrastructure!";
    }

    /// <summary>
    /// ブラウザの検証
    /// </summary>
    public async Task<bool> ValidateBrowserAsync(Browser browser)
    {
        _logService?.LogTrace($"ブラウザ検証開始: {browser.Name}", "LibraryService");
        
        try
        {
            // 基本検証
            if (string.IsNullOrWhiteSpace(browser.Name))
            {
                _logService?.LogWarning("ブラウザ名が空です", "LibraryService");
                return false;
            }

            if (string.IsNullOrWhiteSpace(browser.ExecutablePath))
            {
                _logService?.LogWarning($"ブラウザ '{browser.Name}' の実行パスが空です", "LibraryService");
                return false;
            }

            // 実行ファイルの存在確認
            if (!File.Exists(browser.ExecutablePath))
            {
                _logService?.LogWarning($"ブラウザ '{browser.Name}' の実行ファイルが存在しません: {browser.ExecutablePath}", "LibraryService");
                return false;
            }

            // 拡張子の確認
            string extension = Path.GetExtension(browser.ExecutablePath).ToLowerInvariant();
            if (extension != ".exe")
            {
                _logService?.LogWarning($"ブラウザ '{browser.Name}' の実行ファイルが無効です: {browser.ExecutablePath}", "LibraryService");
                return false;
            }

            _logService?.LogInformation($"ブラウザ検証成功: {browser.Name}", "LibraryService");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"ブラウザ検証エラー: {ex.Message}", "LibraryService", ex);
            return false;
        }
    }

    /// <summary>
    /// URLの正規化
    /// </summary>
    public async Task<string> NormalizeUrlAsync(string url)
    {
        _logService?.LogTrace($"URL正規化開始: {url}", "LibraryService");
        
        try
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                _logService?.LogWarning("URLが空です", "LibraryService");
                return string.Empty;
            }

            string normalizedUrl = url.Trim();

            // プロトコルの追加
            if (!normalizedUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !normalizedUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
                !normalizedUrl.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase) &&
                !normalizedUrl.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                normalizedUrl = "https://" + normalizedUrl;
                _logService?.LogDebug($"プロトコルを追加: {url} -> {normalizedUrl}", "LibraryService");
            }

            // URLの基本検証
            if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out Uri? uri))
            {
                _logService?.LogWarning($"無効なURL形式: {url}", "LibraryService");
                return url; // 元のURLを返す
            }

            _logService?.LogInformation($"URL正規化成功: {url} -> {normalizedUrl}", "LibraryService");
            return normalizedUrl;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"URL正規化エラー: {ex.Message}", "LibraryService", ex);
            return url; // エラー時は元のURLを返す
        }
    }

    /// <summary>
    /// 設定の検証
    /// </summary>
    public async Task<bool> ValidateSettingsAsync(AppSettings settings)
    {
        _logService?.LogTrace("アプリケーション設定検証開始", "LibraryService");
        
        try
        {
            if (settings == null)
            {
                _logService?.LogWarning("設定がnullです", "LibraryService");
                return false;
            }

            // 言語設定の検証
            if (string.IsNullOrWhiteSpace(settings.Language))
            {
                _logService?.LogWarning("言語設定が空です", "LibraryService");
                return false;
            }

            // 言語コードの形式確認
            if (!Regex.IsMatch(settings.Language, @"^[a-z]{2}-[A-Z]{2}$"))
            {
                _logService?.LogWarning($"無効な言語コード形式: {settings.Language}", "LibraryService");
                return false;
            }

            // カスタムプロトコルの検証
            if (!string.IsNullOrWhiteSpace(settings.CustomProtocol))
            {
                if (!Regex.IsMatch(settings.CustomProtocol, @"^[a-z][a-z0-9]*$"))
                {
                    _logService?.LogWarning($"無効なカスタムプロトコル形式: {settings.CustomProtocol}", "LibraryService");
                    return false;
                }
            }

            _logService?.LogInformation("アプリケーション設定検証成功", "LibraryService");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"設定検証エラー: {ex.Message}", "LibraryService", ex);
            return false;
        }
    }

    /// <summary>
    /// ビジュアル設定の検証
    /// </summary>
    public async Task<bool> ValidateVisualSettingsAsync(VisualSettings settings)
    {
        _logService?.LogTrace("ビジュアル設定検証開始", "LibraryService");
        
        try
        {
            if (settings == null)
            {
                _logService?.LogWarning("ビジュアル設定がnullです", "LibraryService");
                return false;
            }

            // ウィンドウサイズの検証
            if (settings.InitialWindowWidth < 400 || settings.InitialWindowWidth > 2000)
            {
                _logService?.LogWarning($"無効なウィンドウ幅: {settings.InitialWindowWidth}", "LibraryService");
                return false;
            }

            if (settings.InitialWindowHeight < 300 || settings.InitialWindowHeight > 1500)
            {
                _logService?.LogWarning($"無効なウィンドウ高さ: {settings.InitialWindowHeight}", "LibraryService");
                return false;
            }

            // グラデーション設定の検証
            if (settings.UseBackgroundGradient)
            {
                if (settings.GradientStartColor == settings.GradientEndColor)
                {
                    _logService?.LogWarning("グラデーションの開始色と終了色が同じです", "LibraryService");
                    return false;
                }
            }

            _logService?.LogInformation("ビジュアル設定検証成功", "LibraryService");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"ビジュアル設定検証エラー: {ex.Message}", "LibraryService", ex);
            return false;
        }
    }

    /// <summary>
    /// URLルールの検証
    /// </summary>
    public async Task<bool> ValidateUrlRuleAsync(UrlRule rule)
    {
        _logService?.LogTrace($"URLルール検証開始: {rule.Pattern}", "LibraryService");
        
        try
        {
            if (rule == null)
            {
                _logService?.LogWarning("URLルールがnullです", "LibraryService");
                return false;
            }

            // パターンの検証
            if (string.IsNullOrWhiteSpace(rule.Pattern))
            {
                _logService?.LogWarning("URLルールのパターンが空です", "LibraryService");
                return false;
            }

            // 正規表現の検証
            try
            {
                _ = new Regex(rule.Pattern);
            }
            catch (ArgumentException)
            {
                _logService?.LogWarning($"無効な正規表現パターン: {rule.Pattern}", "LibraryService");
                return false;
            }

            // ブラウザ名の検証
            if (string.IsNullOrWhiteSpace(rule.BrowserName))
            {
                _logService?.LogWarning("URLルールのブラウザ名が空です", "LibraryService");
                return false;
            }

            // 優先度の検証
            if (rule.Priority < 1 || rule.Priority > 100)
            {
                _logService?.LogWarning($"無効な優先度: {rule.Priority}", "LibraryService");
                return false;
            }

            _logService?.LogInformation($"URLルール検証成功: {rule.Pattern}", "LibraryService");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"URLルール検証エラー: {ex.Message}", "LibraryService", ex);
            return false;
        }
    }

    /// <summary>
    /// ログ設定の検証
    /// </summary>
    public async Task<bool> ValidateLogSettingsAsync(LogSettings settings)
    {
        _logService?.LogTrace("ログ設定検証開始", "LibraryService");
        
        try
        {
            if (settings == null)
            {
                _logService?.LogWarning("ログ設定がnullです", "LibraryService");
                return false;
            }

            // ログレベルの検証
            if (settings.LogLevel < BrowserSelector.Core.Enums.LogLevel.Trace || 
                settings.LogLevel > BrowserSelector.Core.Enums.LogLevel.Critical)
            {
                _logService?.LogWarning($"無効なログレベル: {settings.LogLevel}", "LibraryService");
                return false;
            }

            // ファイルサイズの検証
            if (settings.MaxLogFileSize < 1 || settings.MaxLogFileSize > 1000)
            {
                _logService?.LogWarning($"無効なログファイルサイズ: {settings.MaxLogFileSize}MB", "LibraryService");
                return false;
            }

            // 保持期間の検証
            if (settings.LogRetentionDays < 1 || settings.LogRetentionDays > 365)
            {
                _logService?.LogWarning($"無効なログ保持期間: {settings.LogRetentionDays}日", "LibraryService");
                return false;
            }

            _logService?.LogInformation("ログ設定検証成功", "LibraryService");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"ログ設定検証エラー: {ex.Message}", "LibraryService", ex);
            return false;
        }
    }
}
