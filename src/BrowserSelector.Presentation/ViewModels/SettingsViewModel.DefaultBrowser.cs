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
    /// 現在の既定ブラウザ判定状態を再取得する。設定画面を開いた際やボタン操作後に呼び出す.
    /// </summary>
    public void RefreshDefaultBrowserStatus()
    {
        IsDefaultBrowser = _defaultBrowserService?.IsDefaultBrowser() ?? false;
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
