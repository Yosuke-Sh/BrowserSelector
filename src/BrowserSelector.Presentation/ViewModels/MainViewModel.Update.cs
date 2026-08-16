// <copyright file="MainViewModel.Update.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Core.Models;
using BrowserSelector.Presentation.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BrowserSelector.Presentation.ViewModels;

/// <summary>
/// <see cref="MainViewModel"/> のメインウィンドウ最下部に表示する更新通知バー（Phase H-9）関連のpartialクラス.
/// 起動時バックグラウンドチェック（App.xaml.cs側、Phase H-10）が
/// <see cref="ShowUpdateNotification"/>を呼び出すことで表示状態になる。
/// 「起動→ブラウザ選択」の最短動線を阻害しないため、モーダルは一切使わない.
/// </summary>
public partial class MainViewModel
{
    [ObservableProperty]
    private bool _isUpdateNotificationVisible;

    [ObservableProperty]
    private string _updateNotificationMessage = string.Empty;

    [ObservableProperty]
    private bool _isUpdateDownloading;

    [ObservableProperty]
    private int _updateDownloadProgress;

    private UpdateInfo? _pendingUpdate;

    /// <summary>
    /// アプリケーションの終了が必要になったときに発火する（更新適用後のシャットダウン、Phase H-9）.
    /// <see cref="System.Windows.Application.Current"/>.Shutdown()をViewModelから直接呼ばず、
    /// 呼び出し元（App.xaml.cs）に委譲することでテスト容易性を確保する.
    /// </summary>
    public event EventHandler? ShutdownRequested;

    /// <summary>
    /// 起動時バックグラウンドチェック（Phase H-10）から呼び出され、通知バーを表示する.
    /// </summary>
    /// <param name="updateInfo">検出されたアップデート情報.</param>
    public void ShowUpdateNotification(UpdateInfo updateInfo)
    {
        ArgumentNullException.ThrowIfNull(updateInfo);
        _pendingUpdate = updateInfo;
        UpdateNotificationMessage = LocalizedLogHelper.GetString("Update.Notification.Message", updateInfo.TagName);
        IsUpdateNotificationVisible = true;
    }

