// <copyright file="UpdaterOptions.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
namespace BrowserSelector.Updater;

/// <summary>
/// コマンドライン引数から解決した実行オプション（Phase H-5）.
/// </summary>
internal sealed class UpdaterOptions
{
    /// <summary>
    /// 既定で再起動する実行ファイル名.
    /// </summary>
    /// <remarks>
    /// App側の定数を参照したいところだが、Coreを参照するとDLLがロックされて置換できなくなるため
    /// 意図的に重複させている（csprojのコメントも参照）.
    /// </remarks>
    public const string DefaultExecutableName = "BrowserSelector.exe";

    /// <summary>
    /// Gets the 実行モード.
    /// </summary>
    public string Mode { get; init; } = string.Empty;

    /// <summary>
    /// Gets the 終了を待機する対象プロセスID.
    /// </summary>
    public int ProcessId { get; init; }

    /// <summary>
    /// Gets the 展開済みの新バージョンが置かれたディレクトリ.
    /// </summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>
    /// Gets the 置換対象のインストールディレクトリ.
    /// </summary>
    public string Target { get; init; } = string.Empty;

    /// <summary>
    /// Gets the バックアップの保存先ディレクトリ.
    /// </summary>
    public string Backup { get; init; } = string.Empty;

    /// <summary>
    /// Gets the 再起動する実行ファイル名.
    /// </summary>
    public string ExecutableName { get; init; } = DefaultExecutableName;

    /// <summary>
    /// Gets the ログファイルのパス.
    /// </summary>
    public string LogPath { get; init; } = string.Empty;

    /// <summary>
    /// Gets the 再起動時に渡す引数.
    /// </summary>
    public string RelaunchArguments { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether 適用後の再起動を行わないかどうか.
    /// </summary>
    public bool NoRelaunch { get; init; }
}
