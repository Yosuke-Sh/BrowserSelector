// <copyright file="ILocalizationService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using System.Globalization;

namespace BrowserSelector.Core.Services;

/// <summary>
/// 多言語対応サービスのインターフェース.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets 現在の言語を取得.
    /// </summary>
    CultureInfo CurrentCulture { get; }

    /// <summary>
    /// Gets サポートされている言語の一覧を取得.
    /// </summary>
    IEnumerable<CultureInfo> SupportedLanguages { get; }

    /// <summary>
    /// 指定されたキーの文字列を取得.
    /// </summary>
    /// <returns></returns>
    string GetString(string key);

    /// <summary>
    /// 指定されたキーの文字列を取得（パラメータ付き）.
    /// </summary>
    /// <returns></returns>
    string GetString(string key, params object[] args);

    /// <summary>
    /// 言語を設定.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task SetLanguage(CultureInfo culture);

    /// <summary>
    /// サポートされている言語の一覧を非同期で取得.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<IEnumerable<CultureInfo>> GetSupportedLanguagesAsync();

    /// <summary>
    /// 言語変更イベント
    /// </summary>
    event EventHandler<LanguageChangedEventArgs>? LanguageChanged;
}

/// <summary>
/// 言語変更イベントの引数.
/// </summary>
public class LanguageChangedEventArgs : EventArgs
{
    /// <summary>
    /// 言語変更イベントの引数を初期化.
    /// </summary>
    /// <param name="oldCulture">変更前のカルチャ.</param>
    /// <param name="newCulture">変更後のカルチャ.</param>
    public LanguageChangedEventArgs(CultureInfo oldCulture, CultureInfo newCulture)
    {
        OldCulture = oldCulture;
        NewCulture = newCulture;
    }

    /// <summary>
    /// Gets 変更前のカルチャ.
    /// </summary>
    public CultureInfo OldCulture { get; }

    /// <summary>
    /// Gets 変更後のカルチャ.
    /// </summary>
    public CultureInfo NewCulture { get; }
}
