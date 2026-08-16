// <copyright file="ArgumentParser.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
namespace BrowserSelector.Updater;

/// <summary>
/// コマンドライン引数を解析する（Phase H-5）.
/// </summary>
/// <remarks>
/// 名前付き引数のみを受け付ける。位置引数はパスに空白が含まれる場合の扱いでハマるため使わない.
/// </remarks>
internal static class ArgumentParser
{
    /// <summary>
    /// 適用可能な唯一のモード.
    /// </summary>
    public const string ApplyZipMode = "apply-zip";

    /// <summary>
    /// 値を伴う引数の一覧.
    /// </summary>
    private static readonly HashSet<string> ValueArguments = new(StringComparer.Ordinal)
    {
        "--mode", "--pid", "--source", "--target", "--backup", "--exe", "--log", "--relaunch-args",
    };

    /// <summary>
    /// 省略できない引数の一覧.
    /// </summary>
    private static readonly string[] RequiredArguments = new[] { "--mode", "--pid", "--source", "--target", "--backup" };

    /// <summary>
    /// 引数を解析する.
    /// </summary>
    /// <param name="args">コマンドライン引数.</param>
    /// <param name="options">解析結果.</param>
    /// <param name="error">解析に失敗した理由.</param>
    /// <returns>解析に成功した場合はtrue.</returns>
    public static bool TryParse(string[] args, out UpdaterOptions? options, out string? error)
    {
        ArgumentNullException.ThrowIfNull(args);

        options = null;
        error = null;

        Dictionary<string, string> values = new(StringComparer.Ordinal);
        HashSet<string> flags = new(StringComparer.Ordinal);

        int index = 0;
        while (index < args.Length)
        {
            string arg = args[index];

            if (!arg.StartsWith("--", StringComparison.Ordinal))
            {
                error = $"位置引数はサポートしていません: {arg}";
                return false;
            }

            if (string.Equals(arg, "--no-relaunch", StringComparison.Ordinal))
            {
                _ = flags.Add(arg);
                index++;
                continue;
            }

            if (!ValueArguments.Contains(arg))
            {
                error = $"未知の引数です: {arg}";
                return false;
            }

            if (index + 1 >= args.Length)
            {
                error = $"{arg} に値が指定されていません";
                return false;
            }

            if (!values.TryAdd(arg, args[index + 1]))
            {
                error = $"{arg} が重複して指定されています";
                return false;
            }

            index += 2;
        }

        foreach (string required in RequiredArguments)
        {
            if (!values.ContainsKey(required))
            {
                error = $"必須の引数が指定されていません: {required}";
                return false;
            }
        }

        if (!string.Equals(values["--mode"], ApplyZipMode, StringComparison.Ordinal))
        {
            error = $"未対応のモードです: {values["--mode"]}";
            return false;
        }

        if (!int.TryParse(values["--pid"], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int pid)
            || pid <= 0)
        {
            error = $"--pid が不正です: {values["--pid"]}";
            return false;
        }

        options = new UpdaterOptions
        {
            Mode = values["--mode"],
            ProcessId = pid,
            Source = values["--source"],
            Target = values["--target"],
            Backup = values["--backup"],
            ExecutableName = values.TryGetValue("--exe", out string? exe) && !string.IsNullOrWhiteSpace(exe)
                ? exe
                : UpdaterOptions.DefaultExecutableName,
            LogPath = values.TryGetValue("--log", out string? log) && !string.IsNullOrWhiteSpace(log)
                ? log
                : UpdaterLog.GetDefaultLogPath(),
            RelaunchArguments = values.TryGetValue("--relaunch-args", out string? relaunchArgs) ? relaunchArgs : string.Empty,
            NoRelaunch = flags.Contains("--no-relaunch"),
        };

        return true;
    }
}
