// <copyright file="ApplyEngine.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using System.Globalization;

namespace BrowserSelector.Updater;

/// <summary>
/// 展開済みディレクトリをインストール先へ適用する（Phase H-5）.
/// </summary>
/// <remarks>
/// ZIPの展開・SHA256検証はUpdateService側で完了済み。ここは「展開済みディレクトリのコピー」という
/// 最も単純で失敗しにくい処理だけを担う.
/// </remarks>
internal static class ApplyEngine
{
    /// <summary>
    /// Updater自身の実行ファイル名. 実行中のためロックされており置換できない.
    /// </summary>
    public const string UpdaterExecutableName = "BrowserSelector.Updater.exe";

    /// <summary>
    /// 実行中で置換できないファイルを次回起動時に正規化するための一時拡張子.
    /// </summary>
    public const string PendingExtension = ".new";

    /// <summary>
    /// 置換前のファイルを退避する一時拡張子.
    /// </summary>
    public const string OldExtension = ".old";

    /// <summary>
    /// 残すバックアップの世代数.
    /// </summary>
    public const int BackupGenerations = 2;

    /// <summary>
    /// バックアップから除外するディレクトリ名（ユーザーデータであり置換対象ではない）.
    /// </summary>
    private static readonly string[] ExcludedDirectories = new[] { "logs" };

    /// <summary>
    /// バックアップから除外するファイル名（ユーザー設定を上書きしない）.
    /// </summary>
    private static readonly string[] ExcludedFiles = new[] { "settings.json" };

    /// <summary>
    /// 適用を実行する.
    /// </summary>
    /// <param name="options">実行オプション.</param>
    /// <returns>終了コード.</returns>
    public static UpdaterExitCode Apply(UpdaterOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!Validate(options, out string? validationError))
        {
            UpdaterLog.Error($"事前検証に失敗しました: {validationError}");
            return UpdaterExitCode.ValidationFailed;
        }

        if (!TryCreateBackup(options.Target, options.Backup))
        {
            // まだ何も変更していないので安全に中断できる。
            return UpdaterExitCode.BackupFailed;
        }

        List<string> renamed = new();
        List<string> copied = new();

