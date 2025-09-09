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
        /// バージョン.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// ダウンロードURL.
        /// </summary>
        public Uri DownloadUrl { get; set; } = new Uri("https://example.com");

        /// <summary>
        /// リリースノート.
        /// </summary>
        public string ReleaseNotes { get; set; } = string.Empty;

        /// <summary>
        /// ファイルサイズ.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// チェックサム.
        /// </summary>
        public string Checksum { get; set; } = string.Empty;

        /// <summary>
        /// リリース日.
        /// </summary>
        public DateTime ReleaseDate { get; set; }

        /// <summary>
        /// ローカルファイルパス.
        /// </summary>
        public string? LocalFilePath { get; set; }

        /// <summary>
        /// ダウンロードが完了しているかどうか.
        /// </summary>
        public bool IsDownloaded { get; set; }

        /// <summary>
        /// インストールが完了しているかどうか.
        /// </summary>
        public bool IsInstalled { get; set; }

        /// <summary>
        /// アップデートが利用可能かどうか.
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// アップデートが必須かどうか.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// アップデートの説明.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// アップデートのタイトル.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// アップデートの種類.
        /// </summary>
        public string UpdateType { get; set; } = string.Empty;

        /// <summary>
        /// アップデートの優先度.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// アップデートのカテゴリ.
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// アップデートのタグ.
        /// </summary>
        public string[] Tags { get; set; } = [];

        /// <summary>
        /// アップデートのメタデータ.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = [];
    }
}
