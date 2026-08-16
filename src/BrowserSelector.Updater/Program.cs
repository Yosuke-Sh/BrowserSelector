// <copyright file="Program.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using System.Diagnostics;

namespace BrowserSelector.Updater;

/// <summary>
/// BrowserSelector.Updater のエントリポイント（Phase H-5）.
/// </summary>
/// <remarks>
/// 本体プロセスが自分自身を置換することはWindowsではできない（実行中ファイルがロックされる）ため、
/// 別プロセスとして起動され、呼び出し元の終了を待ってから適用する.
/// </remarks>
internal static class Program
{
    /// <summary>
    /// 呼び出し元プロセスの終了を待つ上限.
    /// </summary>
    internal const int ProcessWaitTimeoutSeconds = 30;

    /// <summary>
    /// プロセス終了後にファイルハンドルの解放を待つ猶予.
    /// </summary>
    internal const int HandleReleaseDelayMilliseconds = 500;

    /// <summary>
    /// エントリポイント.
    /// </summary>
    /// <param name="args">コマンドライン引数.</param>
    /// <returns>終了コード.</returns>
    internal static async Task<int> Main(string[] args)
    {
        if (!ArgumentParser.TryParse(args, out UpdaterOptions? options, out string? error))
        {
            // --log を解決できていないので既定のログパスへ出す（原因追跡のため必ず記録する）。
            UpdaterLog.Initialize(UpdaterLog.GetDefaultLogPath());
            UpdaterLog.Error($"引数エラー: {error}");
            return (int)UpdaterExitCode.InvalidArguments;
        }

        UpdaterLog.Initialize(options!.LogPath);
        UpdaterLog.Info($"アップデートの適用を開始します（mode={options.Mode}, pid={options.ProcessId}）");
        UpdaterLog.Info($"source={options.Source}");
        UpdaterLog.Info($"target={options.Target}");

        if (!await WaitForProcessExitAsync(options.ProcessId).ConfigureAwait(false))
        {
            // Killはしない。ユーザーが操作中のウィンドウを破壊するより、更新を諦める方が安全。
            UpdaterLog.Error($"対象プロセス（PID {options.ProcessId}）が{ProcessWaitTimeoutSeconds}秒以内に終了しませんでした");
            return (int)UpdaterExitCode.WaitTimeout;
        }

        UpdaterExitCode result = ApplyEngine.Apply(options);

        if (result == UpdaterExitCode.Success)
        {
            UpdaterLog.Info("アップデートの適用が完了しました");
            Relaunch(options);
        }
        else
        {
            UpdaterLog.Error($"アップデートの適用に失敗しました（終了コード {(int)result}）");
        }

        return (int)result;
    }

    private static async Task<bool> WaitForProcessExitAsync(int processId)
    {
        try
        {
            using Process process = Process.GetProcessById(processId);
            using CancellationTokenSource cts = new(TimeSpan.FromSeconds(ProcessWaitTimeoutSeconds));

            try
            {
                await process.WaitForExitAsync(cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }
        catch (ArgumentException)
        {
            // 既に終了済み。Process.GetProcessByIdはこの場合ArgumentExceptionを投げる。
            UpdaterLog.Info($"対象プロセス（PID {processId}）は既に終了しています");
        }
        catch (InvalidOperationException)
        {
            UpdaterLog.Info($"対象プロセス（PID {processId}）は既に終了しています");
        }

        // ファイルハンドルの解放はプロセス終了と厳密には同時ではないため猶予を置く。
        await Task.Delay(HandleReleaseDelayMilliseconds).ConfigureAwait(false);
        return true;
    }

    private static void Relaunch(UpdaterOptions options)
    {
        if (options.NoRelaunch)
        {
            UpdaterLog.Info("--no-relaunch が指定されているため再起動しません");
            return;
        }

        string executablePath = Path.Combine(options.Target, options.ExecutableName);

        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = executablePath,
                Arguments = options.RelaunchArguments,
                UseShellExecute = true,
                WorkingDirectory = options.Target,
            };

            using Process? process = Process.Start(startInfo);
            UpdaterLog.Info($"アプリケーションを再起動しました: {executablePath}");
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            // 適用自体は成功しているため、再起動の失敗で終了コードは変えない。
            UpdaterLog.Warn($"アプリケーションの再起動に失敗しました: {ex.Message}");
        }
    }
}
