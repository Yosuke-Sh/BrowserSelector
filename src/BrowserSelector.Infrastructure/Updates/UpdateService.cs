// <copyright file="UpdateService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using BrowserSelector.Core;
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;

namespace BrowserSelector.Infrastructure.Updates;

/// <summary>
/// GitHub Releasesを用いた自動アップデート機能を提供するサービス（Phase H）.
/// </summary>
public class UpdateService : IUpdateService
{
    /// <summary>
    /// <see cref="IHttpClientFactory"/>へ登録する名前付きクライアントの名前.
    /// </summary>
    public const string HttpClientName = "BrowserSelector.Updates";

    /// <summary>
    /// ログ出力時のカテゴリ名.
    /// </summary>
    internal const string LogCategory = "Update";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISettingsService _settingsService;
    private readonly ILogService _logService;
    private readonly string _checkStatePath;
    private readonly Uri _latestReleaseApiUrl;
    private readonly Version _currentVersion;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">HTTPクライアントファクトリ.</param>
    /// <param name="settingsService">設定サービス.</param>
    /// <param name="logService">ログサービス.</param>
    public UpdateService(
        IHttpClientFactory httpClientFactory,
        ISettingsService settingsService,
        ILogService logService)
        : this(httpClientFactory, settingsService, logService, UpdatePaths.GetCheckStatePath(), AppInfo.CurrentVersion)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateService"/> class.
    /// テスト用に状態ファイルのパスと現在バージョンを差し替えられるコンストラクタ.
    /// </summary>
    /// <param name="httpClientFactory">HTTPクライアントファクトリ.</param>
    /// <param name="settingsService">設定サービス.</param>
    /// <param name="logService">ログサービス.</param>
    /// <param name="checkStatePath">チェック状態ファイルのパス.</param>
    /// <param name="currentVersion">現在のアプリケーションバージョン.</param>
    internal UpdateService(
        IHttpClientFactory httpClientFactory,
        ISettingsService settingsService,
        ILogService logService,
        string checkStatePath,
        Version currentVersion)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(settingsService);
        ArgumentNullException.ThrowIfNull(logService);
        ArgumentNullException.ThrowIfNull(checkStatePath);
        ArgumentNullException.ThrowIfNull(currentVersion);

