using System;
using System.Drawing;
using System.Windows.Forms;
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;

namespace BrowserSelector.Infrastructure.SystemIntegration;

/// <summary>
/// システムトレイ機能を提供するサービス
/// </summary>
public class SystemTrayService : ISystemTrayService, IDisposable
{
    private NotifyIcon? _notifyIcon;
    private ContextMenuStrip? _contextMenu;
    private bool _disposed = false;

    public event EventHandler<SystemTrayEventArgs>? SystemTrayAction;

    /// <summary>
    /// システムトレイアイコンを初期化
    /// </summary>
    public void InitializeSystemTray(string iconPath, string tooltipText)
    {
        try
        {
            // 既存のアイコンを破棄
            DisposeNotifyIcon();

            // アイコンを作成
            var icon = LoadIcon(iconPath);
            _notifyIcon = new NotifyIcon
            {
                Icon = icon,
                Text = tooltipText,
                Visible = true
            };

            // コンテキストメニューを作成
            CreateContextMenu();

            // イベントハンドラーを設定
            _notifyIcon.DoubleClick += OnNotifyIconDoubleClick;
            _notifyIcon.MouseClick += OnNotifyIconMouseClick;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"システムトレイの初期化に失敗しました: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// システムトレイアイコンを表示
    /// </summary>
    public void ShowSystemTray()
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = true;
        }
    }

    /// <summary>
    /// システムトレイアイコンを非表示
    /// </summary>
    public void HideSystemTray()
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
        }
    }

    /// <summary>
    /// バルーンティップを表示
    /// </summary>
    public void ShowBalloonTip(string title, string text, System.Windows.Forms.ToolTipIcon icon = System.Windows.Forms.ToolTipIcon.Info, int timeout = 3000)
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.ShowBalloonTip(timeout, title, text, icon);
        }
    }

    /// <summary>
    /// コンテキストメニューを更新
    /// </summary>
    public void UpdateContextMenu(SystemTrayMenuItems menuItems)
    {
        if (_contextMenu != null)
        {
            _contextMenu.Items.Clear();
            CreateMenuItems(_contextMenu.Items, menuItems);
        }
    }

    private void CreateContextMenu()
    {
        _contextMenu = new ContextMenuStrip();

        // デフォルトメニューアイテム
        var showItem = new ToolStripMenuItem("表示(&S)");
        showItem.Click += (s, e) => OnSystemTrayAction(SystemTrayActionType.Show);
        _contextMenu.Items.Add(showItem);

        var settingsItem = new ToolStripMenuItem("設定(&O)");
        settingsItem.Click += (s, e) => OnSystemTrayAction(SystemTrayActionType.Settings);
        _contextMenu.Items.Add(settingsItem);

        _contextMenu.Items.Add(new ToolStripSeparator());

        var exitItem = new ToolStripMenuItem("終了(&X)");
        exitItem.Click += (s, e) => OnSystemTrayAction(SystemTrayActionType.Exit);
        _contextMenu.Items.Add(exitItem);

        if (_notifyIcon != null)
        {
            _notifyIcon.ContextMenuStrip = _contextMenu;
        }
    }

    private void CreateMenuItems(ToolStripItemCollection items, SystemTrayMenuItems menuItems)
    {
        foreach (var item in menuItems.Items)
        {
            if (item.IsSeparator)
            {
                items.Add(new ToolStripSeparator());
            }
            else
            {
                var menuItem = new ToolStripMenuItem(item.Text)
                {
                    Enabled = item.IsEnabled,
                    Visible = item.IsVisible
                };

                if (item.Action != SystemTrayActionType.None)
                {
                    menuItem.Click += (s, e) => OnSystemTrayAction(item.Action);
                }

                if (item.SubItems?.Items.Count > 0)
                {
                    CreateMenuItems(menuItem.DropDownItems, item.SubItems);
                }

                items.Add(menuItem);
            }
        }
    }

    private Icon LoadIcon(string iconPath)
    {
        try
        {
            if (System.IO.File.Exists(iconPath))
            {
                return new Icon(iconPath);
            }
            else
            {
                // デフォルトアイコンを使用
                return SystemIcons.Application;
            }
        }
        catch
        {
            // エラー時はデフォルトアイコンを使用
            return SystemIcons.Application;
        }
    }

    private void OnNotifyIconDoubleClick(object? sender, EventArgs e)
    {
        OnSystemTrayAction(SystemTrayActionType.Show);
    }

    private void OnNotifyIconMouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            OnSystemTrayAction(SystemTrayActionType.Show);
        }
    }

    private void OnSystemTrayAction(SystemTrayActionType actionType)
    {
        SystemTrayAction?.Invoke(this, new SystemTrayEventArgs(actionType));
    }

    private void DisposeNotifyIcon()
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

        if (_contextMenu != null)
        {
            _contextMenu.Dispose();
            _contextMenu = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            DisposeNotifyIcon();
            _disposed = true;
        }
    }
}
