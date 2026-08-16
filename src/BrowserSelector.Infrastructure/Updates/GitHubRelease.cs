// <copyright file="GitHubRelease.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using System.Text.Json.Serialization;

namespace BrowserSelector.Infrastructure.Updates;

/// <summary>
/// GitHub REST API（GET /repos/{owner}/{repo}/releases/latest）のレスポンスDTO（Phase H-2）.
/// </summary>
/// <remarks>
/// GitHub APIのスキーマは外部仕様であり、公開API面に晒すと先方の変更がこちらの破壊的変更になるため
/// 意図的にinternalとしている。アプリ内で扱うのは<see cref="Core.Models.UpdateInfo"/>へ変換したあとの形.
/// </remarks>
internal sealed record GitHubRelease
{
    /// <summary>
    /// Gets タグ名（例: v0.3.0）.
    /// </summary>
    [JsonPropertyName("tag_name")]
    public string? TagName { get; init; }

    /// <summary>
    /// Gets リリース名.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// Gets リリースノート本文.
    /// </summary>
    [JsonPropertyName("body")]
    public string? Body { get; init; }

    /// <summary>
    /// Gets a value indicating whether 下書きかどうか.
    /// </summary>
    [JsonPropertyName("draft")]
    public bool Draft { get; init; }

    /// <summary>
    /// Gets a value indicating whether プレリリースかどうか.
    /// </summary>
    [JsonPropertyName("prerelease")]
    public bool Prerelease { get; init; }

    /// <summary>
    /// Gets 公開日時.
    /// </summary>
    [JsonPropertyName("published_at")]
    public DateTimeOffset? PublishedAt { get; init; }

    /// <summary>
    /// Gets リリースページのURL.
    /// </summary>
    [JsonPropertyName("html_url")]
    public string? HtmlUrl { get; init; }

    /// <summary>
    /// Gets 添付アセットの一覧.
    /// </summary>
    [JsonPropertyName("assets")]
    public IReadOnlyList<GitHubReleaseAsset>? Assets { get; init; }
}

/// <summary>
/// GitHub Releaseに添付された1アセットのDTO（Phase H-2）.
/// </summary>
internal sealed record GitHubReleaseAsset
{
    /// <summary>
    /// Gets アセットのファイル名.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// Gets ダウンロードURL.
    /// </summary>
    [JsonPropertyName("browser_download_url")]
    public string? BrowserDownloadUrl { get; init; }

    /// <summary>
    /// Gets バイト数.
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; init; }

    /// <summary>
    /// Gets Content-Type.
    /// </summary>
    [JsonPropertyName("content_type")]
    public string? ContentType { get; init; }
}
