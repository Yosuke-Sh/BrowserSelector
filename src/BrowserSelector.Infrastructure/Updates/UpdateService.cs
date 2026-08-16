// <copyright file="UpdateService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;

namespace BrowserSelector.Infrastructure.Updates;

/// <summary>
/// 自動アップデート機能を提供するサービス（Phase H）.
/// </summary>
/// <remarks>
/// H-1時点では新しい<see cref="IUpdateService"/>の形へ合わせた暫定実装であり、
/// 実際のGitHub Releases連携はH-3以降で実装する.
/// </remarks>
public class UpdateService : IUpdateService
{
    /// <summary>
    /// ログ出力時のカテゴリ名.
    /// </summary>
    internal const string LogCategory = "Update";

    private readonly ISettingsService _settingsService;
    private readonly ILogService _logService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateService"/> class.
    /// </summary>
    /// <param name="settingsService">設定サービス.</param>
    /// <param name="logService">ログサービス.</param>
    public UpdateService(ISettingsService settingsService, ILogService logService)
    {
        ArgumentNullException.ThrowIfNull(settingsService);
        ArgumentNullException.ThrowIfNull(logService);
        _settingsService = settingsService;
        _logService = logService;
    }

    /// <inheritdoc/>
    public event EventHandler<UpdateAvailableEventArgs>? UpdateAvailable;

    /// <inheritdoc/>
    public async Task<UpdateInfo?> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        // H-3で実装する。それまでは設定の確認までを行い「更新なし」として扱う。
        AppSettings settings = await _settingsService.LoadAppSettingsAsync().ConfigureAwait(false);
        if (!settings.CheckForUpdates)
        {
            _logService.LogDebug("アップデート確認は設定で無効化されています", LogCategory);
            return null;
        }

        _logService.LogDebug("アップデート確認はまだ実装されていません（Phase H-3で実装予定）", LogCategory);
        return null;
    }

    /// <inheritdoc/>
    public Task<UpdateDownloadResult> DownloadUpdateAsync(
        UpdateInfo updateInfo,
        UpdateChannel channel,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updateInfo);

        // H-4で実装する。
        return Task.FromResult(UpdateDownloadResult.Failed(UpdateDownloadFailure.Network));
    }

    /// <inheritdoc/>
    public Task<bool> ApplyUpdateAsync(UpdateInfo updateInfo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updateInfo);

        // H-6で実装する。
        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public UpdateChannel ResolveChannel()
    {
        // H-4で実装する。既定インストールはProgram Files配下のためInstallerを既定値とする。
        return UpdateChannel.Installer;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// リソースを解放します.
    /// </summary>
    /// <param name="disposing">マネージドリソースを解放するかどうか.</param>
    protected virtual void Dispose(bool disposing)
    {
        // HttpClientはIHttpClientFactory管理（H-3）のためここではDisposeしない。
    }

    /// <summary>
    /// <see cref="UpdateAvailable"/>イベントを発火する.
    /// </summary>
    /// <param name="updateInfo">アップデート情報.</param>
    protected void OnUpdateAvailable(UpdateInfo updateInfo) =>
        UpdateAvailable?.Invoke(this, new UpdateAvailableEventArgs(updateInfo));
}

/// <summary>
/// アップデート適用時の異常を表す例外.
/// </summary>
public class UpdateException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateException"/> class.
    /// </summary>
    /// <param name="message">message.</param>
    public UpdateException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateException"/> class.
    /// </summary>
    /// <param name="message">message.</param>
    /// <param name="innerException">innerException.</param>
    public UpdateException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateException"/> class.
    /// </summary>
    public UpdateException()
    {
    }
}
