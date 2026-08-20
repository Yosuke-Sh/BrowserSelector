// <copyright file="SettingsViewModel.DefaultBrowser.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BrowserSelector.Presentation.ViewModels;

/// <summary>
/// <see cref="SettingsViewModel"/> のOS既定ブラウザ（Windowsの「既定のアプリ」）関連のpartialクラス。
/// <see cref="IDefaultBrowserService"/>が未注入の場合（テスト環境等）は判定・遷移のいずれも
/// 何もせず安全に無効化される.
/// </summary>
public partial class SettingsViewModel
{
    [ObservableProperty]
    private bool _isDefaultBrowser;

    /// <summary>
    /// Gets or sets 現在Windowsの既定ブラウザとして設定されているブラウザの表示名。
    /// BrowserSelector自身が既定の場合や判定不能な場合は<see langword="null"/>.
    /// </summary>
    [ObservableProperty]
    private string? _defaultBrowserName;

    /// <summary>
    /// Gets or sets a value indicating whether 既定ブラウザがBrowserSelector自身でも、
    /// 表示名を解決できた他ブラウザでもない（判定不能・未設定）状態かどうか.
    /// </summary>
    [ObservableProperty]
    private bool _isDefaultBrowserUnknown;

    /// <summary>
    /// 現在の既定ブラウザ判定状態を再取得する。設定画面を開いた際やボタン操作後に呼び出す.
    /// </summary>
    public void RefreshDefaultBrowserStatus()
    {
        IsDefaultBrowser = _defaultBrowserService?.IsDefaultBrowser() ?? false;
        DefaultBrowserName = IsDefaultBrowser ? null : _defaultBrowserService?.GetDefaultBrowserDisplayName();
        IsDefaultBrowserUnknown = !IsDefaultBrowser && string.IsNullOrEmpty(DefaultBrowserName);
    }

    /// <summary>
    /// Windowsの「既定のアプリ」設定画面をBrowserSelectorにフォーカスした状態で開く.
    /// </summary>
    [RelayCommand]
    private void OpenDefaultAppsSettings()
    {
        _defaultBrowserService?.OpenDefaultAppsSettings();
    }
}
