// <copyright file="SettingsViewModel.DefaultBrowser.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Core.Services;
using BrowserSelector.Presentation.Helpers;
using CommunityToolkit.Mvvm.Input;

namespace BrowserSelector.Presentation.ViewModels;

/// <summary>
/// <see cref="SettingsViewModel"/> のOS既定ブラウザ（Windowsの「既定のアプリ」）関連のpartialクラス。
/// <see cref="IDefaultBrowserService"/>が未注入の場合（テスト環境等）は何もせず安全に無効化される。
/// 以前は既定ブラウザかどうかの判定結果をボタン周辺に表示していたが、判定処理自体が分かりにくく
/// 「ボタンを押しても何も表示されない」という不具合報告につながったため撤去し、
/// ボタン押下でWindowsの「既定のアプリ」設定画面を開くだけの単純な導線にした.
/// </summary>
public partial class SettingsViewModel
{
    /// <summary>
    /// Windowsの「既定のアプリ」設定画面を開く.
    /// </summary>
    [RelayCommand]
    private void OpenDefaultAppsSettings()
    {
        bool opened = _defaultBrowserService?.OpenDefaultAppsSettings() ?? false;
        if (!opened)
        {
            string message = _localizationService.GetString("Settings.App.OpenDefaultAppsFailed");
            _ = LocalizedMessageBox.ShowError(message);
        }
    }
}
