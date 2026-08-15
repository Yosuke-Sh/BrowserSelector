// <copyright file="TrayIconManager.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using System.Drawing;
using System.IO;
using System.Windows;
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using Forms = System.Windows.Forms;

namespace BrowserSelector.App.SystemIntegration;

/// <summary>
/// システムトレイ常駐を管理するクラス（Phase D）.
/// <see cref="AppSettings.AlwaysResidentInTray"/> が有効な場合、✕ボタンでの終了をトレイへの最小化として扱う。
/// トレイメニュー: 表示 / 既定ブラウザで開く / 設定 / 終了。ダブルクリックで復帰する.
/// BrowserChooser3 v0.1.5 の既知バグ（トレイ格納中に裏でブラウザが自動起動する）を避けるため、
/// <see cref="MinimizedToTray"/>/<see cref="RestoredFromTray"/>イベントの購読側で
/// 必ずカウントダウン（<c>CountdownController</c>）の停止・リセットを行う設計とする.
/// </summary>
internal sealed class TrayIconManager : IDisposable
{
    private readonly Forms.NotifyIcon _notifyIcon;
    private readonly Window _mainWindow;
    private readonly IBrowserService _browserService;
    private readonly ILocalizationService? _localizationService;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrayIconManager"/> class.
    /// </summary>
    /// <param name="mainWindow">トレイ格納・復帰の対象となるメインウィンドウ.</param>
    /// <param name="browserService">既定ブラウザで開くために使用するサービス.</param>
    /// <param name="localizationService">メニュー文言のローカライズに使用するサービス（省略可）.</param>
    public TrayIconManager(Window mainWindow, IBrowserService browserService, ILocalizationService? localizationService = null)
        : this(mainWindow, browserService, localizationService, static () =>
        {
            try
            {
                string iconPath = Path.Combine(AppContext.BaseDirectory, "BrowserSelector_Icon_256.ico");
                return File.Exists(iconPath) ? new Icon(iconPath) : SystemIcons.Application;
            }
            // CA1031: アイコン読み込み失敗時はシステム既定アイコンへフォールバックする意図的な汎用catch。
#pragma warning disable CA1031
            catch (Exception)
            {
                return SystemIcons.Application;
            }
#pragma warning restore CA1031
        })
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TrayIconManager"/> class.
    /// テスト用にトレイアイコンの取得元を差し替え可能にするコンストラクタ.
    /// </summary>
    /// <param name="mainWindow">トレイ格納・復帰の対象となるメインウィンドウ.</param>
    /// <param name="browserService">既定ブラウザで開くために使用するサービス.</param>
    /// <param name="localizationService">メニュー文言のローカライズに使用するサービス（省略可）.</param>
    /// <param name="iconProvider">トレイアイコンを返すデリゲート（テスト用）.</param>
    public TrayIconManager(Window mainWindow, IBrowserService browserService, ILocalizationService? localizationService, Func<Icon> iconProvider)
    {
        ArgumentNullException.ThrowIfNull(mainWindow);
        ArgumentNullException.ThrowIfNull(browserService);
        ArgumentNullException.ThrowIfNull(iconProvider);
        _mainWindow = mainWindow;
        _browserService = browserService;
        _localizationService = localizationService;

        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = iconProvider(),
            Visible = false,
            Text = "BrowserSelector",
        };
        _notifyIcon.DoubleClick += (_, _) => Restore();
        _notifyIcon.ContextMenuStrip = BuildContextMenu();
    }

    /// <summary>
    /// トレイ格納・復帰時に発火する（呼び出し側でカウントダウンの停止・リセットに使用する）.
    /// </summary>
    public event EventHandler? MinimizedToTray;

    /// <summary>
    /// トレイ格納・復帰時に発火する（呼び出し側でカウントダウンの停止・リセットに使用する）.
    /// </summary>
    public event EventHandler? RestoredFromTray;

    /// <summary>
    /// Gets a value indicating whether 現在トレイへ格納されているかどうか.
    /// </summary>
    public bool IsMinimizedToTray { get; private set; }

    /// <summary>
    /// メインウィンドウをトレイへ格納する。ウィンドウを非表示にし、タスクバーからも消す.
    /// </summary>
    public void MinimizeToTray()
    {
        if (IsMinimizedToTray)
        {
            return;
        }

        IsMinimizedToTray = true;
        _notifyIcon.Visible = true;
        _mainWindow.Hide();
        _mainWindow.ShowInTaskbar = false;
        MinimizedToTray?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// トレイからメインウィンドウを復帰させる.
    /// </summary>
    public void Restore()
    {
        if (!IsMinimizedToTray)
        {
            return;
        }

        IsMinimizedToTray = false;
        _mainWindow.ShowInTaskbar = true;
        _mainWindow.Show();
        if (_mainWindow.WindowState == WindowState.Minimized)
        {
            _mainWindow.WindowState = WindowState.Normal;
        }

        _ = _mainWindow.Activate();
        _notifyIcon.Visible = false;
        RestoredFromTray?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _disposed = true;
    }

    private Forms.ContextMenuStrip BuildContextMenu()
    {
        Forms.ContextMenuStrip menu = new();
        _ = menu.Items.Add(GetString("TrayMenu.Show", "表示"), null, (_, _) => Restore());
        _ = menu.Items.Add(GetString("TrayMenu.OpenWithDefaultBrowser", "既定ブラウザで開く"), null, async (_, _) => await OpenWithDefaultBrowserAsync().ConfigureAwait(false));
        _ = menu.Items.Add(GetString("TrayMenu.Settings", "設定"), null, (_, _) =>
        {
            Restore();
            if (_mainWindow.DataContext is Presentation.ViewModels.MainViewModel viewModel && viewModel.OpenSettingsCommand.CanExecute(null))
            {
                viewModel.OpenSettingsCommand.Execute(null);
            }
        });
        _ = menu.Items.Add(new Forms.ToolStripSeparator());
        _ = menu.Items.Add(GetString("TrayMenu.Exit", "終了"), null, (_, _) => System.Windows.Application.Current.Shutdown());
        return menu;
    }

    private async Task OpenWithDefaultBrowserAsync()
    {
        try
        {
            Browser? defaultBrowser = await _browserService.GetDefaultBrowserAsync().ConfigureAwait(false);
            if (defaultBrowser != null
                && _mainWindow.DataContext is Presentation.ViewModels.MainViewModel viewModel
                && !string.IsNullOrWhiteSpace(viewModel.Url)
                && Uri.TryCreate(viewModel.Url, UriKind.Absolute, out Uri? url))
            {
                _ = await _browserService.LaunchBrowserAsync(defaultBrowser, url).ConfigureAwait(false);
            }
        }
        // CA1031: トレイメニューのクリックハンドラー。UIスレッド外に例外を伝播させずログに残す意図的な汎用catch。
#pragma warning disable CA1031
        catch (Exception)
        {
            // トレイメニューからの操作のため、失敗しても静かに無視する（ログサービスはここでは未注入）。
        }
#pragma warning restore CA1031
    }

    private string GetString(string key, string fallback)
    {
        string? value = _localizationService?.GetString(key);
        return string.IsNullOrEmpty(value) || value == key ? fallback : value;
    }
}
