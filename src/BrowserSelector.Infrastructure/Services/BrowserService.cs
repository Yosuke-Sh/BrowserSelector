using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.SystemIntegration;
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
    private readonly List<Browser> _browsers = new();

    public BrowserService(IRegistryService registryService, IUrlService urlService)
    {
        _registryService = registryService;
        _urlService = urlService;
    }

    public async Task<IEnumerable<Browser>> DetectBrowsersAsync()
    {
        try
        {
            // レジストリからブラウザを検出
            var registryBrowsers = await _registryService.DetectBrowsersFromRegistryAsync();
            _browsers.Clear();
            _browsers.AddRange(registryBrowsers);

            // カスタムブラウザを追加（設定から読み込み）
            var customBrowsers = await LoadCustomBrowsersAsync();
            _browsers.AddRange(customBrowsers);

            return _browsers.OrderBy(b => b.DisplayOrder);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ブラウザ検出エラー: {ex.Message}");
            return Enumerable.Empty<Browser>();
        }
    }

    public async Task<bool> LaunchBrowserAsync(Browser browser, string url)
    {
        try
        {
            Debug.WriteLine($"BrowserService: ブラウザ起動開始 - {browser.Name}, パス: {browser.ExecutablePath}, URL: {url}");
            
            if (!browser.IsValid)
            {
                Debug.WriteLine($"BrowserService: ブラウザが無効です - {browser.Name}");
                return false;
            }
            
            if (!File.Exists(browser.ExecutablePath))
            {
                Debug.WriteLine($"BrowserService: 実行ファイルが存在しません - {browser.ExecutablePath}");
                return false;
            }

            // URLを正規化
            var normalizedUrl = await _urlService.NormalizeUrlAsync(url);
            Debug.WriteLine($"BrowserService: 正規化されたURL - {normalizedUrl}");
            
            if (string.IsNullOrEmpty(normalizedUrl))
            {
                Debug.WriteLine($"BrowserService: URLの正規化に失敗しました");
                return false;
            }

            // URLを検証
            var isValidUrl = await _urlService.ValidateUrlAsync(normalizedUrl);
            Debug.WriteLine($"BrowserService: URL検証結果 - {isValidUrl}");
            
            if (!isValidUrl)
            {
                Debug.WriteLine($"BrowserService: URLが無効です - {normalizedUrl}");
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

            Debug.WriteLine($"BrowserService: プロセス起動 - FileName: {startInfo.FileName}, Arguments: {startInfo.Arguments}");

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                Debug.WriteLine($"BrowserService: プロセス起動成功 - PID: {process.Id}");
                
                // プロセス情報を取得して確認
                try
                {
                    var processInfo = Process.GetProcessById(process.Id);
                    Debug.WriteLine($"BrowserService: 実際に起動されたプロセス - 名前: {processInfo.ProcessName}, ファイル名: {processInfo.MainModule?.FileName}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"BrowserService: プロセス情報取得エラー - {ex.Message}");
                }
                
                // 使用回数を増加
                browser.IncrementUseCount();
                await SaveBrowserUsageAsync(browser);
                return true;
            }

            Debug.WriteLine($"BrowserService: プロセス起動失敗");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"BrowserService: ブラウザ起動エラー - {ex.Message}");
            Debug.WriteLine($"BrowserService: スタックトレース - {ex.StackTrace}");
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
            Debug.WriteLine($"ブラウザ追加エラー: {ex.Message}");
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
            Debug.WriteLine($"ブラウザ更新エラー: {ex.Message}");
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

            // カスタムブラウザのみ削除可能
            if (browser.Type != BrowserType.Custom)
                return false;

            _browsers.Remove(browser);
            await SaveCustomBrowsersAsync();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ブラウザ削除エラー: {ex.Message}");
            return false;
        }
    }

    public async Task<IEnumerable<Browser>> GetAllBrowsersAsync()
    {
        if (!_browsers.Any())
        {
            await DetectBrowsersAsync();
        }

        return _browsers.OrderBy(b => b.DisplayOrder);
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
            Debug.WriteLine($"デフォルトブラウザ設定エラー: {ex.Message}");
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
            Debug.WriteLine($"デフォルトブラウザ取得エラー: {ex.Message}");
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
            Debug.WriteLine($"使用統計更新エラー: {ex.Message}");
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
            Debug.WriteLine($"使用統計更新エラー: {ex.Message}");
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

