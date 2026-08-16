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

    [ObservableProperty]
    private bool _isApplyingUpdate;

    [ObservableProperty]
    private int _applyUpdateProgress;

    private UpdateInfo? _foundUpdate;

    /// <summary>
    /// アプリケーションの終了が必要になったときに発火する（更新適用後のシャットダウン）.
    /// <see cref="MainViewModel.ShutdownRequested"/>と同じ理由でViewModelから直接Shutdownを
    /// 呼ばず、呼び出し元（MainViewModel.OpenSettings）に委譲する.
    /// </summary>
    public event EventHandler? ShutdownRequested;

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
    /// Gets a value indicating whether 「今すぐ確認」で更新が見つかっており、この場で適用できる状態かどうか
    /// （「更新を適用」ボタンの表示条件）.
    /// </summary>
    /// <remarks>
    /// 従来は「今すぐ確認」が確認のみを行い、実際のダウンロード・適用はメインウィンドウ下部の
    /// 通知バーでしか行えなかった。設定画面を確認して閉じただけでは更新が一切適用されず、
    /// 「次回確認すると最新の状態です」と表示される（実際は304キャッシュのバグと、
    /// 適用未完了のまま終了しているだけ）という分かりにくさがあったため、設定画面のその場で
    /// 適用まで完結できるようにする.
    /// </remarks>
    public bool HasFoundUpdate => _foundUpdate != null;

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
            // このメソッドはUIバインド対象のRelayCommandで、await後の継続でObservableProperty
            // （UI状態）を更新するため、ConfigureAwait(false)は使わずUIスレッドの
            // SynchronizationContextへの自動復帰に委ねる。バックグラウンドスレッドから
            // Dispatcher.Invokeで戻す方式は、モーダルダイアログの入れ子メッセージループとの
            // 組み合わせでスレッド検証に失敗する事例が実機で確認されたため採用しない。
            UpdateInfo? updateInfo = await _updateService.CheckForUpdatesAsync().ConfigureAwait(true);

            AppSettings.LastUpdateCheckUtc = DateTimeOffset.UtcNow;
            _ = await _settingsService.SaveAppSettingsAsync(AppSettings).ConfigureAwait(true);
            OnPropertyChanged(nameof(LastUpdateCheckDisplay));

            _foundUpdate = updateInfo;
            OnPropertyChanged(nameof(HasFoundUpdate));

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
    /// 「更新を適用」コマンド。「今すぐ確認」で見つかった更新をダウンロード・検証・適用する.
    /// 成功時は<see cref="ShutdownRequested"/>を発火する（呼び出し元がウィンドウを閉じてから
    /// アプリを終了させる）.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanApplyUpdateNow))]
    private async Task ApplyUpdateNowAsync()
    {
        if (_updateService == null || _foundUpdate == null)
        {
            return;
        }

        IsApplyingUpdate = true;
        ApplyUpdateProgress = 0;

        try
        {
            Core.Models.UpdateChannel channel = _updateService.ResolveChannel();
            Progress<int> progress = new(p => ApplyUpdateProgress = p);

            UpdateDownloadResult downloadResult = await _updateService.DownloadUpdateAsync(_foundUpdate, channel, progress).ConfigureAwait(true);
            if (!downloadResult.Success)
            {
                UpdateCheckStatusMessage = downloadResult.Failure == UpdateDownloadFailure.ChecksumMismatch
                    ? LocalizedLogHelper.GetString("Update.Error.ChecksumMismatch")
                    : LocalizedLogHelper.GetString("Update.Error.DownloadFailed");
                LogService?.LogWarning($"アップデートのダウンロードに失敗しました: {downloadResult.Failure}", "SettingsViewModel");
                return;
            }

            bool applied = await _updateService.ApplyUpdateAsync(_foundUpdate).ConfigureAwait(true);
            if (applied)
            {
                LogService?.LogInformation("アップデート適用プロセスを起動しました。アプリケーションを終了します。", "SettingsViewModel");
                ShutdownRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                UpdateCheckStatusMessage = LocalizedLogHelper.GetString("Update.Error.DownloadFailed");
                LogService?.LogWarning("アップデートの適用が開始されませんでした（UACキャンセル等）", "SettingsViewModel");
            }
        }
        // CA1031: RelayCommandハンドラーの最上位try-catch。ネットワーク・ファイルI/O・プロセス起動など
        // 例外種別が多岐にわたり、UIスレッドをクラッシュさせないための意図的な汎用catch。
#pragma warning disable CA1031
        catch (Exception ex)
        {
            UpdateCheckStatusMessage = LocalizedLogHelper.GetString("Update.Error.DownloadFailed");
            LogService?.LogError($"アップデート適用中にエラーが発生しました: {ex.Message}", "SettingsViewModel", ex);
        }
#pragma warning restore CA1031
        finally
        {
            IsApplyingUpdate = false;
        }
    }

    private bool CanApplyUpdateNow() => HasFoundUpdate && !IsApplyingUpdate;

    partial void OnIsApplyingUpdateChanged(bool value)
    {
        ApplyUpdateNowCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// 「スキップを解除」コマンド。<see cref="AppSettings.SkippedUpdateVersion"/>を空にして保存する.
    /// </summary>
    [RelayCommand]
    private async Task ClearSkippedVersionAsync()
    {
        AppSettings.SkippedUpdateVersion = string.Empty;
        OnPropertyChanged(nameof(HasSkippedUpdateVersion));
        _ = await _settingsService.SaveAppSettingsAsync(AppSettings).ConfigureAwait(true);
    }
}
