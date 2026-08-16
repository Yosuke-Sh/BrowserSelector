// <copyright file="UpdateAsset.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Core.Models;

/// <summary>
/// GitHub Releaseに添付された1つの成果物（インストーラ・ポータブルZIP・チェックサムファイル）を表す（Phase H-1）.
/// </summary>
/// <remarks>
/// 位置指定レコード構文ではなくプロパティを明示しているのは、StyleCopのSA1313が
/// レコードの位置パラメータを通常のパラメータとみなして誤検知するため（警告ゼロ方針との両立）.
/// </remarks>
public sealed record UpdateAsset
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAsset"/> class.
    /// </summary>
    /// <param name="name">アセットのファイル名.</param>
    /// <param name="downloadUrl">ダウンロード元URL.</param>
    /// <param name="size">アセットのバイト数.</param>
    public UpdateAsset(string name, Uri downloadUrl, long size)
    {
        Name = name;
        DownloadUrl = downloadUrl;
        Size = size;
    }

    /// <summary>
    /// Gets アセットのファイル名（例: BrowserSelector-Setup-v0.3.0.exe）.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets ダウンロード元URL。信頼済みホストであることは生成側（GitHubReleaseMapper）で検証済み.
    /// </summary>
    public Uri DownloadUrl { get; init; }

    /// <summary>
    /// Gets アセットのバイト数.
    /// </summary>
    public long Size { get; init; }

    /// <summary>
    /// Gets SHA256SUMS.txtから解決した64桁hexのハッシュ値。未解決の場合はnull.
    /// </summary>
    public string? Sha256 { get; init; }
}
