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
    private readonly List<Browser> _browsers =[];

    // 後方互換: 旧シグネチャ用コンストラクタ（テスト等）
    public BrowserService(IRegistryService registryService, IUrlService urlService)
        : this(registryService, urlService, new LogService())
    {
    }

    public BrowserService(IRegistryService registryService, IUrlService urlService, ILogService logService)
    {
        _registryService = registryService;
        _urlService = urlService;
        _logService = logService;
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
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or SecurityException)
        {
            _logService.LogCritical($"ブラウザ検出で致命的エラーが発生: {ex.Message}", nameof(BrowserService), ex);
            return Enumerable.Empty<Browser>();
        }
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

            // URLを正規化
            string normalizedUrl = await _urlService.NormalizeUrlAsync(url).ConfigureAwait(false);
            _logService.LogDebug($"正規化されたURL - {normalizedUrl}", nameof(BrowserService));

            if (string.IsNullOrEmpty(normalizedUrl))
            {
                _logService.LogWarning("URLの正規化に失敗しました", nameof(BrowserService));
                return false;
            }

            // URLを検証
            bool isValidUrl = await _urlService.ValidateUrlAsync(normalizedUrl).ConfigureAwait(false);
            _logService.LogDebug($"URL検証結果 - {isValidUrl}", nameof(BrowserService));

            if (!isValidUrl)
            {
                _logService.LogWarning($"URLが無効です - {normalizedUrl}", nameof(BrowserService));
                return false;
            }

            // ブラウザタイプに応じた起動引数を設定
            string arguments = GetBrowserArguments(browser.Type, normalizedUrl);

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
                await SaveBrowserUsageAsync(browser).ConfigureAwait(false);
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
            if (_browsers.Any(b => b.ExecutablePath.Equals(browser.ExecutablePath, StringComparison.OrdinalIgnoreCase)))
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
            Browser? existingBrowser = _browsers.Find(b => b.Id == browser.Id);
            if (existingBrowser == null)
            {
                return false;
            }

            // プロパティを更新
            existingBrowser.Name = browser.Name;
            existingBrowser.ExecutablePath = browser.ExecutablePath;
            existingBrowser.DisplayOrder = browser.DisplayOrder;

            await SaveCustomBrowsersAsync().ConfigureAwait(false);
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
            // if (browser.Type != BrowserType.Custom)
            //     return false;
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
    public Task<IEnumerable<Browser>> GetAllBrowsersAsync()
    {
        // 既存のブラウザデータを返すのみ（自動検出は行わない）
        _logService.LogDebug($"Source=Cache Count={_browsers.Count}", nameof(BrowserService));
        return Task.FromResult<IEnumerable<Browser>>(_browsers.OrderBy(b => b.DisplayOrder));
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

            await SaveDefaultBrowserAsync(browser).ConfigureAwait(false);
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
            await SaveBrowserUsageAsync(browser).ConfigureAwait(false);
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
                await SaveBrowserUsageAsync(browser).ConfigureAwait(false);
            }
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
        {
            _logService.LogError($"使用統計更新エラー: {ex.Message}", nameof(BrowserService), ex);
        }
    }

    private Task<List<Browser>> LoadCustomBrowsersAsync()
    {
        // TODO: 設定ファイルからカスタムブラウザを読み込み
        return Task.FromResult(new List<Browser>());
    }

    private Task SaveCustomBrowsersAsync()
    {
        // TODO: カスタムブラウザを設定ファイルに保存
        return Task.CompletedTask;
    }

    private Task SaveBrowserUsageAsync(Browser browser)
    {
        // TODO: ブラウザ使用統計を保存
        return Task.CompletedTask;
    }

    private Task SaveDefaultBrowserAsync(Browser browser)
    {
        // TODO: デフォルトブラウザ設定を保存
        return Task.CompletedTask;
    }

    private Task<Browser?> LoadDefaultBrowserAsync()
    {
        // TODO: 設定ファイルからデフォルトブラウザを読み込み
        return Task.FromResult<Browser?>(null);
    }

    /// <summary>
    /// ブラウザタイプに応じた起動引数を取得.
    /// </summary>
    private string GetBrowserArguments(BrowserType browserType, string url)
    {
        return browserType switch
        {
            BrowserType.Chrome => url,
            BrowserType.Firefox => url,
            BrowserType.Edge => url,
            BrowserType.Opera => url,
            BrowserType.Brave => url,
            BrowserType.Vivaldi => url,
            BrowserType.Custom => url,
            _ => url
        };
    }
}
