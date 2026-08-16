// <copyright file="RecordingProcessLauncher.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using System.ComponentModel;
using System.Diagnostics;
using BrowserSelector.Infrastructure.Updates;

namespace BrowserSelector.InfrastructureTests.TestDoubles;

/// <summary>
/// 実際にプロセスを起動せず、起動要求だけを記録するテスト用の<see cref="IProcessLauncher"/>（Phase H-6）.
/// </summary>
internal sealed class RecordingProcessLauncher : IProcessLauncher
{
    /// <summary>
    /// Gets 記録された起動要求.
    /// </summary>
    public List<ProcessStartInfo> Started { get; } = new();

    /// <summary>
    /// Gets or sets 起動時に投げる例外。nullなら成功する.
    /// </summary>
    public Exception? ThrowOnStart { get; set; }

    /// <summary>
    /// UACキャンセルを再現する例外を生成する.
    /// </summary>
    /// <returns>ERROR_CANCELLED（1223）のWin32Exception.</returns>
    public static Win32Exception CreateUacCancellation() => new(1223);

    /// <inheritdoc/>
    public bool Start(ProcessStartInfo startInfo)
    {
        if (ThrowOnStart != null)
        {
            throw ThrowOnStart;
        }

        Started.Add(startInfo);
        return true;
    }
}
