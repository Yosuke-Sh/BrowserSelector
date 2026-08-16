// <copyright file="UpdaterLog.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using System.Globalization;

namespace BrowserSelector.Updater;

/// <summary>
/// Updater専用の最小ログ出力（Phase H-5）.
/// </summary>
/// <remarks>
/// Infrastructure層のLogServiceは参照できない（置換対象のDLLをロードするとファイルロックが起きる）ため、
/// 同等の書式（[yyyy-MM-dd HH:mm:ss.fff] [LEVEL] message）をここで再実装する.
/// </remarks>
internal static class UpdaterLog
{
    private static readonly object SyncRoot = new();

    private static string? logFilePath;

    /// <summary>
    /// ログの出力先ファイルを設定する.
    /// </summary>
    /// <param name="path">ログファイルのパス. nullならコンソールのみ.</param>
    public static void Initialize(string? path)
    {
        lock (SyncRoot)
        {
            logFilePath = path;

            try
            {
                string? directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    _ = Directory.CreateDirectory(directory);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
            {
                // ログのためにアップデートを失敗させない。以降はコンソール出力のみになる。
                logFilePath = null;
            }
        }
    }

    /// <summary>
    /// 情報ログを出力する.
    /// </summary>
    /// <param name="message">メッセージ.</param>
    public static void Info(string message) => Write("INFO", message);

    /// <summary>
    /// 警告ログを出力する.
    /// </summary>
    /// <param name="message">メッセージ.</param>
    public static void Warn(string message) => Write("WARN", message);

    /// <summary>
    /// エラーログを出力する.
    /// </summary>
    /// <param name="message">メッセージ.</param>
    public static void Error(string message) => Write("ERROR", message);

    /// <summary>
    /// 既定のログファイルパスを生成する.
    /// </summary>
    /// <returns>%LOCALAPPDATA%\BrowserSelector\logs\updater_{yyyyMMdd_HHmmss}.log.</returns>
    public static string GetDefaultLogPath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BrowserSelector",
        "logs",
        string.Create(CultureInfo.InvariantCulture, $"updater_{DateTime.Now:yyyyMMdd_HHmmss}.log"));

    private static void Write(string level, string message)
    {
        string line = string.Create(
            CultureInfo.InvariantCulture,
            $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}");

        Console.WriteLine(line);

        lock (SyncRoot)
        {
            if (logFilePath == null)
            {
                return;
            }

            try
            {
                File.AppendAllText(logFilePath, line + Environment.NewLine);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // ログ書き込みの失敗は握り潰す。ログのためにアップデートを失敗させない。
            }
        }
    }
}
