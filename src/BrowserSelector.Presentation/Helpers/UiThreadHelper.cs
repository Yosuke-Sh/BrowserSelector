// <copyright file="UiThreadHelper.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using System.Windows;

namespace BrowserSelector.Presentation.Helpers;

/// <summary>
/// <c>ConfigureAwait(false)</c>後の継続からUIスレッド上のObservablePropertyを安全に更新するためのヘルパー.
/// テスト環境では<see cref="Application.Current"/>がnullになるため、<c>Application.Current?.Dispatcher.Invoke</c>
/// だけでは更新が永久にスキップされてしまう（本番では実害が無いが、状態リセット処理などが検証不能になる）.
/// このヘルパーはApplication.Currentが無い場合・既にUIスレッド上の場合は直接実行し、
/// それ以外の場合のみDispatcherへ切り替える.
/// </summary>
public static class UiThreadHelper
{
    /// <summary>
    /// UIスレッド上でアクションを実行する.
    /// </summary>
    /// <param name="action">実行するアクション.</param>
    public static void Invoke(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        Application? app = Application.Current;
        if (app == null || app.Dispatcher.CheckAccess())
        {
            action();
            return;
        }

        app.Dispatcher.Invoke(action);
    }
}