    /// <summary>
    /// 「今すぐ更新」コマンド。ダウンロード→検証→適用の起動までを行い、成功時は<see cref="ShutdownRequested"/>を発火する.
    /// </summary>
    [RelayCommand]
    private async Task StartUpdateAsync()
    {
        if (_updateService == null || _pendingUpdate == null)
        {
            return;
        }

        try
        {
            IsUpdateDownloading = true;
            UpdateDownloadProgress = 0;

            Core.Models.UpdateChannel channel = _updateService.ResolveChannel();
            Progress<int> progress = new(p => UpdateDownloadProgress = p);

            // このメソッドはUIバインド対象のRelayCommandで、await後の継続でObservableProperty
            // （UI状態）を更新するため、ConfigureAwait(false)は使わずUIスレッドの
            // SynchronizationContextへの自動復帰に委ねる。バックグラウンドスレッドから
            // Dispatcher.Invokeで戻す方式は、モーダルダイアログの入れ子メッセージループとの
            // 組み合わせでスレッド検証に失敗する事例が実機で確認されたため採用しない。
            UpdateDownloadResult downloadResult = await _updateService.DownloadUpdateAsync(_pendingUpdate, channel, progress).ConfigureAwait(true);
            if (!downloadResult.Success)
            {
                _logService?.LogWarning($"アップデートのダウンロードに失敗しました: {downloadResult.Failure}", "MainViewModel");
                UpdateNotificationMessage = downloadResult.Failure == UpdateDownloadFailure.ChecksumMismatch
                    ? LocalizedLogHelper.GetString("Update.Error.ChecksumMismatch")
                    : LocalizedLogHelper.GetString("Update.Error.DownloadFailed");
                return;
            }

            bool applied = await _updateService.ApplyUpdateAsync(_pendingUpdate).ConfigureAwait(true);
            if (applied)
            {
                _logService?.LogInformation("アップデート適用プロセスを起動しました。アプリケーションを終了します。", "MainViewModel");
                ShutdownRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _logService?.LogWarning("アップデートの適用が開始されませんでした（UACキャンセル等）", "MainViewModel");
            }
        }
        // CA1031: RelayCommandハンドラーの最上位try-catch。ネットワーク・ファイルI/O・プロセス起動など
        // 例外種別が多岐にわたり、UIスレッドをクラッシュさせないための意図的な汎用catch。
#pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"アップデート適用中にエラーが発生しました: {ex.Message}", "MainViewModel", ex);
            UpdateNotificationMessage = LocalizedLogHelper.GetString("Update.Error.DownloadFailed");
        }
#pragma warning restore CA1031
        finally
        {
            IsUpdateDownloading = false;
        }
    }

    /// <summary>
    /// 「次回起動時」コマンド。<see cref="AppSettings.UpdatePendingOnNextLaunch"/>を立てて保存し、通知バーを閉じる.
    /// </summary>
    [RelayCommand]
    private async Task DeferUpdateAsync()
    {
        try
        {
            AppSettings appSettings = await _settingsService.LoadAppSettingsAsync().ConfigureAwait(true);
            appSettings.UpdatePendingOnNextLaunch = true;
            _ = await _settingsService.SaveAppSettingsAsync(appSettings).ConfigureAwait(true);
        }
        // CA1031: RelayCommandハンドラーの最上位try-catch。設定保存の失敗で通知バーを閉じる操作自体を
        // 妨げないようにするための意図的な汎用catch。
#pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"更新の次回起動時設定の保存に失敗しました: {ex.Message}", "MainViewModel", ex);
        }
#pragma warning restore CA1031
        finally
        {
            IsUpdateNotificationVisible = false;
        }
    }

    /// <summary>
    /// 「このバージョンをスキップ」コマンド。<see cref="AppSettings.SkippedUpdateVersion"/>に保存し、通知バーを閉じる.
    /// </summary>
    [RelayCommand]
    private async Task SkipUpdateAsync()
    {
        try
        {
            if (_pendingUpdate != null)
            {
                AppSettings appSettings = await _settingsService.LoadAppSettingsAsync().ConfigureAwait(true);
                appSettings.SkippedUpdateVersion = _pendingUpdate.TagName;
                _ = await _settingsService.SaveAppSettingsAsync(appSettings).ConfigureAwait(true);
            }
        }
        // CA1031: RelayCommandハンドラーの最上位try-catch。設定保存の失敗で通知バーを閉じる操作自体を
        // 妨げないようにするための意図的な汎用catch。
#pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"更新のスキップ設定の保存に失敗しました: {ex.Message}", "MainViewModel", ex);
        }
#pragma warning restore CA1031
        finally
        {
            IsUpdateNotificationVisible = false;
        }
    }

    /// <summary>
    /// 「リリースノート」コマンド。<see cref="IExternalLinkService"/>経由でリリースページを開く.
    /// </summary>
    [RelayCommand]
    private async Task OpenUpdateReleaseNotesAsync()
    {
        if (_externalLinkService == null || _pendingUpdate == null || string.IsNullOrEmpty(_pendingUpdate.ReleasePageUrl))
        {
            return;
        }

        try
        {
            _ = await _externalLinkService.OpenAsync(_pendingUpdate.ReleasePageUrl).ConfigureAwait(true);
        }
        // CA1031: RelayCommandハンドラーの最上位try-catch。ブラウザ起動処理は例外種別が多岐にわたり、
        // UIスレッドをクラッシュさせないための意図的な汎用catch。
#pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"リリースノートを開く際にエラーが発生しました: {ex.Message}", "MainViewModel", ex);
        }
#pragma warning restore CA1031
    }
}
