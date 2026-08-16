// <copyright file="UpdateCheckState.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using System.Text.Json.Serialization;

namespace BrowserSelector.Infrastructure.Updates;

/// <summary>
/// アップデート確認の状態を永続化するためのモデル（Phase H-3）.
/// </summary>
/// <remarks>
/// GitHub APIは未認証だと1時間あたり60リクエストのレート制限があるため、
/// ETagによる条件付きリクエスト（304ならレート制限を消費しない）と
/// レート制限リセット時刻の記録でリクエスト数を抑える.
/// </remarks>
internal sealed class UpdateCheckState
{
    /// <summary>
    /// Gets or sets 前回のレスポンスのETag.
    /// </summary>
    [JsonPropertyName("etag")]
    public string? ETag { get; set; }

    /// <summary>
    /// Gets or sets 最後にチェックした日時（UTC）.
    /// </summary>
    [JsonPropertyName("lastCheckedUtc")]
    public DateTimeOffset? LastCheckedUtc { get; set; }

    /// <summary>
    /// Gets or sets 前回取得したリリースのタグ名（304応答時のログ用）.
    /// </summary>
    [JsonPropertyName("cachedTagName")]
    public string? CachedTagName { get; set; }

    /// <summary>
    /// Gets or sets レート制限が解除される日時（UTC）。この時刻まではチェックを行わない.
    /// </summary>
    [JsonPropertyName("rateLimitResetUtc")]
    public DateTimeOffset? RateLimitResetUtc { get; set; }
}
