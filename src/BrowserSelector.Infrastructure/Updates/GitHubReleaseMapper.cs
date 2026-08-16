// <copyright file="GitHubReleaseMapper.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using BrowserSelector.Core.Models;

namespace BrowserSelector.Infrastructure.Updates;

/// <summary>
/// GitHub APIのレスポンス（<see cref="GitHubRelease"/>）をアプリ内モデル（<see cref="UpdateInfo"/>）へ変換する（Phase H-2）.
/// </summary>
/// <remarks>
/// v0.2.0までは GitHub のレスポンスを <c>JsonSerializer.Deserialize&lt;UpdateInfo&gt;()</c> で直接受けており、
/// <c>tag_name</c> に対応するプロパティが無いためVersionが常に空文字となり、更新が原理的に検出されなかった。
/// この変換層はその不具合の根本対応にあたる.
/// </remarks>
internal static class GitHubReleaseMapper
{
    /// <summary>
    /// インストーラアセットのファイル名の接頭辞（release.ymlの命名規約と一致）.
    /// </summary>
    private const string InstallerPrefix = "BrowserSelector-Setup-";

    /// <summary>
    /// ポータブルZIPアセットのファイル名の接尾辞.
    /// </summary>
    private const string PortableSuffix = "-win-x64.zip";

    /// <summary>
    /// ポータブルZIPアセットのファイル名の接頭辞.
    /// </summary>
    private const string PortablePrefix = "BrowserSelector-v";

    /// <summary>
    /// チェックサムファイルの名前.
    /// </summary>
    private const string ChecksumsFileName = "SHA256SUMS.txt";

    /// <summary>
    /// GitHubのリリース情報を<see cref="UpdateInfo"/>へ変換する.
    /// </summary>
    /// <param name="release">GitHub APIのレスポンス.</param>
    /// <param name="updateInfo">変換結果.</param>
    /// <returns>変換に成功した場合はtrue。下書き・バージョン解決不能の場合はfalse.</returns>
    public static bool TryMap(GitHubRelease? release, out UpdateInfo? updateInfo)
    {
        updateInfo = null;

        if (release == null)
        {
            return false;
        }

        // 下書きは公開されていないリリースなので常に除外する。
        if (release.Draft)
        {
            return false;
        }

        if (!TryParseVersion(release.TagName, out Version? version))
        {
            // バージョンを決定できないリリースは比較のしようがないため、安全側に倒して更新なしとして扱う。
            return false;
        }

        updateInfo = new UpdateInfo
        {
            Version = version!,
            TagName = release.TagName ?? string.Empty,
            ReleaseNotes = release.Body ?? string.Empty,
            ReleasePageUrl = UpdateHostValidator.TryCreateAllowedUri(release.HtmlUrl, out Uri? htmlUri)
                ? htmlUri!.ToString()
                : string.Empty,
            PublishedAt = release.PublishedAt,
            IsPrerelease = release.Prerelease,
            InstallerAsset = FindAsset(release.Assets, IsInstaller),
            PortableAsset = FindAsset(release.Assets, IsPortable),
            ChecksumsAsset = FindAsset(release.Assets, IsChecksums),
        };

        return true;
    }

    /// <summary>
    /// タグ名（例: v0.3.0, v0.3.0-beta1）から<see cref="Version"/>を取り出す.
    /// </summary>
    /// <param name="tagName">タグ名.</param>
    /// <param name="version">解決したバージョン.</param>
    /// <returns>解決できた場合はtrue.</returns>
    public static bool TryParseVersion(string? tagName, out Version? version)
    {
        version = null;

        if (string.IsNullOrWhiteSpace(tagName))
        {
            return false;
        }

        string candidate = tagName.Trim();

        // 先頭の "v" / "V" を除去する（release.ymlのタグ規約は vX.Y.Z）。
        if (candidate.Length > 0 && (candidate[0] == 'v' || candidate[0] == 'V'))
        {
            candidate = candidate[1..];
        }

        // "-beta1" 等のプレリリース識別子とビルドメタデータを切り落とす。
        // System.Versionはこれらを解釈できないため、数値部分だけを比較対象にする。
        int cut = candidate.IndexOfAny(['-', '+']);
        if (cut >= 0)
        {
            candidate = candidate[..cut];
        }

        return Version.TryParse(candidate, out version);
    }

    private static UpdateAsset? FindAsset(
        IReadOnlyList<GitHubReleaseAsset>? assets,
        Func<string, bool> nameMatches)
    {
        if (assets == null)
        {
            return null;
        }

        foreach (GitHubReleaseAsset asset in assets)
        {
            if (string.IsNullOrWhiteSpace(asset.Name) || !nameMatches(asset.Name))
            {
                continue;
            }

            // 信頼できないホストのURLは、そのアセットが無かったものとして扱う。
            if (!UpdateHostValidator.TryCreateAllowedUri(asset.BrowserDownloadUrl, out Uri? downloadUri))
            {
                continue;
            }

            return new UpdateAsset(asset.Name, downloadUri!, asset.Size);
        }

        return null;
    }

    private static bool IsInstaller(string name) =>
        name.StartsWith(InstallerPrefix, StringComparison.OrdinalIgnoreCase)
        && name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);

    private static bool IsPortable(string name) =>
        name.StartsWith(PortablePrefix, StringComparison.OrdinalIgnoreCase)
        && name.EndsWith(PortableSuffix, StringComparison.OrdinalIgnoreCase);

    private static bool IsChecksums(string name) =>
        string.Equals(name, ChecksumsFileName, StringComparison.OrdinalIgnoreCase);
}
