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
    private readonly List<Browser> _browsers = new();

    public BrowserService(IRegistryService registryService)
    {
        _registryService = registryService;
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
            if (!browser.IsValid || !File.Exists(browser.ExecutablePath))
                return false;

            // URLを正規化
            var normalizedUrl = NormalizeUrl(url);
            if (string.IsNullOrEmpty(normalizedUrl))
                return false;

            // ブラウザを起動
            var startInfo = new ProcessStartInfo
            {
                FileName = browser.ExecutablePath,
                Arguments = normalizedUrl,
                UseShellExecute = true
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                // 使用回数を増加
                browser.IncrementUseCount();
                await SaveBrowserUsageAsync(browser);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ブラウザ起動エラー: {ex.Message}");
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

    private string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        // URLの正規化
        url = url.Trim();

        // プロトコルがない場合はhttp://を追加
        if (!url.StartsWith("http://") && !url.StartsWith("https://") && !url.StartsWith("ftp://"))
        {
            url = "http://" + url;
        }

        return url;
    }

    private async Task<List<Browser>> LoadCustomBrowsersAsync()
    {
        // TODO: 設定ファイルからカスタムブラウザを読み込み
        return new List<Browser>();
    }

    private async Task SaveCustomBrowsersAsync()
    {
        // TODO: カスタムブラウザを設定ファイルに保存
    }

    private async Task SaveBrowserUsageAsync(Browser browser)
    {
        // TODO: ブラウザ使用統計を保存
    }

    private async Task SaveDefaultBrowserAsync(Browser browser)
    {
        // TODO: デフォルトブラウザ設定を保存
    }

    private async Task<Browser?> LoadDefaultBrowserAsync()
    {
        // TODO: 設定ファイルからデフォルトブラウザを読み込み
        return null;
    }
}

