using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.Logging;
using System.Diagnostics;
using System.IO;

namespace BrowserSelector.Infrastructure.Services;

/// <summary>
/// ブラウザ管理サービスの実装
/// </summary>
public class BrowserService : IBrowserService
{
    private readonly IRegistryService _registryService;
    private readonly IUrlService _urlService;
    private readonly ILogService _logService;
    private readonly List<Browser> _browsers = new();

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

    public async Task<IEnumerable<Browser>> DetectBrowsersAsync()
    {
        try
        {
            // レジストリからブラウザを検出
            var registryBrowsers = await _registryService.DetectBrowsersFromRegistryAsync();
            _browsers.Clear();

            // 自動検出されたブラウザにアイコンを設定
            foreach (var browser in registryBrowsers)
            {
                if (string.IsNullOrEmpty(browser.IconPath) && !string.IsNullOrEmpty(browser.ExecutablePath))
                {
                    browser.IconPath = browser.ExecutablePath; // 実行ファイルからアイコンを抽出
                }
            }

            _browsers.AddRange(registryBrowsers);

            // カスタムブラウザを追加（設定から読み込み）
            var customBrowsers = await LoadCustomBrowsersAsync();
            _browsers.AddRange(customBrowsers);

            return _browsers.OrderBy(b => b.DisplayOrder);
        }
        catch (Exception ex)
        {
            _logService.LogError($"ブラウザ検出エラー: {ex.Message}", nameof(BrowserService), ex);
            return Enumerable.Empty<Browser>();
        }
    }

    public async Task<bool> LaunchBrowserAsync(Browser browser, string url)
    {
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
            var normalizedUrl = await _urlService.NormalizeUrlAsync(url);
            _logService.LogDebug($"正規化されたURL - {normalizedUrl}", nameof(BrowserService));

            if (string.IsNullOrEmpty(normalizedUrl))
            {
                _logService.LogWarning("URLの正規化に失敗しました", nameof(BrowserService));
                return false;
            }

            // URLを検証
            var isValidUrl = await _urlService.ValidateUrlAsync(normalizedUrl);
            _logService.LogDebug($"URL検証結果 - {isValidUrl}", nameof(BrowserService));

            if (!isValidUrl)
            {
                _logService.LogWarning($"URLが無効です - {normalizedUrl}", nameof(BrowserService));
                return false;
            }

            // ブラウザタイプに応じた起動引数を設定
            string arguments = GetBrowserArguments(browser.Type, normalizedUrl);

            // ブラウザを起動
            var startInfo = new ProcessStartInfo
            {
                FileName = browser.ExecutablePath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = false
            };

            _logService.LogDebug($"プロセス起動 - FileName: {startInfo.FileName}, Arguments: {startInfo.Arguments}", nameof(BrowserService));

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                _logService.LogInformation($"プロセス起動成功 - PID: {process.Id}", nameof(BrowserService));

                // プロセス情報を取得して確認
                try
                {
                    var processInfo = Process.GetProcessById(process.Id);
                    _logService.LogDebug($"実際に起動されたプロセス - 名前: {processInfo.ProcessName}, ファイル名: {processInfo.MainModule?.FileName}", nameof(BrowserService));
                }
                catch (Exception ex)
                {
                    _logService.LogDebug($"プロセス情報取得エラー - {ex.Message}", nameof(BrowserService), ex);
                }

                // 使用回数を増加
                browser.IncrementUseCount();
                await SaveBrowserUsageAsync(browser);
                return true;
            }

            _logService.LogError("プロセス起動失敗", nameof(BrowserService));
            return false;
        }
        catch (Exception ex)
        {
            _logService.LogError($"ブラウザ起動エラー - {ex.Message}", nameof(BrowserService), ex);
            return false;
        }
    }

    public async Task<bool> AddBrowserAsync(Browser browser)
    {
        try
        {
            if (!browser.IsValid)
                return false;

            // 重複チェック
            if (_browsers.Any(b => b.ExecutablePath.Equals(browser.ExecutablePath, StringComparison.OrdinalIgnoreCase)))
                return false;

            browser.Type = BrowserType.Custom;
            browser.DisplayOrder = _browsers.Count + 1;
            _browsers.Add(browser);

            await SaveCustomBrowsersAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logService.LogError($"ブラウザ追加エラー: {ex.Message}", nameof(BrowserService), ex);
            return false;
        }
    }

    public async Task<bool> UpdateBrowserAsync(Browser browser)
    {
        try
        {
            var existingBrowser = _browsers.FirstOrDefault(b => b.Id == browser.Id);
            if (existingBrowser == null)
                return false;

            // プロパティを更新
            existingBrowser.Name = browser.Name;
            existingBrowser.ExecutablePath = browser.ExecutablePath;
            existingBrowser.DisplayOrder = browser.DisplayOrder;

            await SaveCustomBrowsersAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logService.LogError($"ブラウザ更新エラー: {ex.Message}", nameof(BrowserService), ex);
            return false;
        }
    }

    public async Task<bool> RemoveBrowserAsync(Guid browserId)
    {
        try
        {
            var browser = _browsers.FirstOrDefault(b => b.Id == browserId);
            if (browser == null)
                return false;

            // システム検出ブラウザも削除可能にする
            // if (browser.Type != BrowserType.Custom)
            //     return false;

            _browsers.Remove(browser);
            await SaveCustomBrowsersAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logService.LogError($"ブラウザ削除エラー: {ex.Message}", nameof(BrowserService), ex);
            return false;
        }
    }

    public Task<IEnumerable<Browser>> GetAllBrowsersAsync()
    {
        // 既存のブラウザデータを返すのみ（自動検出は行わない）
        _logService.LogDebug($"Source=Cache Count={_browsers.Count}", nameof(BrowserService));
        return Task.FromResult<IEnumerable<Browser>>(_browsers.OrderBy(b => b.DisplayOrder));
    }

    public async Task<bool> SetDefaultBrowserAsync(Guid browserId)
    {
        try
        {
            var browser = _browsers.FirstOrDefault(b => b.Id == browserId);
            if (browser == null)
                return false;

            // デフォルトブラウザを設定
            foreach (var b in _browsers)
            {
                b.IsDefault = b.Id == browserId;
            }

            await SaveDefaultBrowserAsync(browser);
            return true;
        }
        catch (Exception ex)
        {
            _logService.LogError($"デフォルトブラウザ設定エラー: {ex.Message}", nameof(BrowserService), ex);
            return false;
        }
    }

    public async Task<Browser?> GetDefaultBrowserAsync()
    {
        try
        {
            var defaultBrowser = _browsers.FirstOrDefault(b => b.IsDefault);
            if (defaultBrowser != null)
                return defaultBrowser;

            // 設定から読み込み
            return await LoadDefaultBrowserAsync();
        }
        catch (Exception ex)
        {
            _logService.LogError($"デフォルトブラウザ取得エラー: {ex.Message}", nameof(BrowserService), ex);
            return null;
        }
    }

    public async Task UpdateUsageAsync(Browser browser)
    {
        try
        {
            await SaveBrowserUsageAsync(browser);
        }
        catch (Exception ex)
        {
            _logService.LogError($"使用統計更新エラー: {ex.Message}", nameof(BrowserService), ex);
        }
    }

    public async Task UpdateBrowserUsageAsync(Guid browserId)
    {
        try
        {
            var browser = _browsers.FirstOrDefault(b => b.Id == browserId);
            if (browser != null)
            {
                await SaveBrowserUsageAsync(browser);
            }
        }
        catch (Exception ex)
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
    /// ブラウザタイプに応じた起動引数を取得
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