        try
        {
            CopyNewFiles(options, renamed, copied);
            CommitRenamed(renamed);
            UpdaterLog.Info($"{copied.Count}個のファイルを適用しました");
            CleanupOldBackups(options.Backup);
            return UpdaterExitCode.Success;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            UpdaterLog.Error($"適用に失敗しました: {ex.Message}");
            return Rollback(options, renamed, copied);
        }
    }

    /// <summary>
    /// 適用の事前検証を行う.
    /// </summary>
    /// <param name="options">実行オプション.</param>
    /// <param name="error">検証に失敗した理由.</param>
    /// <returns>適用可能な場合はtrue.</returns>
    public static bool Validate(UpdaterOptions options, out string? error)
    {
        ArgumentNullException.ThrowIfNull(options);

        error = null;

        if (!Directory.Exists(options.Source))
        {
            error = $"ソースディレクトリが存在しません: {options.Source}";
            return false;
        }

        if (!File.Exists(Path.Combine(options.Source, options.ExecutableName)))
        {
            error = $"ソースに{options.ExecutableName}が含まれていません";
            return false;
        }

        if (!Directory.Exists(options.Target))
        {
            error = $"インストールディレクトリが存在しません: {options.Target}";
            return false;
        }

        if (!IsDirectoryWritable(options.Target))
        {
            // 昇格が必要な場所（Program Files配下等）。呼び出し側がインストーラ経路へ切り替えられるよう
            // ValidationFailedを返す。
            error = $"インストールディレクトリに書き込めません: {options.Target}";
            return false;
        }

        return true;
    }

    /// <summary>
    /// バックアップの世代を整理する.
    /// </summary>
    /// <param name="backupDirectory">今回のバックアップディレクトリ.</param>
    public static void CleanupOldBackups(string backupDirectory)
    {
        ArgumentNullException.ThrowIfNull(backupDirectory);

        try
        {
            string? parent = Path.GetDirectoryName(Path.GetFullPath(backupDirectory));
            if (string.IsNullOrEmpty(parent) || !Directory.Exists(parent))
            {
                return;
            }

            IEnumerable<DirectoryInfo> stale = new DirectoryInfo(parent)
                .GetDirectories()
                .OrderByDescending(d => d.LastWriteTimeUtc)
                .Skip(BackupGenerations);

            foreach (DirectoryInfo directory in stale)
            {
                try
                {
                    directory.Delete(recursive: true);
                    UpdaterLog.Info($"古いバックアップを削除しました: {directory.Name}");
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    UpdaterLog.Warn($"古いバックアップを削除できませんでした（{directory.Name}）: {ex.Message}");
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            // 世代管理の失敗は更新の成否に影響しない。
            UpdaterLog.Warn($"バックアップの世代管理に失敗しました: {ex.Message}");
        }
    }

    /// <summary>
    /// 指定ファイルがバックアップ・適用の対象外かどうかを判定する.
    /// </summary>
    /// <param name="relativePath">ターゲットディレクトリからの相対パス.</param>
    /// <returns>対象外の場合はtrue.</returns>
    public static bool IsExcluded(string relativePath)
    {
        ArgumentNullException.ThrowIfNull(relativePath);

        string[] segments = relativePath.Split(
            new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
            StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
        {
            return false;
        }

        if (ExcludedDirectories.Contains(segments[0], StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        return segments.Length == 1
            && ExcludedFiles.Contains(segments[0], StringComparer.OrdinalIgnoreCase);
    }

    private static bool TryCreateBackup(string target, string backup)
    {
        try
        {
            _ = Directory.CreateDirectory(backup);

            int count = 0;
            foreach (string file in Directory.EnumerateFiles(target, "*", SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(target, file);
                if (IsExcluded(relative))
                {
                    continue;
                }

                string destination = Path.Combine(backup, relative);
                string? directory = Path.GetDirectoryName(destination);
                if (!string.IsNullOrEmpty(directory))
                {
                    _ = Directory.CreateDirectory(directory);
                }

                File.Copy(file, destination, overwrite: true);
                count++;
            }

            UpdaterLog.Info($"{count}個のファイルをバックアップしました: {backup}");
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            UpdaterLog.Error($"バックアップに失敗しました: {ex.Message}");
            return false;
        }
    }

    private static void CopyNewFiles(UpdaterOptions options, List<string> renamed, List<string> copied)
    {
        foreach (string sourceFile in Directory.EnumerateFiles(options.Source, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(options.Source, sourceFile);
            if (IsExcluded(relative))
            {
                continue;
            }

            // Updater自身は実行中でロックされているため置換できない。決定事項どおり「1世代遅れ」を
            // 許容し、.newとして置いておく（次回のインストーラ更新で正規化される）。
            bool isSelf = string.Equals(
                Path.GetFileName(relative),
                UpdaterExecutableName,
                StringComparison.OrdinalIgnoreCase);

            string destination = Path.Combine(options.Target, isSelf ? relative + PendingExtension : relative);

            string? directory = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(directory))
            {
                _ = Directory.CreateDirectory(directory);
            }

            // 削除ではなくリネームで退避する。ロックされているファイルでもリネームは通ることが多く、
            // 失敗時にも元へ戻せる。
            if (!isSelf && File.Exists(destination))
            {
                string old = destination + OldExtension;
                if (File.Exists(old))
                {
                    File.Delete(old);
                }

                File.Move(destination, old);
                renamed.Add(destination);
            }

            File.Copy(sourceFile, destination, overwrite: true);
            copied.Add(destination);
        }
    }

    private static void CommitRenamed(List<string> renamed)
    {
        foreach (string destination in renamed)
        {
            try
            {
                string old = destination + OldExtension;
                if (File.Exists(old))
                {
                    File.Delete(old);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // .oldが残っても動作には影響しない。次回の適用時に上書き削除される。
                UpdaterLog.Warn($"退避ファイルを削除できませんでした（{destination}{OldExtension}）: {ex.Message}");
            }
        }
    }

    private static UpdaterExitCode Rollback(UpdaterOptions options, List<string> renamed, List<string> copied)
    {
        UpdaterLog.Warn("ロールバックを開始します");

        bool succeeded = true;

        // まず今回コピーしたファイルを取り除く。退避元が無いもの（新規追加ファイル）はこれで消える。
        foreach (string destination in copied)
        {
            try
            {
                if (File.Exists(destination))
                {
                    File.Delete(destination);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                UpdaterLog.Error($"ロールバック中にファイルを削除できませんでした（{destination}）: {ex.Message}");
                succeeded = false;
            }
        }

        // 退避しておいた元ファイルを戻す。
        foreach (string destination in renamed)
        {
            try
            {
                string old = destination + OldExtension;
                if (File.Exists(old))
                {
                    File.Move(old, destination, overwrite: true);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                UpdaterLog.Error($"ロールバック中に退避ファイルを戻せませんでした（{destination}）: {ex.Message}");
                succeeded = false;
            }
        }

        // リネームで戻せなかったものはバックアップから復元する。
        if (!succeeded)
        {
            succeeded = TryRestoreFromBackup(options.Backup, options.Target);
        }

        if (succeeded)
        {
            UpdaterLog.Info("ロールバックが完了しました");
            return UpdaterExitCode.ApplyFailedRolledBack;
        }

        UpdaterLog.Error("ロールバックに失敗しました。手動での復旧が必要です");
        return UpdaterExitCode.ApplyFailedRollbackFailed;
    }

    private static bool TryRestoreFromBackup(string backup, string target)
    {
        try
        {
            foreach (string file in Directory.EnumerateFiles(backup, "*", SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(backup, file);
                string destination = Path.Combine(target, relative);

                string? directory = Path.GetDirectoryName(destination);
                if (!string.IsNullOrEmpty(directory))
                {
                    _ = Directory.CreateDirectory(directory);
                }

                File.Copy(file, destination, overwrite: true);
            }

            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            UpdaterLog.Error($"バックアップからの復元に失敗しました: {ex.Message}");
            return false;
        }
    }

    private static bool IsDirectoryWritable(string directory)
    {
        try
        {
            string probe = Path.Combine(
                directory,
                string.Create(CultureInfo.InvariantCulture, $".bs_updater_probe_{Guid.NewGuid():N}"));

            using (File.Create(probe, 1, FileOptions.DeleteOnClose))
            {
                return true;
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return false;
        }
    }
}