        _httpClientFactory = httpClientFactory;
        _settingsService = settingsService;
        _logService = logService;
        _checkStatePath = checkStatePath;
        _currentVersion = currentVersion;
        _latestReleaseApiUrl = new Uri(AppInfo.LatestReleaseApiUrl);
    }

    /// <inheritdoc/>
    public event EventHandler<UpdateAvailableEventArgs>? UpdateAvailable;

    /// <inheritdoc/>
    public async Task<UpdateInfo?> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            AppSettings settings = await _settingsService.LoadAppSettingsAsync().ConfigureAwait(false);

            UpdateCheckState state = LoadCheckState();

            // レート制限中は通信そのものを行わない（403を再度食らってもリセットが延びるだけのため）。
            if (state.RateLimitResetUtc is { } resetAt && resetAt > DateTimeOffset.UtcNow)
            {
                _logService.LogInformation(
                    $"GitHub APIのレート制限中のためアップデート確認を見送ります（解除予定: {resetAt:u}）",
                    LogCategory);
                return null;
            }

            // ホスト検証はAPI URLに対しても行う（AppInfoの改変・設定ミスに対する防御）。
            if (!UpdateHostValidator.IsAllowedHost(_latestReleaseApiUrl))
            {
                _logService.LogError($"アップデート確認先のホストが許可されていません: {_latestReleaseApiUrl.Host}", LogCategory);
                return null;
            }

            using HttpRequestMessage request = new(HttpMethod.Get, _latestReleaseApiUrl);
            if (!string.IsNullOrEmpty(state.ETag))
            {
                request.Headers.TryAddWithoutValidation("If-None-Match", state.ETag);
            }

            HttpClient client = _httpClientFactory.CreateClient(HttpClientName);
            using HttpResponseMessage response = await client
                .SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken)
                .ConfigureAwait(false);

            // AllowAutoRedirect=trueのため、リダイレクト後の最終URLも必ず検証する。
            if (!UpdateHostValidator.IsAllowedHost(response.RequestMessage?.RequestUri))
            {
                _logService.LogError(
                    $"アップデート確認がリダイレクトされ、許可されていないホストへ到達しました: {response.RequestMessage?.RequestUri?.Host}",
                    LogCategory);
                return null;
            }

            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                // 304はレート制限を消費しない。前回から変化が無いので確認済みとして記録だけ更新する。
                _logService.LogDebug($"アップデートに変更はありません（304 Not Modified, {state.CachedTagName}）", LogCategory);
                state.LastCheckedUtc = DateTimeOffset.UtcNow;
                SaveCheckState(state);
                return null;
            }

            if (response.StatusCode == HttpStatusCode.Forbidden && IsRateLimited(response))
            {
                state.RateLimitResetUtc = ReadRateLimitReset(response);
                SaveCheckState(state);
                _logService.LogWarning(
                    $"GitHub APIのレート制限に達しました（解除予定: {state.RateLimitResetUtc:u}）",
                    LogCategory);

                // レート制限はユーザーの操作で解決できないためUI通知はしない（ログのみ）。
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logService.LogWarning($"アップデート確認が失敗しました（HTTP {(int)response.StatusCode}）", LogCategory);
                return null;
            }

            string json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            GitHubRelease? release = JsonSerializer.Deserialize<GitHubRelease>(json);

            if (!GitHubReleaseMapper.TryMap(release, out UpdateInfo? updateInfo))
            {
                _logService.LogWarning("リリース情報を解釈できませんでした（下書き、またはタグからバージョンを解決できません）", LogCategory);
                return null;
            }

            state.ETag = response.Headers.ETag?.Tag;
            state.LastCheckedUtc = DateTimeOffset.UtcNow;
            state.CachedTagName = updateInfo!.TagName;
            state.RateLimitResetUtc = null;
            SaveCheckState(state);

            if (updateInfo.IsPrerelease && !settings.IncludePrereleases)
            {
                _logService.LogDebug($"プレリリース {updateInfo.TagName} は設定により対象外です", LogCategory);
                return null;
            }

            if (updateInfo.Version <= _currentVersion)
            {
                _logService.LogDebug($"最新版を使用しています（現在: {_currentVersion} / 最新: {updateInfo.Version}）", LogCategory);
                return null;
            }

            _logService.LogInformation(
                $"新しいバージョンが利用可能です: {updateInfo.TagName}（現在: {_currentVersion}）",
                LogCategory);

            UpdateAvailable?.Invoke(this, new UpdateAvailableEventArgs(updateInfo));
            return updateInfo;
        }
        catch (OperationCanceledException)
        {
            // 起動時チェックはアプリ終了時にキャンセルされる。異常ではないので静かに戻る。
            _logService.LogDebug("アップデート確認がキャンセルされました", LogCategory);
            return null;
        }
        catch (HttpRequestException ex)
        {
            // ネットワーク不通・DNS失敗。ユーザーには通知せずログのみ残す。
            _logService.LogInformation($"アップデート確認に失敗しました（ネットワーク）: {ex.Message}", LogCategory);
            return null;
        }
        catch (JsonException ex)
        {
            _logService.LogWarning($"アップデート情報の解析に失敗しました: {ex.Message}", LogCategory);
            return null;
        }
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
        // HttpClientはIHttpClientFactoryが管理するためここではDisposeしない。
    }

    private static bool IsRateLimited(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("X-RateLimit-Remaining", out IEnumerable<string>? values))
        {
            return false;
        }

        string? remaining = values.FirstOrDefault();
        return string.Equals(remaining, "0", StringComparison.Ordinal);
    }

    private static DateTimeOffset? ReadRateLimitReset(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("X-RateLimit-Reset", out IEnumerable<string>? values)
            && long.TryParse(values.FirstOrDefault(), out long unixSeconds))
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        }

        // ヘッダーが読めない場合は保守的に1時間抑止する（GitHubのレート制限窓と同じ長さ）。
        return DateTimeOffset.UtcNow.AddHours(1);
    }

    private UpdateCheckState LoadCheckState()
    {
        try
        {
            if (!File.Exists(_checkStatePath))
            {
                return new UpdateCheckState();
            }

            string json = File.ReadAllText(_checkStatePath);
            return JsonSerializer.Deserialize<UpdateCheckState>(json) ?? new UpdateCheckState();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            // キャッシュが壊れていても更新確認自体は続行できる（ETagが無いだけ）。
            _logService.LogDebug($"アップデート確認状態の読み込みに失敗しました: {ex.Message}", LogCategory);
            return new UpdateCheckState();
        }
    }

    private void SaveCheckState(UpdateCheckState state)
    {
        try
        {
            string? directory = Path.GetDirectoryName(_checkStatePath);
            if (!string.IsNullOrEmpty(directory))
            {
                _ = Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_checkStatePath, JsonSerializer.Serialize(state));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // 保存できなくても次回は条件付きリクエストにならないだけで動作は継続できる。
            _logService.LogDebug($"アップデート確認状態の保存に失敗しました: {ex.Message}", LogCategory);
        }
    }
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
