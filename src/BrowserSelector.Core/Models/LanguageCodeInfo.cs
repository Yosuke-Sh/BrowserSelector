// <copyright file="LanguageCodeInfo.cs" company="BrowserSelector">
// Copyright (c) 2024 BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Core.Models
{
    /// <summary>
    /// 言語コード情報.
    /// </summary>
    public class LanguageCodeInfo
    {
        /// <summary>
        /// 言語コード情報を初期化.
        /// </summary>
        /// <param name="code">言語コード.</param>
        /// <param name="displayName">表示名.</param>
        /// <param name="nativeName">ネイティブ名.</param>
        public LanguageCodeInfo(string code, string displayName, string nativeName)
        {
            Code = code;
            DisplayName = displayName;
            NativeName = nativeName;
        }

        /// <summary>
        /// Gets or sets 言語コード.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets 表示名.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets ネイティブ名.
        /// </summary>
        public string NativeName { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{DisplayName} ({Code})";
        }
    }
}
