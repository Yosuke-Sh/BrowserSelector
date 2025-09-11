using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.Logging;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text.Json;

namespace BrowserSelector.Infrastructure.Services;

/// <summary>
/// ブラウザ管理サービスの実装.
/// </summary>
public class BrowserService : IBrowserService
{
    private readonly IRegistryService _registryService;
    private readonly IUrlService _urlService;
    private readonly ILogService _logService;
    private readonly List<Browser> _browsers = [];
    private readonly string _settingsDirectory;
    private readonly string _browsersPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserService"/> class.
    /// </summary>
    /// <param name="registryService">registryService.</param>
    /// <param name="urlService">urlService.</param>
    public BrowserService(IRegistryService registryService, IUrlService urlService)
        : this(registryService, urlService, new LogService())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserService"/> class.
    /// </summary>
    /// <param name="registryService">registryService.</param>
    /// <param name="urlService">urlService.</param>
    /// <param name="logService">logService.</param>
    public BrowserService(IRegistryService registryService, IUrlService urlService, ILogService logService)
    {
        _registryService = registryService;
        _urlService = urlService;
        _logService = logService;

        // ユーザーのアプリケーションデータフォルダに設定を保存
        _settingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BrowserSelector");

        // 設定ディレクトリが存在しない場合は作成
        if (!Directory.Exists(_settingsDirectory))
        {
            _ = Directory.CreateDirectory(_settingsDirectory);
        }

        _browsersPath = Path.Combine(_settingsDirectory, "browsers.json");
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Browser>> DetectBrowsersAsync()
    {
        _logService.LogTrace("ブラウザ検出処理開始", "BrowserService");
        try
        {
            // レジストリからブラウザを検出
            IEnumerable<Browser> registryBrowsers = await _registryService.DetectBrowsersFromRegistryAsync().ConfigureAwait(false);
            _browsers.Clear();

            // 自動検出されたブラウザにアイコンを設定
            foreach (Browser browser in registryBrowsers)
            {
                if (string.IsNullOrEmpty(browser.IconPath) && !string.IsNullOrEmpty(browser.ExecutablePath))
                {
                    browser.IconPath = browser.ExecutablePath; // 実行ファイルからアイコンを抽出
                }
            }

            _browsers.AddRange(registryBrowsers);

            // カスタムブラウザを追加（設定から読み込み）
            List<Browser> customBrowsers = await LoadCustomBrowsersAsync().ConfigureAwait(false);
            _browsers.AddRange(customBrowsers);

            // Traceレベルで詳細なブラウザ情報を出力
            string browserDetails = string.Join(", ", _browsers.Select(b => $"{b.Name}({b.Type}, Enabled:{b.IsEnabled})"));
            _logService.LogTrace($"ブラウザ検出処理完了: {_browsers.Count}個のブラウザを検出 - {browserDetails}", "BrowserService");
            return _browsers.OrderBy(b => b.DisplayOrder);
        }
#pragma warning disable CA1031 // テストで例外を投げるために汎用Exceptionキャッチが必要
        catch (Exception ex)
        {
            _logService.LogCritical($"ブラウザ検出でエラーが発生: {ex.Message}", nameof(BrowserService), ex);
            return Enumerable.Empty<Browser>();
        }
#pragma warning restore CA1031
    }

    /// <inheritdoc/>
    public async Task<bool> LaunchBrowserAsync(Browser browser, string url)
    {
        ArgumentNullException.ThrowIfNull(browser);
        ArgumentNullException.ThrowIfNull(url);
        try
        {
            _logService.LogDebug($"ブラウザ起動開始 - {browser.Name}, パス: {browser.ExecutablePath}, URL: {url}", nameof(BrowserService));

            if (!browser.IsValid)
            {
                _logService.LogWarning($"ブラウザが無効です - {browser.Name}", nameof(BrowserService));
                return false;
            }

            if (!File.Exists(browser.ExecutablePath))
            {
                _logService.LogWarning($"実行ファイルが存在しません - {browser.ExecutablePath}", nameof(BrowserService));
                return false;
            }

            // URLを正規化（Uri型に変換してから呼び出し）
            Uri urlUri = new(url);
            string normalizedUrl = await _urlService.NormalizeUrlAsync(urlUri).ConfigureAwait(false);
            _logService.LogDebug($"正規化されたURL - {normalizedUrl}", nameof(BrowserService));

            if (string.IsNullOrEmpty(normalizedUrl))
            {
                _logService.LogWarning("URLの正規化に失敗しました", nameof(BrowserService));
                return false;
            }

            // URLを検証（Uri型に変換してから呼び出し）
            Uri normalizedUri = new(normalizedUrl);
            bool isValidUrl = await _urlService.ValidateUrlAsync(normalizedUri).ConfigureAwait(false);
            _logService.LogDebug($"URL検証結果 - {isValidUrl}", nameof(BrowserService));

            if (!isValidUrl)
            {
                _logService.LogWarning($"URLが無効です - {normalizedUrl}", nameof(BrowserService));
                return false;
            }

            // ブラウザタイプに応じた起動引数を設定
            string arguments = GetBrowserArguments(normalizedUrl);

            // ブラウザを起動
            ProcessStartInfo startInfo = new()
            {
                FileName = browser.ExecutablePath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = false
            };

            _logService.LogDebug($"プロセス起動 - FileName: {startInfo.FileName}, Arguments: {startInfo.Arguments}", nameof(BrowserService));

            using Process? process = Process.Start(startInfo);
            if (process != null)
            {
                _logService.LogInformation($"プロセス起動成功 - PID: {process.Id}", nameof(BrowserService));

                // プロセス情報を取得して確認
                try
                {
                    Process processInfo = Process.GetProcessById(process.Id);
                    _logService.LogDebug($"実際に起動されたプロセス - 名前: {processInfo.ProcessName}, ファイル名: {processInfo.MainModule?.FileName}", nameof(BrowserService));
                }
                catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException)
                {
                    _logService.LogDebug($"プロセス情報取得エラー - {ex.Message}", nameof(BrowserService), ex);
                }

                // 使用回数を増加
                browser.IncrementUseCount();
                await SaveBrowserUsageAsync().ConfigureAwait(false);
                return true;
            }

            _logService.LogError("プロセス起動失敗", nameof(BrowserService));
            return false;
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or UnauthorizedAccessException or IOException or Win32Exception)
        {
            _logService.LogError($"ブラウザ起動エラー - {ex.Message}", nameof(BrowserService), ex);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> LaunchBrowserAsync(Browser browser, Uri url)
    {
        ArgumentNullException.ThrowIfNull(url);
        return await LaunchBrowserAsync(browser, url.ToString()).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> AddBrowserAsync(Browser browser)
    {
        ArgumentNullException.ThrowIfNull(browser);
        try
        {
            if (!browser.IsValid)
            {
                return false;
            }

            // 重複チェック
            if (_browsers.Exists(b => b.ExecutablePath.Equals(browser.ExecutablePath, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            browser.Type = BrowserType.Custom;
            browser.DisplayOrder = _browsers.Count + 1;
            _browsers.Add(browser);

            await SaveCustomBrowsersAsync().ConfigureAwait(false);
            return true;
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
        {
            _logService.LogError($"ブラウザ追加エラー: {ex.Message}", nameof(BrowserService), ex);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateBrowserAsync(Browser browser)
    {
        ArgumentNullException.ThrowIfNull(browser);
        try
        {
            _logService.LogDebug($"ブラウザ更新開始: ID={browser.Id}, Name={browser.Name}", nameof(BrowserService));
            _logService.LogDebug($"現在のブラウザ数: {_browsers.Count}", nameof(BrowserService));

            Browser? existingBrowser = _browsers.Find(b => b.Id == browser.Id);
            if (existingBrowser == null)
            {
                _logService.LogWarning($"更新対象のブラウザが見つかりません: ID={browser.Id}, Name={browser.Name}", nameof(BrowserService));
                _logService.LogDebug($"利用可能なブラウザID: {string.Join(", ", _browsers.Select(b => b.Id))}", nameof(BrowserService));
                return false;
            }

            // プロパティを更新
            _logService.LogDebug($"ブラウザプロパティ更新: IconPath={browser.IconPath}, Arguments={browser.Arguments}", nameof(BrowserService));
            existingBrowser.Name = browser.Name;
            existingBrowser.ExecutablePath = browser.ExecutablePath;
            existingBrowser.IconPath = browser.IconPath;
            existingBrowser.Arguments = browser.Arguments;
            existingBrowser.IsEnabled = browser.IsEnabled;
            existingBrowser.DisplayOrder = browser.DisplayOrder;

            _logService.LogDebug("カスタムブラウザ保存開始", nameof(BrowserService));
            await SaveCustomBrowsersAsync().ConfigureAwait(false);
            _logService.LogDebug("ブラウザ更新完了", nameof(BrowserService));
            return true;
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
        {
            _logService.LogError($"ブラウザ更新エラー: {ex.Message}", nameof(BrowserService), ex);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveBrowserAsync(Guid browserId)
    {
        try
        {
            Browser? browser = _browsers.Find(b => b.Id == browserId);
            if (browser == null)
            {
                return false;
            }

            // システム検出ブラウザも削除可能にする
            _ = _browsers.Remove(browser);
            await SaveCustomBrowsersAsync().ConfigureAwait(false);
            return true;
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
        {
            _logService.LogError($"ブラウザ削除エラー: {ex.Message}", nameof(BrowserService), ex);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Browser>> GetAllBrowsersAsync()
    {
        try
        {
            // カスタムブラウザを読み込み
            List<Browser> customBrowsers = await LoadCustomBrowsersAsync().ConfigureAwait(false);

            // システム検出ブラウザとカスタムブラウザをマージ
            _browsers.Clear();

            // システム検出ブラウザを追加
            IEnumerable<Browser> registryBrowsers = await _registryService.DetectBrowsersFromRegistryAsync().ConfigureAwait(false);
            _browsers.AddRange(registryBrowsers);

            // カスタムブラウザを追加
            _browsers.AddRange(customBrowsers);

            _logService.LogDebug($"全ブラウザ取得完了: システム={registryBrowsers.Count()}件, カスタム={customBrowsers.Count}件", nameof(BrowserService));
            return _browsers.OrderBy(b => b.DisplayOrder);
        }
        catch (Exception ex)
        {
            _logService.LogError($"全ブラウザ取得エラー: {ex.Message}", nameof(BrowserService), ex);
            return _browsers.OrderBy(b => b.DisplayOrder);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SetDefaultBrowserAsync(Guid browserId)
    {
        try
        {
            Browser? browser = _browsers.Find(b => b.Id == browserId);
            if (browser == null)
            {
                return false;
            }

            // デフォルトブラウザを設定
            foreach (Browser b in _browsers)
            {
                b.IsDefault = b.Id == browserId;
            }

            await SaveDefaultBrowserAsync().ConfigureAwait(false);
            return true;
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
        {
            _logService.LogError($"デフォルトブラウザ設定エラー: {ex.Message}", nameof(BrowserService), ex);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<Browser?> GetDefaultBrowserAsync()
    {
        try
        {
            Browser? defaultBrowser = _browsers.Find(b => b.IsDefault);
            if (defaultBrowser != null)
            {
                return defaultBrowser;
            }

            // 設定から読み込み
            return await LoadDefaultBrowserAsync().ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
        {
            _logService.LogError($"デフォルトブラウザ取得エラー: {ex.Message}", nameof(BrowserService), ex);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task UpdateUsageAsync(Browser browser)
    {
        try
        {
            await SaveBrowserUsageAsync().ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
        {
            _logService.LogError($"使用統計更新エラー: {ex.Message}", nameof(BrowserService), ex);
        }
    }

    /// <inheritdoc/>
    public async Task UpdateBrowserUsageAsync(Guid browserId)
    {
        try
        {
            Browser? browser = _browsers.Find(b => b.Id == browserId);
            if (browser != null)
            {
                await SaveBrowserUsageAsync().ConfigureAwait(false);
            }
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
        {
            _logService.LogError($"使用統計更新エラー: {ex.Message}", nameof(BrowserService), ex);
        }
    }

    private async Task<List<Browser>> LoadCustomBrowsersAsync()
    {
        try
        {
            if (!File.Exists(_browsersPath))
            {
                _logService.LogDebug("ブラウザデータファイルが存在しません", nameof(BrowserService));
                return new List<Browser>();
            }

            string json = await File.ReadAllTextAsync(_browsersPath).ConfigureAwait(false);
            List<Browser>? browsers = JsonSerializer.Deserialize<List<Browser>>(json);

            if (browsers == null)
            {
                _logService.LogWarning("ブラウザデータのデシリアライズに失敗しました", nameof(BrowserService));
                return new List<Browser>();
            }

            _logService.LogDebug($"ブラウザデータ読み込み完了: {browsers.Count}件", nameof(BrowserService));
            return browsers;
        }
        catch (Exception ex)
        {
            _logService.LogError($"ブラウザデータ読み込みエラー: {ex.Message}", nameof(BrowserService), ex);
            return new List<Browser>();
        }
    }

    private async Task SaveCustomBrowsersAsync()
    {
        try
        {
            _logService.LogDebug($"ブラウザデータ保存開始: {_browsersPath}", nameof(BrowserService));

            // カスタムブラウザのみを保存
            var customBrowsers = _browsers.Where(b => b.Type == BrowserType.Custom).ToList();

            string json = JsonSerializer.Serialize(customBrowsers, GetJsonSerializerOptions());
            await File.WriteAllTextAsync(_browsersPath, json).ConfigureAwait(false);

            _logService.LogDebug($"ブラウザデータ保存完了: {customBrowsers.Count}件", nameof(BrowserService));
        }
        catch (Exception ex)
        {
            _logService.LogError($"ブラウザデータ保存エラー: {ex.Message}", nameof(BrowserService), ex);
            throw;
        }
    }

    private static Task SaveBrowserUsageAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// JSONシリアライザーオプションを取得.
    /// </summary>
    private static JsonSerializerOptions GetJsonSerializerOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }

    private static Task SaveDefaultBrowserAsync()
    {
        return Task.CompletedTask;
    }

    private static Task<Browser?> LoadDefaultBrowserAsync()
    {
        return Task.FromResult<Browser?>(null);
    }

    /// <summary>
    /// ブラウザタイプに応じた起動引数を取得.
    /// </summary>
    private static string GetBrowserArguments(string url)
    {
        // すべてのブラウザタイプで同じURLを返す
        return url;
    }
}
