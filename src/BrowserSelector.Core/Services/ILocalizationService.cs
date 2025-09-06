using System.Globalization;

namespace BrowserSelector.Core.Services;

/// <summary>
/// 多言語対応サービスのインターフェース
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// 指定されたキーの文字列を取得
    /// </summary>
    string GetString(string key);

    /// <summary>
    /// 指定されたキーの文字列を取得（パラメータ付き）
    /// </summary>
    string GetString(string key, params object[] args);

    /// <summary>
    /// 言語を設定
    /// </summary>
    void SetLanguage(CultureInfo culture);

    /// <summary>
    /// 現在の言語を取得
    /// </summary>
    CultureInfo CurrentCulture { get; }

    /// <summary>
    /// サポートされている言語の一覧を取得
    /// </summary>
    IEnumerable<CultureInfo> SupportedLanguages { get; }

    /// <summary>
    /// サポートされている言語の一覧を非同期で取得
    /// </summary>
    Task<IEnumerable<CultureInfo>> GetSupportedLanguagesAsync();

    /// <summary>
    /// 言語変更イベント
    /// </summary>
    event EventHandler<LanguageChangedEventArgs>? LanguageChanged;
}

/// <summary>
/// 言語変更イベントの引数
/// </summary>
public class LanguageChangedEventArgs : EventArgs
{
    public CultureInfo OldCulture { get; }
    public CultureInfo NewCulture { get; }

    public LanguageChangedEventArgs(CultureInfo oldCulture, CultureInfo newCulture)
    {
        OldCulture = oldCulture;
        NewCulture = newCulture;
    }
}
