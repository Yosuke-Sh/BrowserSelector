// <copyright file="SettingsViewModel.Update.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Presentation.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BrowserSelector.Presentation.ViewModels;

/// <summary>
/// <see cref="SettingsViewModel"/> の「一般」タブ内「アップデート設定」GroupBox（Phase H-8）関連の
/// partialクラス。「今すぐ確認」「スキップ解除」コマンドと表示用プロパティを提供する.
/// </summary>
public partial class SettingsViewModel
{
    [ObservableProperty]
    private bool _isCheckingForUpdates;

    [ObservableProperty]
    private string _updateCheckStatusMessage = string.Empty;

    /// <summary>
    /// Gets 最終アップデート確認日時の表示用文字列。未チェックの場合はローカライズされた「未チェック」を返す.
    /// </summary>
    public string LastUpdateCheckDisplay =>
        AppSettings.LastUpdateCheckUtc is DateTimeOffset lastChecked
            ? lastChecked.ToLocalTime().ToString("G", System.Globalization.CultureInfo.CurrentCulture)
            : LocalizedLogHelper.GetString("Settings.App.LastUpdateCheckNever");

    /// <summary>
    /// Gets a value indicating whether 「このバージョンをスキップ」が設定されているか（スキップ解除ボタンの表示条件）.
    /// </summary>
    public bool HasSkippedUpdateVersion => !string.IsNullOrEmpty(AppSettings.SkippedUpdateVersion);

    /// <summary>
    /// 「今すぐ確認」コマンド。<see cref="IUpdateService.CheckForUpdatesAsync"/>を実行し、結果をステータスメッセージへ反映する.
    /// 二重実行防止のため<see cref="IsCheckingForUpdates"/>で<see cref="CheckForUpdatesNowCommand"/>のCanExecuteを制御する.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCheckForUpdatesNow))]
    private async Task CheckForUpdatesNowAsync()
    {
        if (_updateService == null)
        {
            UpdateCheckStatusMessage = LocalizedLogHelper.GetString("Settings.App.CheckFailed");
            LogService?.LogWarning("IUpdateServiceが未設定のためアップデート確認を実行できません", "SettingsViewModel");
            return;
        }

        IsCheckingForUpdates = true;
        UpdateCheckStatusMessage = LocalizedLogHelper.GetString("Settings.App.Checking");

        try
        {
            UpdateInfo? updateInfo = await _updateService.CheckForUpdatesAsync().ConfigureAwait(false);

            AppSettings.LastUpdateCheckUtc = DateTimeOffset.UtcNow;
            _ = await _settingsService.SaveAppSettingsAsync(AppSettings).ConfigureAwait(false);
            OnPropertyChanged(nameof(LastUpdateCheckDisplay));

            UpdateCheckStatusMessage = updateInfo != null
                ? LocalizedLogHelper.GetString("Settings.App.UpdateFound", updateInfo.TagName)
                : LocalizedLogHelper.GetString("Settings.App.UpToDate");
        }
        // CA1031: RelayCommandハンドラーの最上位try-catch。ネットワーク呼び出しは例外種別が多岐にわたり、
        // UIスレッドをクラッシュさせないための意図的な汎用catch。
#pragma warning disable CA1031
        catch (Exception ex)
        {
            UpdateCheckStatusMessage = LocalizedLogHelper.GetString("Settings.App.CheckFailed");
            LogService?.LogError($"アップデート確認エラー: {ex.Message}", "SettingsViewModel", ex);
        }
#pragma warning restore CA1031
        finally
        {
            IsCheckingForUpdates = false;
        }
    }

    private bool CanCheckForUpdatesNow() => !IsCheckingForUpdates;

    partial void OnIsCheckingForUpdatesChanged(bool value)
    {
        CheckForUpdatesNowCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// 「スキップを解除」コマンド。<see cref="AppSettings.SkippedUpdateVersion"/>を空にして保存する.
    /// </summary>
    [RelayCommand]
    private async Task ClearSkippedVersionAsync()
    {
        AppSettings.SkippedUpdateVersion = string.Empty;
        OnPropertyChanged(nameof(HasSkippedUpdateVersion));
        _ = await _settingsService.SaveAppSettingsAsync(AppSettings).ConfigureAwait(false);
    }
}
