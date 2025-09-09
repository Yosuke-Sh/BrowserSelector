using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using System.IO;
using System.Net.Http;
using System.Text.Json;

namespace BrowserSelector.Infrastructure.Updates;

/// <summary>
/// 自動アップデート機能を提供するサービス.
/// </summary>
public class UpdateService : IUpdateService
{
    private readonly HttpClient _httpClient;
    private readonly string _updateCheckUrl;
    private readonly string _currentVersion;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateService"/> class.
    /// アップデートサービスを初期化.
    /// </summary>
    /// <param name="updateCheckUrl">updateCheckUrl.</param>
    /// <param name="currentVersion">currentVersion.</param>
    public UpdateService(Uri updateCheckUrl, string currentVersion)
    {
        ArgumentNullException.ThrowIfNull(updateCheckUrl);
        ArgumentNullException.ThrowIfNull(currentVersion);
        _updateCheckUrl = updateCheckUrl.ToString();
        _currentVersion = currentVersion;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "BrowserSelector-UpdateChecker");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateService"/> class.
    /// アップデートサービスを初期化.
    /// </summary>
    /// <param name="updateCheckUrl">updateCheckUrl.</param>
    /// <param name="currentVersion">currentVersion.</param>
    public UpdateService(string updateCheckUrl, string currentVersion)
    {
        _updateCheckUrl = updateCheckUrl;
        _currentVersion = currentVersion;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "BrowserSelector-UpdateChecker");
    }

    /// <inheritdoc/>
    public event EventHandler<UpdateAvailableEventArgs>? UpdateAvailable;

    /// <summary>
    /// アップデートをチェック.
    /// </summary>
    /// <returns>bool.</returns>
    /// <exception cref="UpdateException">UpdateException.</exception>
    public async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        try
        {
            string response = await _httpClient.GetStringAsync(new Uri(_updateCheckUrl)).ConfigureAwait(false);
            UpdateInfo? updateInfo = JsonSerializer.Deserialize<UpdateInfo>(response);

            if (updateInfo != null && IsNewerVersion(updateInfo.Version))
            {
                UpdateAvailable?.Invoke(this, new UpdateAvailableEventArgs(updateInfo));
                return updateInfo;
            }

            return null;
        }
        catch (Exception ex)
        {
            throw new UpdateException($"アップデートチェックに失敗しました: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// アップデートをダウンロード.
    /// </summary>
    /// <param name="updateInfo">updateInfo.</param>
    /// <param name="progress">progress.</param>
    /// <returns>bool.</returns>
    /// <exception cref="UpdateException">UpdateException.</exception>
    public async Task<bool> DownloadUpdateAsync(UpdateInfo updateInfo, IProgress<int>? progress = null)
    {
        ArgumentNullException.ThrowIfNull(updateInfo);
        try
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            HttpResponseMessage response = await _httpClient.GetAsync(updateInfo.DownloadUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();

            long totalBytes = response.Content.Headers.ContentLength ?? 0;
            long downloadedBytes = 0L;

            using Stream contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using FileStream fileStream = new(tempPath, FileMode.Create, FileAccess.Write, FileShare.None);

            byte[] buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer).ConfigureAwait(false)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead)).ConfigureAwait(false);
                downloadedBytes += bytesRead;

                if (totalBytes > 0 && progress != null)
                {
                    int percentage = (int)(downloadedBytes * 100 / totalBytes);
                    progress.Report(percentage);
                }
            }

            updateInfo.LocalFilePath = tempPath;
            return true;
        }
        catch (Exception ex)
        {
            throw new UpdateException($"アップデートのダウンロードに失敗しました: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// アップデートをインストール.
    /// </summary>
    /// <param name="updateInfo">updateInfo.</param>
    /// <returns>bool.</returns>
    /// <exception cref="UpdateException">UpdateException.</exception>
    public async Task<bool> InstallUpdateAsync(UpdateInfo updateInfo)
    {
        ArgumentNullException.ThrowIfNull(updateInfo);
        try
        {
            if (string.IsNullOrEmpty(updateInfo.LocalFilePath) || !File.Exists(updateInfo.LocalFilePath))
            {
                throw new UpdateException("ダウンロードされたファイルが見つかりません");
            }

            // インストーラーを起動
            System.Diagnostics.Process process = new()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = updateInfo.LocalFilePath,
                    UseShellExecute = true,
                    Verb = "runas" // 管理者権限で実行
                }
            };

            bool result = process.Start();
            if (result)
            {
                // アプリケーションを終了
                await Task.Delay(1000).ConfigureAwait(false); // インストーラーが起動するまで少し待機
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            throw new UpdateException($"アップデートのインストールに失敗しました: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Task.
    /// </summary>
    /// <returns>bool.</returns>
    /// <exception cref="UpdateException">UpdateException.</exception>
    public async Task<bool> RollbackUpdateAsync()
    {
        try
        {
            // バックアップファイルから復元
            string backupPath = GetBackupPath();
            if (File.Exists(backupPath))
            {
                string currentExePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

                // 現在のファイルをバックアップ
                File.Copy(currentExePath, tempPath, true);

                // バックアップから復元
                File.Copy(backupPath, currentExePath, true);

                // アプリケーションを再起動
                System.Diagnostics.Process process = new()
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = currentExePath,
                        UseShellExecute = true
                    }
                };

                bool result = process.Start();
                if (result)
                {
                    await Task.Delay(1000).ConfigureAwait(false);
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            throw new UpdateException($"アップデートのロールバックに失敗しました: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// バックアップを作成.
    /// </summary>
    /// <returns>bool.</returns>
    public bool CreateBackup()
    {
        try
        {
            string currentExePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string backupPath = GetBackupPath();

            if (File.Exists(currentExePath))
            {
                File.Copy(currentExePath, backupPath, true);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            throw new UpdateException($"バックアップの作成に失敗しました: {ex.Message}", ex);
        }
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
        if (disposing)
        {
            _httpClient?.Dispose();
        }
    }

    private static string GetBackupPath()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string backupDir = Path.Combine(appDataPath, "BrowserSelector", "Backup");
        _ = Directory.CreateDirectory(backupDir);
        return Path.Combine(backupDir, "BrowserSelector.exe.backup");
    }

    private bool IsNewerVersion(string newVersion)
    {
        try
        {
            Version current = new(_currentVersion);
            Version newer = new(newVersion);
            return newer > current;
        }
        catch (ArgumentException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Version comparison failed (ArgumentException): {ex.Message}");
            return false;
        }
        catch (FormatException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Version comparison failed (FormatException): {ex.Message}");
            return false;
        }
        catch (OverflowException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Version comparison failed (OverflowException): {ex.Message}");
            return false;
        }
    }
}

/// <summary>
/// アップデート例外.
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
