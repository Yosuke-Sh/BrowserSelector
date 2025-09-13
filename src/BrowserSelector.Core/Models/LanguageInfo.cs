// <copyright file="LanguageInfo.cs" company="BrowserSelector">
// Copyright (c) 2024 BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Core.Models
{
    /// <summary>
    /// 言語情報を表すモデル.
    /// </summary>
    public class LanguageInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageInfo"/> class.
        /// コンストラクタ.
        /// </summary>
        public LanguageInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageInfo"/> class.
        /// コンストラクタ.
        /// </summary>
        /// <param name="cultureCode">カルチャーコード.</param>
        /// <param name="displayName">表示名.</param>
        public LanguageInfo(string cultureCode, string displayName)
        {
            CultureCode = cultureCode;
            DisplayName = displayName;
        }

        /// <summary>
        /// Gets or sets カルチャーコード（例: en-US, ja-JP）.
        /// </summary>
        public string CultureCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets 表示名（例: English, 日本語）.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 文字列表現.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return DisplayName;
        }

        /// <summary>
        /// 等価性の比較.
        /// </summary>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            return obj is LanguageInfo other && CultureCode == other.CultureCode;
        }

        /// <summary>
        /// ハッシュコードの取得.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return CultureCode.GetHashCode(StringComparison.Ordinal);
        }
    }
}
