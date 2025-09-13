using BrowserSelector.Core.Models;

namespace BrowserSelector.Library.Core.Services;

/// <summary>
/// ライブラリサービスのインターフェース.
/// テスト可能なビジネスロジックを提供.
/// </summary>
public interface ILibraryService
{
    /// <summary>
    /// ライブラリメッセージの取得.
    /// </summary>
    /// <returns>ライブラリメッセージ.</returns>
    string GetLibraryMessage();

    /// <summary>
    /// ブラウザの検証.
    /// </summary>
    /// <param name="browser">検証するブラウザ.</param>
    /// <returns>検証結果.</returns>
    Task<bool> ValidateBrowserAsync(Browser browser);

    /// <summary>
    /// URLの正規化.
    /// </summary>
    /// <param name="url">正規化するURL.</param>
    /// <returns>正規化されたURL.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "URL normalization requires string input for processing")]
    Task<string> NormalizeUrlAsync(string url);

    /// <summary>
    /// 設定の検証.
    /// </summary>
    /// <param name="settings">検証する設定.</param>
    /// <returns>検証結果.</returns>
    Task<bool> ValidateSettingsAsync(AppSettings settings);

    /// <summary>
    /// ビジュアル設定の検証.
    /// </summary>
    /// <param name="settings">検証するビジュアル設定.</param>
    /// <returns>検証結果.</returns>
    Task<bool> ValidateVisualSettingsAsync(VisualSettings settings);

    /// <summary>
    /// URLルールの検証.
    /// </summary>
    /// <param name="rule">検証するURLルール.</param>
    /// <returns>検証結果.</returns>
    Task<bool> ValidateUrlRuleAsync(UrlRule rule);

    /// <summary>
    /// ログ設定の検証.
    /// </summary>
    /// <param name="settings">検証するログ設定.</param>
    /// <returns>検証結果.</returns>
    Task<bool> ValidateLogSettingsAsync(LogSettings settings);
}
