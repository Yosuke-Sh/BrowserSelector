// <copyright file="IUpdateService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// 自動アップデート機能を提供するサービスのインターフェース（Phase H-1で再設計）.
/// ロールバックとバックアップはアプリが動いていない状態でしか意味をなさないため、
/// このインターフェースからは削除しBrowserSelector.Updater.exe側へ移設した.
/// </summary>
public interface IUpdateService : IDisposable
{
    /// <summary>
    /// アップデートが利用可能になった時のイベント.
    /// </summary>
    event EventHandler<UpdateAvailableEventArgs>? UpdateAvailable;

    /// <summary>
    /// GitHub Releasesの最新リリースを確認し、現在のバージョンより新しければその情報を返す.
    /// ネットワーク不通・タイムアウト・レート制限といった異常系では例外を投げずnullを返す
    /// （呼び出し側での握り潰しを不要にするため）.
    /// </summary>
    /// <param name="cancellationToken">キャンセルトークン.</param>
    /// <returns>利用可能なアップデート情報。更新が無い場合・確認に失敗した場合はnull.</returns>
    Task<UpdateInfo?> CheckForUpdatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 指定チャネルのアセットをダウンロードし、SHA256による完全性検証を行う.
    /// Portableチャネルの場合はZIPの展開（Zip Slip対策込み）までを行う.
    /// </summary>
    /// <param name="updateInfo">アップデート情報.</param>
    /// <param name="channel">適用経路.</param>
    /// <param name="progress">0-100の進捗報告.</param>
    /// <param name="cancellationToken">キャンセルトークン.</param>
    /// <returns>成否と失敗理由を含む結果.</returns>
    Task<UpdateDownloadResult> DownloadUpdateAsync(
        UpdateInfo updateInfo,
        UpdateChannel channel,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// ダウンロード済みのアップデートを適用する（インストーラ実行、またはUpdater.exeの起動）.
    /// アプリケーションの終了は呼び出し側（ViewModel）の責務であり、このメソッドは行わない.
    /// </summary>
    /// <param name="updateInfo">ダウンロード済みのアップデート情報.</param>
    /// <param name="cancellationToken">キャンセルトークン.</param>
    /// <returns>適用プロセスの起動に成功したかどうか。UACキャンセル時はfalse.</returns>
    Task<bool> ApplyUpdateAsync(UpdateInfo updateInfo, CancellationToken cancellationToken = default);

    /// <summary>
    /// 現在の実行環境がインストーラ配置かポータブル配置かを判定する.
    /// </summary>
    /// <returns>適用経路.</returns>
    UpdateChannel ResolveChannel();
}
