// <copyright file="UpdaterExitCode.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
namespace BrowserSelector.Updater;

/// <summary>
/// Updaterの終了コード（Phase H-5）.
/// </summary>
/// <remarks>
/// 呼び出し元（UpdateService）はこの値で「単なる失敗」と「昇格して再実行すべき状況」を区別する.
/// </remarks>
internal enum UpdaterExitCode
{
    /// <summary>成功.</summary>
    Success = 0,

    /// <summary>引数が不正.</summary>
    InvalidArguments = 1,

    /// <summary>対象プロセスの終了待機がタイムアウトした.</summary>
    WaitTimeout = 2,

    /// <summary>バックアップに失敗した（この時点では何も変更していない）.</summary>
    BackupFailed = 3,

    /// <summary>適用に失敗し、ロールバックには成功した.</summary>
    ApplyFailedRolledBack = 4,

    /// <summary>適用に失敗し、ロールバックにも失敗した（致命的）.</summary>
    ApplyFailedRollbackFailed = 5,

    /// <summary>事前検証に失敗した（＝呼び出し側が昇格して再実行するための信号）.</summary>
    ValidationFailed = 6,
}
