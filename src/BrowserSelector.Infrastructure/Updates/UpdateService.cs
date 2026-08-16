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
    private readonly string _baseDirectory;

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
        : this(
            httpClientFactory,
            settingsService,
            logService,
            UpdatePaths.GetCheckStatePath(),
            AppInfo.CurrentVersion,
            AppContext.BaseDirectory)
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
    /// <param name="baseDirectory">実行ファイルの配置ディレクトリ.</param>
    internal UpdateService(
        IHttpClientFactory httpClientFactory,
        ISettingsService settingsService,
        ILogService logService,
        string checkStatePath,
        Version currentVersion,
        string baseDirectory)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(settingsService);
        ArgumentNullException.ThrowIfNull(logService);
        ArgumentNullException.ThrowIfNull(checkStatePath);
        ArgumentNullException.ThrowIfNull(currentVersion);
        ArgumentNullException.ThrowIfNull(baseDirectory);

        _httpClientFactory = httpClientFactory;
        _settingsService = settingsService;
        _logService = logService;
        _checkStatePath = checkStatePath;
        _currentVersion = currentVersion;
        _baseDirectory = baseDirectory;
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
    public async Task<UpdateDownloadResult> DownloadUpdateAsync(
        UpdateInfo updateInfo,
        UpdateChannel channel,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updateInfo);

        UpdateAsset? asset = channel == UpdateChannel.Installer
            ? updateInfo.InstallerAsset
            : updateInfo.PortableAsset;

        if (asset == null)
        {
            _logService.LogWarning($"{channel}チャネルに対応するアセットがリリースに存在しません", LogCategory);
            return UpdateDownloadResult.Failed(UpdateDownloadFailure.Network);
        }

        if (updateInfo.ChecksumsAsset == null)
        {
            // コード署名が無い以上、検証を省略可にはしない。
            _logService.LogWarning("SHA256SUMS.txtがリリースに存在しないため、検証できず中止します", LogCategory);
            return UpdateDownloadResult.Failed(UpdateDownloadFailure.ChecksumUnavailable);
        }

        string versionDirectory = UpdatePaths.GetVersionDirectory(updateInfo.Version);
        string destinationPath = Path.Combine(versionDirectory, asset.Name);

        try
        {
            _ = Directory.CreateDirectory(versionDirectory);

            if (!HasSufficientDiskSpace(versionDirectory, asset.Size))
            {
                _logService.LogWarning("ダウンロードに必要なディスク空き容量が不足しています", LogCategory);
                return UpdateDownloadResult.Failed(UpdateDownloadFailure.Io);
            }

            HttpClient client = _httpClientFactory.CreateClient(HttpClientName);

            // 先にチェックサムを取得する。取得できないならダウンロードする意味がない。
            string? checksumsContent = await DownloadStringAsync(client, updateInfo.ChecksumsAsset.DownloadUrl, cancellationToken)
                .ConfigureAwait(false);

            if (checksumsContent == null)
            {
                return UpdateDownloadResult.Failed(UpdateDownloadFailure.ChecksumUnavailable);
            }

            IReadOnlyDictionary<string, string> checksums = ChecksumFile.Parse(checksumsContent);
            if (!checksums.TryGetValue(asset.Name, out string? expectedHash))
            {
                _logService.LogWarning($"SHA256SUMS.txtに{asset.Name}のエントリがありません", LogCategory);
                return UpdateDownloadResult.Failed(UpdateDownloadFailure.ChecksumUnavailable);
            }

            if (!await DownloadFileAsync(client, asset, destinationPath, progress, cancellationToken).ConfigureAwait(false))
            {
                return UpdateDownloadResult.Failed(UpdateDownloadFailure.Network);
            }

            string actualHash = await ComputeSha256Async(destinationPath, cancellationToken).ConfigureAwait(false);

            if (!string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase))
            {
                // 改竄または破損。実行可能な状態で残さないよう必ず削除する。
                _logService.LogError(
                    $"ダウンロードしたファイルのSHA256が一致しません（{asset.Name}）。ファイルを削除しました",
                    LogCategory);
                TryDeleteFile(destinationPath);
                return UpdateDownloadResult.Failed(UpdateDownloadFailure.ChecksumMismatch);
            }

            _logService.LogInformation($"{asset.Name}のダウンロードと検証が完了しました", LogCategory);

            if (channel == UpdateChannel.Installer)
            {
                updateInfo.LocalFilePath = destinationPath;
                updateInfo.IsDownloaded = true;
                return UpdateDownloadResult.Succeeded(destinationPath);
            }

            // Portableは展開まで行い、Updaterには展開済みディレクトリのコピーだけを担わせる。
            string extractedPath = Path.Combine(versionDirectory, "extracted");
            if (Directory.Exists(extractedPath))
            {
                Directory.Delete(extractedPath, recursive: true);
            }

            if (!ZipExtractor.TryExtract(destinationPath, extractedPath, out string? failureReason))
            {
                _logService.LogError($"ポータブルZIPの展開に失敗しました: {failureReason}", LogCategory);
                TryDeleteFile(destinationPath);
                return UpdateDownloadResult.Failed(UpdateDownloadFailure.Io);
            }

            updateInfo.LocalFilePath = extractedPath;
            updateInfo.IsDownloaded = true;
            return UpdateDownloadResult.Succeeded(extractedPath);
        }
        catch (OperationCanceledException)
        {
            _logService.LogDebug("アップデートのダウンロードがキャンセルされました", LogCategory);
            TryDeleteFile(destinationPath);
            return UpdateDownloadResult.Canceled();
        }
        catch (HttpRequestException ex)
        {
            _logService.LogInformation($"アップデートのダウンロードに失敗しました（ネットワーク）: {ex.Message}", LogCategory);
            TryDeleteFile(destinationPath);
            return UpdateDownloadResult.Failed(UpdateDownloadFailure.Network);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logService.LogWarning($"アップデートのダウンロードに失敗しました（入出力）: {ex.Message}", LogCategory);
            TryDeleteFile(destinationPath);
            return UpdateDownloadResult.Failed(UpdateDownloadFailure.Io);
        }
    }

    /// <inheritdoc/>
    public Task<bool> ApplyUpdateAsync(UpdateInfo updateInfo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updateInfo);

        // H-6で実装する。
        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public UpdateChannel ResolveChannel() => ResolveChannelFor(_baseDirectory);

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 指定ディレクトリを実行位置とみなして適用経路を判定する（テスト用に分離）.
    /// </summary>
    /// <param name="baseDirectory">実行ファイルの配置ディレクトリ.</param>
    /// <returns>適用経路.</returns>
    /// <remarks>
    /// .issがDefaultDirName={autopf} + PrivilegesRequired=adminのため、既定インストールは
    /// Program Files配下になる。よってInstallerルートが実質の主経路.
    /// </remarks>
    internal static UpdateChannel ResolveChannelFor(string baseDirectory)
    {
        ArgumentNullException.ThrowIfNull(baseDirectory);

        // Program Files配下は昇格なしに書き換えられないため、必ずインストーラ経由で更新する。
        if (IsUnderProgramFiles(baseDirectory))
        {
            return UpdateChannel.Installer;
        }

        // 書き込めない場所（管理者が配置した共有フォルダ等）もUpdater.exeでは置換できない。
        if (!IsDirectoryWritable(baseDirectory))
        {
            return UpdateChannel.Installer;
        }

        return UpdateChannel.Portable;
    }

    /// <summary>
    /// リソースを解放します.
    /// </summary>
    /// <param name="disposing">マネージドリソースを解放するかどうか.</param>
    protected virtual void Dispose(bool disposing)
    {
        // HttpClientはIHttpClientFactoryが管理するためここではDisposeしない。
    }

    private static bool IsUnderProgramFiles(string directory)
    {
        string normalized = NormalizeDirectory(directory);

        foreach (Environment.SpecialFolder folder in new[]
        {
            Environment.SpecialFolder.ProgramFiles,
            Environment.SpecialFolder.ProgramFilesX86,
        })
        {
            string programFiles = Environment.GetFolderPath(folder);
            if (string.IsNullOrEmpty(programFiles))
            {
                continue;
            }

            string prefix = NormalizeDirectory(programFiles) + Path.DirectorySeparatorChar;
            if (normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizeDirectory(string path) =>
        Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    private static bool IsDirectoryWritable(string directory)
    {
        try
        {
            if (!Directory.Exists(directory))
            {
                return false;
            }

            // 実際に書けるかどうかは、権限の組み合わせが複雑なため試すのが確実。
            string probe = Path.Combine(directory, $".bs_write_probe_{Guid.NewGuid():N}");
            using (FileStream stream = File.Create(probe, 1, FileOptions.DeleteOnClose))
            {
                return true;
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return false;
        }
    }

    private static bool HasSufficientDiskSpace(string directory, long requiredBytes)
    {
        try
        {
            string? root = Path.GetPathRoot(Path.GetFullPath(directory));
            if (string.IsNullOrEmpty(root))
            {
                return true;
            }

            // ZIPは展開でもう1部使うため、ダウンロード + 展開 + 余裕で3倍を要求する。
            return new DriveInfo(root).AvailableFreeSpace > (requiredBytes * 3);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
        {
            // 空き容量を判定できない場合は続行し、実際の書き込み失敗に委ねる。
            return true;
        }
    }

    private static async Task<string> ComputeSha256Async(string filePath, CancellationToken cancellationToken)
    {
        FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        await using (stream.ConfigureAwait(false))
        {
            byte[] hash = await System.Security.Cryptography.SHA256.HashDataAsync(stream, cancellationToken)
                .ConfigureAwait(false);

            // ChecksumFile.Parseと同じく大文字hexへ正規化して比較する。
            return Convert.ToHexString(hash);
        }
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

    private void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logService.LogDebug($"一時ファイルの削除に失敗しました（{path}）: {ex.Message}", LogCategory);
        }
    }

    private async Task<string?> DownloadStringAsync(HttpClient client, Uri url, CancellationToken cancellationToken)
    {
        if (!UpdateHostValidator.IsAllowedHost(url))
        {
            _logService.LogError($"許可されていないホストからのダウンロードを拒否しました: {url.Host}", LogCategory);
            return null;
        }

        using HttpResponseMessage response = await client.GetAsync(url, cancellationToken).ConfigureAwait(false);

        if (!UpdateHostValidator.IsAllowedHost(response.RequestMessage?.RequestUri))
        {
            _logService.LogError(
                $"ダウンロードがリダイレクトされ、許可されていないホストへ到達しました: {response.RequestMessage?.RequestUri?.Host}",
                LogCategory);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logService.LogWarning($"ダウンロードに失敗しました（HTTP {(int)response.StatusCode}）: {url}", LogCategory);
            return null;
        }

        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<bool> DownloadFileAsync(
        HttpClient client,
        UpdateAsset asset,
        string destinationPath,
        IProgress<int>? progress,
        CancellationToken cancellationToken)
    {
        if (!UpdateHostValidator.IsAllowedHost(asset.DownloadUrl))
        {
            _logService.LogError($"許可されていないホストからのダウンロードを拒否しました: {asset.DownloadUrl.Host}", LogCategory);
            return false;
        }

        using HttpResponseMessage response = await client
            .GetAsync(asset.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        if (!UpdateHostValidator.IsAllowedHost(response.RequestMessage?.RequestUri))
        {
            _logService.LogError(
                $"ダウンロードがリダイレクトされ、許可されていないホストへ到達しました: {response.RequestMessage?.RequestUri?.Host}",
                LogCategory);
            return false;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logService.LogWarning($"ダウンロードに失敗しました（HTTP {(int)response.StatusCode}）: {asset.Name}", LogCategory);
            return false;
        }

        long totalBytes = response.Content.Headers.ContentLength ?? asset.Size;
        long downloadedBytes = 0;
        int lastReported = -1;

        Stream contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using (contentStream.ConfigureAwait(false))
        {
            // 元のアセット名（＝拡張子）のまま保存する。
            // v0.2.0はPath.GetRandomFileName()で拡張子なしに保存しており、インストーラを
            // Process.Startしても起動しなかった。
            FileStream fileStream = new(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await using (fileStream.ConfigureAwait(false))
            {
                byte[] buffer = new byte[81920];
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
                    downloadedBytes += bytesRead;

                    if (totalBytes > 0 && progress != null)
                    {
                        int percentage = (int)Math.Min(100, downloadedBytes * 100 / totalBytes);
                        if (percentage != lastReported)
                        {
                            lastReported = percentage;
                            progress.Report(percentage);
                        }
                    }
                }
            }
        }

        return true;
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
