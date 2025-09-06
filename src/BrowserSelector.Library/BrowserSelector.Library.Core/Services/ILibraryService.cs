using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;

namespace BrowserSelector.Library.Core.Services;

/// <summary>
/// ライブラリサービスのインターフェース
/// テスト可能なビジネスロジックを提供
/// </summary>
public interface ILibraryService
{
    /// <summary>
    /// ライブラリメッセージの取得
    /// </summary>
    string GetLibraryMessage();

    /// <summary>
    /// ブラウザの検証
    /// </summary>
    Task<bool> ValidateBrowserAsync(Browser browser);

    /// <summary>
    /// URLの正規化
    /// </summary>
    Task<string> NormalizeUrlAsync(string url);

    /// <summary>
    /// 設定の検証
    /// </summary>
    Task<bool> ValidateSettingsAsync(AppSettings settings);

    /// <summary>
    /// ビジュアル設定の検証
    /// </summary>
    Task<bool> ValidateVisualSettingsAsync(VisualSettings settings);

    /// <summary>
    /// URLルールの検証
    /// </summary>
    Task<bool> ValidateUrlRuleAsync(UrlRule rule);

    /// <summary>
    /// ログ設定の検証
    /// </summary>
    Task<bool> ValidateLogSettingsAsync(LogSettings settings);
}
