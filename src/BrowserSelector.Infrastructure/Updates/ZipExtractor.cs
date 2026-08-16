// <copyright file="ZipExtractor.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using System.IO;
using System.IO.Compression;

namespace BrowserSelector.Infrastructure.Updates;

/// <summary>
/// ポータブルZIPを安全に展開する（Phase H-4）.
/// </summary>
/// <remarks>
/// 展開はUpdater.exeではなくこちら（本体側）で行う。
/// 検証ロジックはテスト済みの本体に置き、Updaterには「展開済みディレクトリのコピー」という
/// 最も単純で失敗しにくい処理だけを担わせるため.
/// </remarks>
internal static class ZipExtractor
{
    /// <summary>
    /// 展開を許可するエントリ数の上限（ZIP爆弾対策）.
    /// </summary>
    public const int MaxEntryCount = 5000;

    /// <summary>
    /// 展開後の合計サイズの上限（ZIP爆弾対策）.
    /// </summary>
    public const long MaxTotalUncompressedBytes = 500L * 1024 * 1024;

    /// <summary>
    /// 展開後に存在していなければならない実行ファイル名.
    /// </summary>
    public const string RequiredExecutableName = "BrowserSelector.exe";

    /// <summary>
    /// ZIPを指定ディレクトリへ展開する.
    /// </summary>
    /// <param name="zipPath">ZIPファイルのパス.</param>
    /// <param name="destinationRoot">展開先ディレクトリ.</param>
    /// <param name="failureReason">失敗した場合の理由（成功時はnull）.</param>
    /// <returns>展開に成功した場合はtrue.</returns>
    public static bool TryExtract(string zipPath, string destinationRoot, out string? failureReason)
    {
        ArgumentNullException.ThrowIfNull(zipPath);
        ArgumentNullException.ThrowIfNull(destinationRoot);

        failureReason = null;

        try
        {
            // 展開先の比較基準は必ず正規化した絶対パス + 区切り文字とする。
            // 区切り文字を付けないと "C:\dest" と "C:\destination" が前方一致してしまう。
            string root = Path.GetFullPath(destinationRoot);
            string rootWithSeparator = root.EndsWith(Path.DirectorySeparatorChar)
                ? root
                : root + Path.DirectorySeparatorChar;

            _ = Directory.CreateDirectory(root);

            using ZipArchive archive = ZipFile.OpenRead(zipPath);

            if (archive.Entries.Count > MaxEntryCount)
            {
                failureReason = $"ZIPのエントリ数が上限({MaxEntryCount})を超えています: {archive.Entries.Count}";
                return false;
            }

            long totalUncompressed = 0;

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (!IsEntryNameSafe(entry.FullName))
                {
                    failureReason = $"ZIPに安全でないエントリ名が含まれています: {entry.FullName}";
                    return false;
                }

                totalUncompressed += entry.Length;
                if (totalUncompressed > MaxTotalUncompressedBytes)
                {
                    failureReason = $"ZIPの展開後サイズが上限({MaxTotalUncompressedBytes}バイト)を超えています";
                    return false;
                }

                string destination = Path.GetFullPath(Path.Combine(root, entry.FullName));

                // Zip Slip対策の本体。正規化後のパスが展開先の外を指していたら中断する。
                if (!destination.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
                {
                    failureReason = $"ZIPのエントリが展開先の外を指しています: {entry.FullName}";
                    return false;
                }

                // ディレクトリエントリ（名前が区切り文字で終わる、または長さ0）はディレクトリ作成のみ。
                if (entry.Name.Length == 0)
                {
                    _ = Directory.CreateDirectory(destination);
                    continue;
                }

                string? parent = Path.GetDirectoryName(destination);
                if (!string.IsNullOrEmpty(parent))
                {
                    _ = Directory.CreateDirectory(parent);
                }

                entry.ExtractToFile(destination, overwrite: true);
            }

            if (!File.Exists(Path.Combine(root, RequiredExecutableName)))
            {
                failureReason = $"展開結果に{RequiredExecutableName}が含まれていません";
                return false;
            }

            return true;
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException or NotSupportedException)
        {
            failureReason = $"ZIPの展開に失敗しました: {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// ZIPエントリ名が展開して安全な形かどうかを判定する.
    /// </summary>
    /// <param name="entryName">エントリのFullName.</param>
    /// <returns>安全な場合はtrue.</returns>
    public static bool IsEntryNameSafe(string? entryName)
    {
        if (string.IsNullOrWhiteSpace(entryName))
        {
            return false;
        }

        // 区切り文字をWindows形式へ揃えてから判定する。
        string normalized = entryName.Replace('/', '\\');

        // 絶対パス（C:\... や \\server\share、先頭の \）を拒否する。
        if (Path.IsPathRooted(normalized) || normalized.StartsWith('\\'))
        {
            return false;
        }

        // ':' はドライブレター指定と代替データストリーム（file.txt:hidden）の両方を含むため一律拒否する。
        if (normalized.Contains(':', StringComparison.Ordinal))
        {
            return false;
        }

        // 親ディレクトリ参照を含むセグメントを拒否する。
        foreach (string segment in normalized.Split('\\', StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment == "..")
            {
                return false;
            }
        }

        return true;
    }
}
