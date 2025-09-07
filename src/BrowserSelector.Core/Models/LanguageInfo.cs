// <copyright file="LanguageInfo.cs" company="BrowserSelector">
// Copyright (c) 2024 BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Core.Models
{

/// <summary>
/// 言語情報を表すモデル
/// </summary>
public class LanguageInfo
{
    /// <summary>
    /// カルチャーコード（例: en-US, ja-JP）
    /// </summary>
    public string CultureCode { get; set; } = string.Empty;

    /// <summary>
    /// 表示名（例: English, 日本語）
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public LanguageInfo()
    {
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="cultureCode">カルチャーコード</param>
    /// <param name="displayName">表示名</param>
    public LanguageInfo(string cultureCode, string displayName)
    {
        this.CultureCode = cultureCode;
        this.DisplayName = displayName;
    }

    /// <summary>
    /// 文字列表現
    /// </summary>
    public override string ToString()
    {
        return this.DisplayName;
    }

    /// <summary>
    /// 等価性の比較
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is LanguageInfo other && this.CultureCode == other.CultureCode;
    }

    /// <summary>
    /// ハッシュコードの取得
    /// </summary>
    public override int GetHashCode()
    {
        return this.CultureCode.GetHashCode();
    }
}
}
