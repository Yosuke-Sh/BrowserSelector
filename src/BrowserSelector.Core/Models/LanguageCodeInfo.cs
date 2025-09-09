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
        public string Code { get; set; }

        public string DisplayName { get; set; }

        public string NativeName { get; set; }

        public LanguageCodeInfo(string code, string displayName, string nativeName)
        {
            Code = code;
            DisplayName = displayName;
            NativeName = nativeName;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{DisplayName} ({Code})";
        }
    }
}
