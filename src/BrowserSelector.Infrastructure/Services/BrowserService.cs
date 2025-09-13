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
    // フィールド（SA1201: メソッドより前に配置）
    private readonly IRegistryService _registryService;
    private readonly IUrlService _urlService;
    private readonly ILogService _logService;
    private readonly List<Browser> _browsers = [];
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
        string settingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BrowserSelector");

        // 設定ディレクトリが存在しない場合は作成
        if (!Directory.Exists(settingsDirectory))
        {
            _ = Directory.CreateDirectory(settingsDirectory);
        }

        _browsersPath = Path.Combine(settingsDirectory, "browsers.json");
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Browser>> DetectBrowsersAsync()
    {
        _logService.LogTrace("ブラウザ検出処理開始", "BrowserService");
        try
        {
            // レジストリからブラウザを検出
            IEnumerable<Browser> detectedBrowsers = await _registryService.DetectBrowsersFromRegistryAsync().ConfigureAwait(false);

            // 検出されたブラウザにアイコンを設定
            foreach (Browser browser in detectedBrowsers)
            {
                if (string.IsNullOrEmpty(browser.IconPath) && !string.IsNullOrEmpty(browser.ExecutablePath))
                {
                    browser.IconPath = browser.ExecutablePath; // 実行ファイルからアイコンを抽出
                }
            }

            // 検出結果を設定ファイルに保存
            List<Browser> browsersList = detectedBrowsers.ToList();
            string json = JsonSerializer.Serialize(browsersList, GetJsonSerializerOptions());
            await File.WriteAllTextAsync(_browsersPath, json).ConfigureAwait(false);

            _browsers.Clear();
            _browsers.AddRange(browsersList);

            // Traceレベルで詳細なブラウザ情報を出力
            string browserDetails = string.Join(", ", _browsers.Select(b => $"{b.Name}(Enabled:{b.IsEnabled})"));
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

            browser.DisplayOrder = _browsers.Count + 1;
            _browsers.Add(browser);

            await SaveBrowsersToFileAsync().ConfigureAwait(false);
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

            // 最新のブラウザリストを取得
            IEnumerable<Browser> allBrowsers = await GetAllBrowsersAsync().ConfigureAwait(false);
            List<Browser> currentBrowsers = allBrowsers.ToList();
            _logService.LogDebug($"現在のブラウザ数: {currentBrowsers.Count}", nameof(BrowserService));

            Browser? existingBrowser = currentBrowsers.Find(b => b.Id == browser.Id);
            if (existingBrowser == null)
            {
                _logService.LogWarning($"更新対象のブラウザが見つかりません: ID={browser.Id}, Name={browser.Name}", nameof(BrowserService));
                _logService.LogDebug($"利用可能なブラウザID: {string.Join(", ", currentBrowsers.Select(b => b.Id))}", nameof(BrowserService));
                return false;
            }

            // プロパティを更新
            _logService.LogDebug($"ブラウザプロパティ更新: IconPath={browser.IconPath}, IconIndex={browser.IconIndex}, Arguments={browser.Arguments}", nameof(BrowserService));
            existingBrowser.Name = browser.Name;
            existingBrowser.ExecutablePath = browser.ExecutablePath;
            existingBrowser.IconPath = browser.IconPath;
            existingBrowser.IconIndex = browser.IconIndex;
            existingBrowser.Arguments = browser.Arguments;
            existingBrowser.IsEnabled = browser.IsEnabled;
            existingBrowser.DisplayOrder = browser.DisplayOrder;

            // _browsersコレクションも更新
            Browser? browserInCollection = _browsers.Find(b => b.Id == browser.Id);
            if (browserInCollection != null)
            {
                browserInCollection.Name = browser.Name;
                browserInCollection.ExecutablePath = browser.ExecutablePath;
                browserInCollection.IconPath = browser.IconPath;
                browserInCollection.IconIndex = browser.IconIndex;
                browserInCollection.Arguments = browser.Arguments;
                browserInCollection.IsEnabled = browser.IsEnabled;
                browserInCollection.DisplayOrder = browser.DisplayOrder;
            }

            _logService.LogDebug("カスタムブラウザ保存開始", nameof(BrowserService));
            await SaveBrowsersToFileAsync().ConfigureAwait(false);
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
            await SaveBrowsersToFileAsync().ConfigureAwait(false);
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
            // 設定ファイルからブラウザを読み込み
            List<Browser> browsers = await LoadBrowsersFromFileAsync().ConfigureAwait(false);

            // 設定ファイルが空の場合のみ、システム検出を実行して保存
            if (browsers.Count == 0)
            {
                _logService.LogInformation("設定ファイルが空のため、システムブラウザを検出して保存します", nameof(BrowserService));
                IEnumerable<Browser> detectedBrowsers = await _registryService.DetectBrowsersFromRegistryAsync().ConfigureAwait(false);
                if (detectedBrowsers.Any())
                {
                    browsers = detectedBrowsers.ToList();
                    string json = JsonSerializer.Serialize(browsers, GetJsonSerializerOptions());
                    await File.WriteAllTextAsync(_browsersPath, json).ConfigureAwait(false);
                    _logService.LogInformation($"システムブラウザ検出完了: {browsers.Count}件を設定ファイルに保存", nameof(BrowserService));
                }
            }

            _browsers.Clear();
            _browsers.AddRange(browsers);

            _logService.LogDebug($"ブラウザ読み込み完了: {browsers.Count}件", nameof(BrowserService));
            return _browsers.OrderBy(b => b.DisplayOrder);
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
        {
            _logService.LogError($"ブラウザ読み込みエラー: {ex.Message}", nameof(BrowserService), ex);
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

    private static Task SaveBrowserUsageAsync()
    {
        return Task.CompletedTask;
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

    private async Task<List<Browser>> LoadBrowsersFromFileAsync()
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
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
        {
            _logService.LogError($"ブラウザデータ読み込みエラー: {ex.Message}", nameof(BrowserService), ex);
            return new List<Browser>();
        }
    }

    private async Task SaveBrowsersToFileAsync()
    {
        try
        {
            _logService.LogDebug($"ブラウザデータ保存開始: {_browsersPath}", nameof(BrowserService));

            // すべてのブラウザを保存（設定ファイルベースの管理）
            var browsersToSave = _browsers.ToList();

            string json = JsonSerializer.Serialize(browsersToSave, GetJsonSerializerOptions());
            await File.WriteAllTextAsync(_browsersPath, json).ConfigureAwait(false);

            _logService.LogDebug($"ブラウザデータ保存完了: {browsersToSave.Count}件", nameof(BrowserService));
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
        {
            _logService.LogError($"ブラウザデータ保存エラー: {ex.Message}", nameof(BrowserService), ex);
            throw;
        }
    }
}
