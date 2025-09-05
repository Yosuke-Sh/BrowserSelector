using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// 自動アップデート機能を提供するサービスのインターフェース
/// </summary>
public interface IUpdateService : IDisposable
{
    /// <summary>
    /// アップデートが利用可能になった時のイベント
    /// </summary>
    event EventHandler<UpdateAvailableEventArgs>? UpdateAvailable;

    /// <summary>
    /// アップデートをチェック
    /// </summary>
    /// <returns>アップデート情報</returns>
    Task<UpdateInfo?> CheckForUpdatesAsync();

    /// <summary>
    /// アップデートをダウンロード
    /// </summary>
    /// <param name="updateInfo">アップデート情報</param>
    /// <param name="progress">進捗報告</param>
    /// <returns>ダウンロードが成功したかどうか</returns>
    Task<bool> DownloadUpdateAsync(UpdateInfo updateInfo, IProgress<int>? progress = null);

    /// <summary>
    /// アップデートをインストール
    /// </summary>
    /// <param name="updateInfo">アップデート情報</param>
    /// <returns>インストールが成功したかどうか</returns>
    Task<bool> InstallUpdateAsync(UpdateInfo updateInfo);

    /// <summary>
    /// アップデートをロールバック
    /// </summary>
    /// <returns>ロールバックが成功したかどうか</returns>
    Task<bool> RollbackUpdateAsync();

    /// <summary>
    /// バックアップを作成
    /// </summary>
    /// <returns>バックアップが成功したかどうか</returns>
    bool CreateBackup();
}
