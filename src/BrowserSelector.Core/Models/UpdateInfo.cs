// <copyright file="UpdateInfo.cs" company="BrowserSelector">
// Copyright (c) 2024 BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Core.Models
{
    /// <summary>
    /// GitHub Releasesの1リリースから解決したアップデート情報（Phase H-1で再設計）.
    /// v0.2.0までは18プロパティを持っていたが、GitHub Releasesに対応物が無く永久に既定値のままだった
    /// プロパティ（IsInstalled/IsAvailable/IsRequired/Description/Title/UpdateType/Priority/Category/Tags/Metadata）を削除し、
    /// アセット固有の情報（サイズ・チェックサム・URL）は<see cref="UpdateAsset"/>へ移した.
    /// </summary>
    public class UpdateInfo
    {
        /// <summary>
        /// Gets or sets リリースのバージョン（tag_nameから"v"とプレリリース識別子を除去してパースしたもの）.
        /// </summary>
        public Version Version { get; set; } = new Version(0, 0, 0);

        /// <summary>
        /// Gets or sets タグ名の原文（例: v0.3.0）.
        /// </summary>
        public string TagName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets リリースノート本文（GitHub APIのbody）.
        /// </summary>
        public string ReleaseNotes { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets リリースページのURL（GitHub APIのhtml_url）.
        /// </summary>
        public string ReleasePageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets リリース公開日時。取得できない場合はnull.
        /// </summary>
        public DateTimeOffset? PublishedAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether プレリリースかどうか.
        /// </summary>
        public bool IsPrerelease { get; set; }

        /// <summary>
        /// Gets or sets インストーラアセット（BrowserSelector-Setup-v*.exe）。存在しない場合はnull.
        /// </summary>
        public UpdateAsset? InstallerAsset { get; set; }

        /// <summary>
        /// Gets or sets ポータブルZIPアセット（BrowserSelector-v*-win-x64.zip）。存在しない場合はnull.
        /// </summary>
        public UpdateAsset? PortableAsset { get; set; }

        /// <summary>
        /// Gets or sets チェックサムファイルのアセット（SHA256SUMS.txt）。存在しない場合はnull.
        /// </summary>
        public UpdateAsset? ChecksumsAsset { get; set; }

        /// <summary>
        /// Gets or sets ダウンロード済み成果物のローカルパス.
        /// Installerはインストーラexeのパス、Portableは展開済みディレクトリのパス.
        /// </summary>
        public string? LocalFilePath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether ダウンロードと完全性検証が完了しているかどうか.
        /// </summary>
        public bool IsDownloaded { get; set; }
    }
}
