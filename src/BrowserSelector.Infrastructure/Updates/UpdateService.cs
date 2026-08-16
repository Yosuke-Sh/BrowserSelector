// <copyright file="UpdateService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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

    /// <summary>
    /// ポータブル更新を担う別プロセスの実行ファイル名.
    /// </summary>
    internal const string UpdaterExecutableName = "BrowserSelector.Updater.exe";

    /// <summary>
    /// 更新後に再起動する本体の実行ファイル名.
    /// </summary>
    internal const string ApplicationExecutableName = "BrowserSelector.exe";

    /// <summary>
    /// Inno Setupのサイレント実行引数.
    /// </summary>
    /// <remarks>
    /// /VERYSILENTではなく/SILENTを選ぶ — 進捗を見せた方が「勝手に何か動いた」という不安が小さい.
    /// </remarks>
    internal const string InstallerArguments = "/SILENT /CLOSEAPPLICATIONS /RESTARTAPPLICATIONS /NORESTART";

    /// <summary>
    /// UACダイアログをユーザーがキャンセルしたときのWin32エラーコード（ERROR_CANCELLED）.
    /// </summary>
    private const int ErrorCancelled = 1223;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISettingsService _settingsService;
    private readonly ILogService _logService;
    private readonly IProcessLauncher _processLauncher;
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
            AppContext.BaseDirectory,
            new ProcessLauncher())
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
    /// <param name="processLauncher">プロセス起動の抽象.</param>
    internal UpdateService(
        IHttpClientFactory httpClientFactory,
        ISettingsService settingsService,
        ILogService logService,
        string checkStatePath,
        Version currentVersion,
        string baseDirectory,
        IProcessLauncher processLauncher)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(settingsService);
        ArgumentNullException.ThrowIfNull(logService);
        ArgumentNullException.ThrowIfNull(checkStatePath);
        ArgumentNullException.ThrowIfNull(currentVersion);
        ArgumentNullException.ThrowIfNull(baseDirectory);
        ArgumentNullException.ThrowIfNull(processLauncher);

        _httpClientFactory = httpClientFactory;
        _settingsService = settingsService;
        _logService = logService;
        _processLauncher = processLauncher;
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

            HttpClient client = _httpClientFactory.CreateClient(HttpClientName);

            // ETagが前回チェック時点でキャッシュされたタグに対するものであり、キャッシュされたタグ自体が
            // まだ現在バージョンより新しい（＝前回検出した更新が未適用の）場合は、304で早期リターンせず
            // 完全なUpdateInfoを取得し直す。304時点ではボディが無くUpdateAssetsを再構築できないため、
            // ここでETagを送らずフルリクエストにフォールバックする（適用済みなら通常どおりETagで
            // レート制限を節約する）。
            bool forceFullRequest = IsCachedTagNewerThanCurrent(state.CachedTagName);
            using HttpRequestMessage request = new(HttpMethod.Get, _latestReleaseApiUrl);
            if (!forceFullRequest && !string.IsNullOrEmpty(state.ETag))
            {
                request.Headers.TryAddWithoutValidation("If-None-Match", state.ETag);
            }

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

        if (!updateInfo.IsDownloaded || string.IsNullOrEmpty(updateInfo.LocalFilePath))
        {
            _logService.LogWarning("ダウンロードと検証が完了していないため適用できません", LogCategory);
            return Task.FromResult(false);
        }

        cancellationToken.ThrowIfCancellationRequested();

        // 適用は「プロセスを起動して自分は終了する」だけなので同期処理で完結する。
        // インターフェースがTaskを返すのは、将来インストーラの終了待ちを挟めるようにするため。
        bool result = ResolveChannel() == UpdateChannel.Installer
            ? ApplyWithInstaller(updateInfo)
            : ApplyWithUpdater(updateInfo);

        return Task.FromResult(result);
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

    /// <summary>
    /// 前回304応答時にキャッシュしたタグ名が、現在の実行バージョンより新しいかどうかを判定する.
    /// </summary>
    /// <remarks>
    /// 「一度更新を検出したがユーザーが適用しなかった」場合、次回以降はETagにより304が返り続け、
    /// バージョン比較（<see cref="CheckForUpdatesAsync"/>本体）に到達できず「更新なし」のまま
    /// 通知され続けなくなる不具合があった。ここでキャッシュタグと現在バージョンを比較し、
    /// まだ新しいままであれば呼び出し元でETagを送らずフルリクエストへフォールバックさせる.
    /// </remarks>
    /// <param name="cachedTagName">前回304応答時にキャッシュしたタグ名.</param>
    /// <returns>キャッシュタグが現在バージョンより新しい場合はtrue.</returns>
    private bool IsCachedTagNewerThanCurrent(string? cachedTagName)
    {
        if (string.IsNullOrEmpty(cachedTagName))
        {
            return false;
        }

        return GitHubReleaseMapper.TryParseVersion(cachedTagName, out Version? cachedVersion)
            && cachedVersion! > _currentVersion;
    }

    /// <summary>
    /// インストーラを昇格つきで起動する（主経路）.
    /// </summary>
    /// <param name="updateInfo">ダウンロード済みのアップデート情報.</param>
    /// <returns>起動できた場合はtrue.</returns>
    /// <remarks>
    /// .issがDefaultDirName={autopf} + PrivilegesRequired=adminのため、既定インストールでは
    /// こちらが実質の主経路になる.
    /// </remarks>
    private bool ApplyWithInstaller(UpdateInfo updateInfo)
    {
        string installerPath = updateInfo.LocalFilePath!;

        if (!File.Exists(installerPath))
        {
            _logService.LogError($"インストーラが見つかりません: {installerPath}", LogCategory);
            return false;
        }

        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = installerPath,
                Arguments = InstallerArguments,
                UseShellExecute = true,
                Verb = "runas",
            };

            _ = _processLauncher.Start(startInfo);
            _logService.LogInformation($"インストーラを起動しました: {Path.GetFileName(installerPath)}", LogCategory);
            return true;
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == ErrorCancelled)
        {
            // ユーザーがUACを明示的に拒否した。意図した操作なのでエラーダイアログは出さない。
            _logService.LogInformation("ユーザーがアップデートの昇格をキャンセルしました", LogCategory);
            return false;
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            _logService.LogError($"インストーラの起動に失敗しました: {ex.Message}", LogCategory);
            return false;
        }
    }

    /// <summary>
    /// 展開済みディレクトリをUpdater.exeへ渡して適用させる（ポータブル経路）.
    /// </summary>
    /// <param name="updateInfo">ダウンロード済みのアップデート情報.</param>
    /// <returns>起動できた場合はtrue.</returns>
    private bool ApplyWithUpdater(UpdateInfo updateInfo)
    {
        string extractedPath = updateInfo.LocalFilePath!;

        if (!Directory.Exists(extractedPath))
        {
            _logService.LogError($"展開済みディレクトリが見つかりません: {extractedPath}", LogCategory);
            return false;
        }

        string updaterPath = Path.Combine(_baseDirectory, UpdaterExecutableName);
        if (!File.Exists(updaterPath))
        {
            _logService.LogError($"{UpdaterExecutableName}が見つかりません: {updaterPath}", LogCategory);
            return false;
        }

        string backupPath = Path.Combine(
            UpdatePaths.GetBackupRoot(),
            DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture));

        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = updaterPath,
                UseShellExecute = false,
            };

            // 名前付き引数で渡す（パスに空白が含まれてもArgumentListなら引用符の心配が要らない）。
            startInfo.ArgumentList.Add("--mode");
            startInfo.ArgumentList.Add("apply-zip");
            startInfo.ArgumentList.Add("--pid");
            startInfo.ArgumentList.Add(Environment.ProcessId.ToString(CultureInfo.InvariantCulture));
            startInfo.ArgumentList.Add("--source");
            startInfo.ArgumentList.Add(extractedPath);
            startInfo.ArgumentList.Add("--target");
            startInfo.ArgumentList.Add(_baseDirectory);
            startInfo.ArgumentList.Add("--backup");
            startInfo.ArgumentList.Add(backupPath);
            startInfo.ArgumentList.Add("--exe");
            startInfo.ArgumentList.Add(ApplicationExecutableName);

            _ = _processLauncher.Start(startInfo);
            _logService.LogInformation($"{UpdaterExecutableName}を起動しました（backup: {backupPath}）", LogCategory);
            return true;
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            _logService.LogError($"{UpdaterExecutableName}の起動に失敗しました: {ex.Message}", LogCategory);
            return false;
        }
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
