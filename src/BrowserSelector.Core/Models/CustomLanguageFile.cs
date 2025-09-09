// <copyright file="CustomLanguageFile.cs" company="BrowserSelector">
// Copyright (c) 2024 BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Core.Models
{
    /// <summary>
    /// カスタム言語ファイルのモデル.
    /// </summary>
    public class CustomLanguageFile
    {
        /// <summary>
        /// Gets or sets カルチャーコード（例: zh-CN, ko-KR）.
        /// </summary>
        public string CultureCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets 表示名（例: 中文 (简体), 한국어）.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets リソース辞書（キー: リソースキー, 値: 翻訳文字列）.
        /// </summary>
        public Dictionary<string, string> Resources { get; set; } =[];

        /// <summary>
        /// Gets or sets ファイルの作成日時.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets ファイルの更新日時.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets ファイルのバージョン.
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// Gets or sets 説明.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets 作成者.
        /// </summary>
        public string? Author { get; set; }
    }
}
