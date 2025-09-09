using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BrowserSelector.Infrastructure.SystemIntegration;

/// <summary>
/// システムトレイ機能を提供するサービス.
/// </summary>
public class SystemTrayService : ISystemTrayService, IDisposable
{
    private NotifyIcon? _notifyIcon;
    private ContextMenuStrip? _contextMenu;
    private bool _disposed = false;

    /// <inheritdoc/>
    public event EventHandler<SystemTrayEventArgs>? SystemTrayAction;

    /// <summary>
    /// システムトレイアイコンを初期化.
    /// </summary>
    public void InitializeSystemTray(string iconPath, string tooltipText)
    {
        try
        {
            // 既存のアイコンを破棄
            DisposeNotifyIcon();

            // アイコンを作成
            Icon icon = LoadIcon(iconPath);
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
    /// システムトレイアイコンを表示.
    /// </summary>
    public void ShowSystemTray()
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = true;
        }
    }

    /// <summary>
    /// システムトレイアイコンを非表示.
    /// </summary>
    public void HideSystemTray()
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
        }
    }

    /// <summary>
    /// バルーンティップを表示.
    /// </summary>
    public void ShowBalloonTip(string title, string text, System.Windows.Forms.ToolTipIcon icon = System.Windows.Forms.ToolTipIcon.Info, int timeout = 3000)
    {
        _notifyIcon?.ShowBalloonTip(timeout, title, text, icon);
    }

    /// <summary>
    /// コンテキストメニューを更新.
    /// </summary>
    public void UpdateContextMenu(SystemTrayMenuItems menuItems)
    {
        ArgumentNullException.ThrowIfNull(menuItems);
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
        ToolStripMenuItem showItem = new("表示(&S)");
        showItem.Click += (s, e) => OnSystemTrayAction(SystemTrayActionType.Show);
        _ = _contextMenu.Items.Add(showItem);

        ToolStripMenuItem settingsItem = new("設定(&O)");
        settingsItem.Click += (s, e) => OnSystemTrayAction(SystemTrayActionType.Settings);
        _ = _contextMenu.Items.Add(settingsItem);

        _ = _contextMenu.Items.Add(new ToolStripSeparator());

        ToolStripMenuItem exitItem = new("終了(&X)");
        exitItem.Click += (s, e) => OnSystemTrayAction(SystemTrayActionType.Exit);
        _ = _contextMenu.Items.Add(exitItem);

        if (_notifyIcon != null)
        {
            _notifyIcon.ContextMenuStrip = _contextMenu;
        }
    }

    private void CreateMenuItems(ToolStripItemCollection items, SystemTrayMenuItems menuItems)
    {
        foreach (SystemTrayMenuItem item in menuItems.Items)
        {
            if (item.IsSeparator)
            {
                _ = items.Add(new ToolStripSeparator());
            }
            else
            {
                ToolStripMenuItem menuItem = new(item.Text)
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

                _ = items.Add(menuItem);
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
        catch (ArgumentException ex)
        {
            // エラー時はデフォルトアイコンを使用
            System.Diagnostics.Debug.WriteLine($"Icon loading failed (ArgumentException): {ex.Message}");
            return SystemIcons.Application;
        }
        catch (FileNotFoundException ex)
        {
            // エラー時はデフォルトアイコンを使用
            System.Diagnostics.Debug.WriteLine($"Icon loading failed (FileNotFoundException): {ex.Message}");
            return SystemIcons.Application;
        }
        catch (UnauthorizedAccessException ex)
        {
            // エラー時はデフォルトアイコンを使用
            System.Diagnostics.Debug.WriteLine($"Icon loading failed (UnauthorizedAccessException): {ex.Message}");
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

    /// <inheritdoc/>
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
