// <copyright file="UpdateInfo.cs" company="BrowserSelector">
// Copyright (c) 2024 BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Core.Models
{
    /// <summary>
    /// アップデート情報.
    /// </summary>
    public class UpdateInfo
    {
        /// <summary>
        /// Gets or sets バージョン.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets ダウンロードURL.
        /// </summary>
        public Uri DownloadUrl { get; set; } = new Uri("https://example.com", UriKind.Absolute);

        /// <summary>
        /// Gets or sets リリースノート.
        /// </summary>
        public string ReleaseNotes { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets ファイルサイズ.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets or sets チェックサム.
        /// </summary>
        public string Checksum { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets リリース日.
        /// </summary>
        public DateTime ReleaseDate { get; set; }

        /// <summary>
        /// Gets or sets ローカルファイルパス.
        /// </summary>
        public string? LocalFilePath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether ダウンロードが完了しているかどうか.
        /// </summary>
        public bool IsDownloaded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether インストールが完了しているかどうか.
        /// </summary>
        public bool IsInstalled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether アップデートが利用可能かどうか.
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether アップデートが必須かどうか.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets アップデートの説明.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets アップデートのタイトル.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets アップデートの種類.
        /// </summary>
        public string UpdateType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets アップデートの優先度.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets アップデートのカテゴリ.
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets アップデートのタグ.
        /// </summary>
        public string[] Tags { get; set; } = [];

        /// <summary>
        /// Gets or sets アップデートのメタデータ.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = [];
    }
}
