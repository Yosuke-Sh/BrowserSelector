// <copyright file="UpdatePaths.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using System.IO;

namespace BrowserSelector.Infrastructure.Updates;

/// <summary>
/// 更新処理が使う作業ディレクトリのパスを解決する（Phase H-3）.
/// </summary>
/// <remarks>
/// 作業ファイルはすべて %LOCALAPPDATA%\BrowserSelector 配下に置く。
/// インストール先（Program Files配下）へは絶対に書き込まない — 昇格が必要になるうえ、
/// 適用前の一時ファイルが本体と混ざると復旧が難しくなるため.
/// </remarks>
internal static class UpdatePaths
{
    /// <summary>
    /// ETag等のチェック状態を保存するファイル名.
    /// </summary>
    public const string CheckStateFileName = "etag.json";

    /// <summary>
    /// 更新用の作業ディレクトリのルートを取得する.
    /// </summary>
    /// <returns>%LOCALAPPDATA%\BrowserSelector\updates.</returns>
    public static string GetUpdatesRoot() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BrowserSelector",
        "updates");

    /// <summary>
    /// バックアップの保存先ルートを取得する.
    /// </summary>
    /// <returns>%LOCALAPPDATA%\BrowserSelector\backup.</returns>
    public static string GetBackupRoot() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BrowserSelector",
        "backup");

    /// <summary>
    /// チェック状態ファイルのパスを取得する.
    /// </summary>
    /// <returns>etag.jsonのフルパス.</returns>
    public static string GetCheckStatePath() => Path.Combine(GetUpdatesRoot(), CheckStateFileName);

    /// <summary>
    /// 指定バージョンのダウンロード先ディレクトリのパスを取得する.
    /// </summary>
    /// <param name="version">バージョン.</param>
    /// <returns>ダウンロード先ディレクトリ.</returns>
    public static string GetVersionDirectory(Version version)
    {
        ArgumentNullException.ThrowIfNull(version);
        return Path.Combine(GetUpdatesRoot(), version.ToString());
    }
}
