// <copyright file="UpdateHostValidator.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Infrastructure.Updates;

/// <summary>
/// 更新処理で接続してよいホストかどうかを判定する（Phase H-3）.
/// </summary>
/// <remarks>
/// このアプリはコード署名を持たないため、「どこから落としたか」の保証がここの判定に集約される。
/// API呼び出し前・アセットダウンロード前に加え、リダイレクト後の最終URLでも必ず検証すること.
/// </remarks>
internal static class UpdateHostValidator
{
    private const string GitHubApiHost = "api.github.com";
    private const string GitHubHost = "github.com";

    /// <summary>
    /// アセット配信で許可するホストのサフィックス.
    /// 先頭のドットを含めて比較することで evil-githubusercontent.com のような
    /// 「サフィックス文字列としては一致するが別ドメイン」を弾く.
    /// </summary>
    private const string UserContentSuffix = ".githubusercontent.com";

    /// <summary>
    /// 指定URIが更新処理で接続を許可されたホストかどうかを判定する.
    /// </summary>
    /// <param name="uri">検証対象のURI.</param>
    /// <returns>許可される場合はtrue.</returns>
    public static bool IsAllowedHost(Uri? uri)
    {
        if (uri == null || !uri.IsAbsoluteUri)
        {
            return false;
        }

        // 平文HTTPは中間者攻撃で差し替え可能なため常に拒否する。
        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal))
        {
            return false;
        }

        string host = uri.Host;

        return string.Equals(host, GitHubApiHost, StringComparison.OrdinalIgnoreCase)
            || string.Equals(host, GitHubHost, StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(UserContentSuffix, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 文字列URLを検証し、許可ホストであれば<see cref="Uri"/>として返す.
    /// </summary>
    /// <param name="url">検証対象のURL文字列.</param>
    /// <param name="uri">許可された場合の絶対URI.</param>
    /// <returns>許可される場合はtrue.</returns>
    public static bool TryCreateAllowedUri(string? url, out Uri? uri)
    {
        uri = null;

        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? parsed) || !IsAllowedHost(parsed))
        {
            return false;
        }

        uri = parsed;
        return true;
    }
}
