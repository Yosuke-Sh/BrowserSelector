// <copyright file="CommandLineOptions.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.App.CommandLine;

/// <summary>
/// コマンドライン引数のパース結果（Phase D）.
/// </summary>
internal sealed class CommandLineOptions
{
    /// <summary>
    /// URLとして扱える最大文字数。BrowserChooser3に倣い、これを超える部分は打ち切る.
    /// </summary>
    public const int MaxUrlLength = 8191;

    /// <summary>
    /// Gets or sets 起動遅延秒数（<c>-d</c>/<c>--delay</c>）。未指定の場合は<see langword="null"/>で、
    /// <see cref="Core.Models.AppSettings.DefaultDelay"/>の値がそのまま使われることを表す.
    /// </summary>
    public int? Delay { get; set; }

    /// <summary>
    /// Gets or sets 起動時に選択するブラウザのGUID（<c>-b</c>/<c>--browser</c>）.
    /// </summary>
    public Guid? BrowserId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether UIを表示せず既定ブラウザへ直接遷移する（<c>--silent</c>）.
    /// </summary>
    public bool Silent { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether 遅延0で即時自動起動する（<c>--auto-launch</c>）。<see cref="Delay"/>を0で上書きする.
    /// </summary>
    public bool AutoLaunch { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether ヘルプを表示して終了する（<c>-h</c>/<c>--help</c>）.
    /// </summary>
    public bool ShowHelp { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether バージョン情報を表示して終了する（<c>-v</c>/<c>--version</c>）.
    /// </summary>
    public bool ShowVersion { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether テストモードかどうか（既存の<c>--test-mode</c>を維持）.
    /// </summary>
    public bool TestMode { get; set; }

    // CA1056: 8191文字打ち切り・不正な形式でもそのまま保持する必要があるため、Uri型ではなくstringのまま保持する
    // （Uri.TryCreateで弾かれる形式のURLもCLI経由ではそのままMainViewModel側の検証へ渡す設計のため）。
#pragma warning disable CA1056
    /// <summary>
    /// Gets or sets 起動時に渡されたURL（8191文字で打ち切り、URLエンコードされた<c>%</c>はデコード済み）.
    /// </summary>
    public string? Url { get; set; }
#pragma warning restore CA1056

    /// <summary>
    /// Gets 認識できなかった引数の一覧（不正なオプション名や値の解析失敗など）.
    /// </summary>
    public List<string> UnrecognizedArguments { get; } = [];

    /// <summary>
    /// Gets a value indicating whether パースエラーが1件以上あるかどうか.
    /// </summary>
    public bool HasErrors => UnrecognizedArguments.Count > 0;
}
