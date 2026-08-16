// <copyright file="CommandLineParser.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using System.Globalization;

namespace BrowserSelector.App.CommandLine;

/// <summary>
/// コマンドライン引数をパースするヘルパー（Phase D）.
/// サポートするオプション: <c>-d/--delay &lt;秒&gt;</c>, <c>-b/--browser &lt;GUID&gt;</c>,
/// <c>--silent</c>, <c>--auto-launch</c>, <c>-h/--help</c>, <c>-v/--version</c>, <c>--test-mode</c>（既存維持）.
/// オプションに該当しない引数はURLとして扱う（8191文字で打ち切り、<c>%</c>エンコードをデコード）.
/// </summary>
internal static class CommandLineParser
{
    /// <summary>
    /// Gets ヘルプメッセージ（<c>-h</c>/<c>--help</c>）。コンソール表示用のヘルプ文字列.
    /// </summary>
    public static string HelpText =>
        """
        BrowserSelector - 複数ブラウザ選択・起動ツール

        使用法: BrowserSelector.exe [オプション] [URL]

        オプション:
          -d, --delay <秒>       既定ブラウザへ自動起動するまでの秒数を指定します（0で無効）
          -b, --browser <GUID>   起動時に選択するブラウザをGUIDで指定します
          --silent                UIを表示せず既定ブラウザへ直接遷移します
          --auto-launch           遅延0で即時に既定ブラウザを自動起動します
          --test-mode             テストモードで起動します
          -h, --help              このヘルプを表示します
          -v, --version           バージョン情報を表示します
        """;

    /// <summary>
    /// コマンドライン引数をパースする.
    /// </summary>
    /// <param name="args">コマンドライン引数.</param>
    /// <returns>パース結果.</returns>
    public static CommandLineOptions Parse(IReadOnlyList<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);

        CommandLineOptions options = new();
        Queue<string> queue = new(args);

        while (queue.Count > 0)
        {
            string arg = queue.Dequeue();

            switch (arg)
            {
                case "-h":
                case "--help":
                    options.ShowHelp = true;
                    break;

                case "-v":
                case "--version":
                    options.ShowVersion = true;
                    break;

                case "--silent":
                    options.Silent = true;
                    break;

                case "--auto-launch":
                    options.AutoLaunch = true;
                    options.Delay = 0;
                    break;

                case "--test-mode":
                    options.TestMode = true;
                    break;

                case "-d":
                case "--delay":
                    if (queue.Count > 0 && int.TryParse(queue.Peek(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int delay) && delay >= 0)
                    {
                        options.Delay = delay;
                        queue.Dequeue();
                    }
                    else
                    {
                        options.UnrecognizedArguments.Add(arg);
                    }

                    break;

                case "-b":
                case "--browser":
                    if (queue.Count > 0 && Guid.TryParse(queue.Peek(), out Guid browserId))
                    {
                        options.BrowserId = browserId;
                        queue.Dequeue();
                    }
                    else
                    {
                        options.UnrecognizedArguments.Add(arg);
                    }

                    break;

                default:
                    if (arg.StartsWith('-'))
                    {
                        options.UnrecognizedArguments.Add(arg);
                    }
                    else
                    {
                        options.Url = NormalizeUrl(arg);
                    }

                    break;
            }
        }

        return options;
    }

    /// <summary>
    /// URLを8191文字で打ち切り、<c>%</c>エンコードをデコードする.
    /// </summary>
    /// <param name="url">元のURL文字列.</param>
    /// <returns>正規化後のURL文字列.</returns>
    private static string NormalizeUrl(string url)
    {
        string truncated = url.Length > CommandLineOptions.MaxUrlLength
            ? url[..CommandLineOptions.MaxUrlLength]
            : url;

        try
        {
            return Uri.UnescapeDataString(truncated);
        }
        catch (FormatException)
        {
            return truncated;
        }
    }
}
