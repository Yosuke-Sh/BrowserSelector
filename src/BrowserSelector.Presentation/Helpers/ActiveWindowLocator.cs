// <copyright file="ActiveWindowLocator.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using System.Windows;

namespace BrowserSelector.Presentation.Helpers;

/// <summary>
/// ダイアログのOwnerに設定するアクティブウィンドウを取得するヘルパー。
/// アクティブウィンドウが取得できない場合はメインウィンドウを返す（背面表示防止のため）。
/// <see cref="System.Windows.Application.Current"/>にはスレッド親和性があるため、
/// 必ずUIスレッドから呼び出すこと.
/// </summary>
public static class ActiveWindowLocator
{
    /// <summary>
    /// アクティブなウィンドウ（無ければメインウィンドウ）を取得する.
    /// テスト環境等で<see cref="System.Windows.Application.Current"/>が存在しない場合はnullを返す.
    /// </summary>
    /// <returns>Ownerに設定すべきウィンドウ、無ければnull.</returns>
    public static Window? GetActiveWindow()
    {
        Application? application = Application.Current;
        if (application == null)
        {
            return null;
        }

        return application.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            ?? application.MainWindow;
    }
}
