// <copyright file="ShellCloseService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using System.Windows;
using BrowserSelector.Core.Services;

namespace BrowserSelector.App.SystemIntegration;

/// <summary>
/// <see cref="IShellCloseService"/> のApp層実装。<see cref="TrayIconManager"/> は
/// トレイアイコンのセットアップ後（<c>App.SetupTrayIcon</c>）に判明するため、
/// コンストラクタではなく <see cref="AttachTrayIcon"/> で後付け注入する.
/// </summary>
internal sealed class ShellCloseService : IShellCloseService
{
    private TrayIconManager? _trayIconManager;

    /// <inheritdoc/>
    public bool CanMinimizeToTray => _trayIconManager is { IsMinimizedToTray: false };

    /// <summary>
    /// トレイ常駐が有効な場合に、格納対象となる <see cref="TrayIconManager"/> を紐付ける.
    /// トレイ常駐が無効な場合は呼び出されず、<see cref="_trayIconManager"/> は null のままとなる.
    /// </summary>
    /// <param name="trayIconManager">トレイ格納・復帰を管理するインスタンス.</param>
    public void AttachTrayIcon(TrayIconManager trayIconManager)
    {
        _trayIconManager = trayIconManager;
    }

    /// <inheritdoc/>
    public void RequestClose()
    {
        // UIスレッド以外（ブラウザ起動処理の継続部分等）から呼ばれる可能性があるため、
        // Dispatcher.Invokeによるマーシャリングをこのメソッド内で完結させる。
        // 呼び出し側にDispatcher操作を要求すると、スレッド親和性の対応漏れが再発しやすいため。
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            if (CanMinimizeToTray)
            {
                _trayIconManager!.MinimizeToTray();
            }
            else
            {
                System.Windows.Application.Current.Shutdown();
            }
        });
    }
}
