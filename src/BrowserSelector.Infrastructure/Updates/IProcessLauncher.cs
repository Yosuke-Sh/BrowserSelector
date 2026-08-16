// <copyright file="IProcessLauncher.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using System.Diagnostics;

namespace BrowserSelector.Infrastructure.Updates;

/// <summary>
/// プロセス起動を差し替え可能にするための抽象（Phase H-6）.
/// </summary>
/// <remarks>
/// テストで実際にインストーラやUpdater.exeを起動させないために導入する。
/// GitHub APIのDTOと同じく<c>internal</c>に留め、公開API面は増やさない.
/// </remarks>
internal interface IProcessLauncher
{
    /// <summary>
    /// プロセスを起動する.
    /// </summary>
    /// <param name="startInfo">起動情報.</param>
    /// <returns>起動できた場合はtrue.</returns>
    /// <exception cref="System.ComponentModel.Win32Exception">UACのキャンセルを含む起動失敗.</exception>
    bool Start(ProcessStartInfo startInfo);
}

/// <summary>
/// <see cref="Process.Start(ProcessStartInfo)"/> を用いた既定の実装.
/// </summary>
internal sealed class ProcessLauncher : IProcessLauncher
{
    /// <inheritdoc/>
    public bool Start(ProcessStartInfo startInfo)
    {
        using Process? process = Process.Start(startInfo);

        // UseShellExecute=trueでシェルが既存プロセスへ処理を委譲した場合はnullが返るが、
        // 起動要求自体は成功しているため成功として扱う。
        return true;
    }
}
