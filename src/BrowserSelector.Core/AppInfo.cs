// <copyright file="AppInfo.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using System.Reflection;

namespace BrowserSelector.Core;

/// <summary>
/// アプリケーション全体で共有するリポジトリURL・バージョン情報を集約する定数クラス（Phase E-2c）.
/// About セクション（E-2）・<see cref="Services.IUpdateService"/>（B-3/Phase H）・更新通知UI（H-5）が
/// すべてこれを参照する。リポジトリの移管・改名が起きても1箇所の修正で済むようにする.
/// </summary>
// CA1056: これらのURLはAbout画面の表示・HTTP呼び出し・JSON応答比較など文字列のまま使う箇所が大半のため、
// 呼び出し側での都度のToString()を避け意図的にstringで公開する（プロジェクト内の類似箇所と同じ方針）。
#pragma warning disable CA1056
public static class AppInfo
{
    /// <summary>
    /// GitHubリポジトリのオーナー名.
    /// </summary>
    public const string RepositoryOwner = "Yosuke-Sh";

    /// <summary>
    /// GitHubリポジトリ名.
    /// </summary>
    public const string RepositoryName = "BrowserSelector";

    /// <summary>
    /// Gets GitHubリポジトリのURL.
    /// </summary>
    public static string RepositoryUrl => $"https://github.com/{RepositoryOwner}/{RepositoryName}";

    /// <summary>
    /// Gets Issues一覧のURL.
    /// </summary>
    public static string IssuesUrl => $"{RepositoryUrl}/issues";

    /// <summary>
    /// Gets リリース一覧のURL.
    /// </summary>
    public static string ReleasesUrl => $"{RepositoryUrl}/releases";

    /// <summary>
    /// Gets 最新リリースを取得するGitHub REST APIのURL.
    /// </summary>
    public static string LatestReleaseApiUrl => $"https://api.github.com/repos/{RepositoryOwner}/{RepositoryName}/releases/latest";

    /// <summary>
    /// Gets 現在実行中のアセンブリのバージョン。
    /// <see cref="Assembly.GetEntryAssembly"/>が取得できない環境（一部テストホスト等）では、
    /// このアセンブリ（BrowserSelector.Core）自身のバージョンへフォールバックする.
    /// </summary>
    public static Version CurrentVersion { get; } = ResolveCurrentVersion();

    private static Version ResolveCurrentVersion()
    {
        Version? entryVersion = Assembly.GetEntryAssembly()?.GetName().Version;
        if (entryVersion != null && entryVersion != new Version(0, 0, 0, 0))
        {
            return entryVersion;
        }

        Version? coreVersion = Assembly.GetExecutingAssembly().GetName().Version;
        return coreVersion ?? new Version(0, 2, 0);
    }
}
#pragma warning restore CA1056
