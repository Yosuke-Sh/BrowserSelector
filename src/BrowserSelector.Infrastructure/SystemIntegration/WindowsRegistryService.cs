using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using Microsoft.Win32;
using System.IO;

namespace BrowserSelector.Infrastructure.SystemIntegration;

/// <summary>
/// Windowsレジストリからブラウザ情報を取得するサービス.
/// </summary>
public class WindowsRegistryService : IRegistryService
{
    private readonly ILogService? _logService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsRegistryService"/> class.
    /// </summary>
    /// <param name="logService">logService.</param>
    public WindowsRegistryService(ILogService? logService = null)
    {
        _logService = logService;
    }

    /// <inheritdoc/>
    public Task<IEnumerable<Browser>> DetectBrowsersFromRegistryAsync()
    {
        List<Browser> browsers = [];

        try
        {
            // Chrome検出
            browsers.AddRange(DetectChrome());

            // Firefox検出
            browsers.AddRange(DetectFirefox());

            // Edge検出
            browsers.AddRange(DetectEdge());

            // Opera検出
            browsers.AddRange(DetectOpera());

            // Brave検出
            browsers.AddRange(DetectBrave);

            // Vivaldi検出
            browsers.AddRange(DetectVivaldi());
        }
        catch (UnauthorizedAccessException ex)
        {
            _logService?.LogError($"ブラウザ検出エラー（アクセス権限なし）: {ex.Message}", "WindowsRegistryService", ex);
        }
        catch (System.Security.SecurityException ex)
        {
            _logService?.LogError($"ブラウザ検出エラー（セキュリティ例外）: {ex.Message}", "WindowsRegistryService", ex);
        }
        catch (ArgumentException ex)
        {
            _logService?.LogError($"ブラウザ検出エラー（引数例外）: {ex.Message}", "WindowsRegistryService", ex);
        }

        // 重複を除去（同じパスのブラウザは最初に見つかったもののみ保持）
        _logService?.LogDebug($"重複除去前のブラウザ数: {browsers.Count}", "WindowsRegistryService");
        foreach (Browser browser in browsers)
        {
            _logService?.LogDebug($"重複除去前: {browser.Name}, ID: {browser.Id}, パス: {browser.ExecutablePath}", "WindowsRegistryService");
        }

        IOrderedEnumerable<Browser> uniqueBrowsers = browsers
            .Where(b => b.IsValid)
            .GroupBy(b => b.ExecutablePath?.ToUpperInvariant())
            .Select(g => g.First())
            .OrderBy(b => b.DisplayOrder);

        _logService?.LogDebug($"重複除去後のブラウザ数: {uniqueBrowsers.Count()}", "WindowsRegistryService");
        foreach (Browser? browser in uniqueBrowsers)
        {
            _logService?.LogDebug($"重複除去後: {browser.Name}, ID: {browser.Id}, パス: {browser.ExecutablePath}", "WindowsRegistryService");
        }

        return Task.FromResult<IEnumerable<Browser>>(uniqueBrowsers);
    }

    private IEnumerable<Browser> DetectChrome()
    {
        List<Browser> browsers = [];

        try
        {
            // Chrome 64bit
            string chromePath = GetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe", string.Empty);
            _logService?.LogDebug($"Chrome 64bit パス: {chromePath}", "WindowsRegistryService");

            if (!string.IsNullOrEmpty(chromePath) && File.Exists(chromePath))
            {
                Browser chrome64 = new()
                {
                    Name = "Google Chrome",
                    ExecutablePath = chromePath,
                    Type = BrowserType.Chrome,
                    DisplayOrder = 1
                };
                browsers.Add(chrome64);
                _logService?.LogDebug($"Chrome 64bit 追加: {chrome64.Name}, ID: {chrome64.Id}, パス: {chrome64.ExecutablePath}", "WindowsRegistryService");
            }
            else
            {
                _logService?.LogDebug($"Chrome 64bit が見つかりません - パス: {chromePath}, 存在: {File.Exists(chromePath)}", "WindowsRegistryService");
            }

            // Chrome 32bit
            string chromePath32 = GetRegistryValue(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe", string.Empty);
            _logService?.LogDebug($"Chrome 32bit パス: {chromePath32}", "WindowsRegistryService");

            if (!string.IsNullOrEmpty(chromePath32) && File.Exists(chromePath32))
            {
                Browser chrome32 = new()
                {
                    Name = "Google Chrome (32-bit)",
                    ExecutablePath = chromePath32,
                    Type = BrowserType.Chrome,
                    DisplayOrder = 2
                };
                browsers.Add(chrome32);
                _logService?.LogDebug($"Chrome 32bit 追加: {chrome32.Name}, ID: {chrome32.Id}, パス: {chrome32.ExecutablePath}", "WindowsRegistryService");
            }
            else
            {
                _logService?.LogDebug($"Chrome 32bit が見つかりません - パス: {chromePath32}, 存在: {File.Exists(chromePath32)}", "WindowsRegistryService");
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logService?.LogError($"Chrome検出エラー（アクセス権限なし）: {ex.Message}", "WindowsRegistryService", ex);
        }
        catch (System.Security.SecurityException ex)
        {
            _logService?.LogError($"Chrome検出エラー（セキュリティ例外）: {ex.Message}", "WindowsRegistryService", ex);
        }
        catch (ArgumentException ex)
        {
            _logService?.LogError($"Chrome検出エラー（引数例外）: {ex.Message}", "WindowsRegistryService", ex);
        }

        return browsers;
    }

    private IEnumerable<Browser> DetectFirefox()
    {
        List<Browser> browsers = [];

        try
        {
            // Firefox 64bit
            string firefoxPath = GetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\firefox.exe", string.Empty);
            if (!string.IsNullOrEmpty(firefoxPath) && File.Exists(firefoxPath))
            {
                browsers.Add(new Browser
                {
                    Name = "Mozilla Firefox",
                    ExecutablePath = firefoxPath,
                    Type = BrowserType.Firefox,
                    DisplayOrder = 3
                });
            }

            // Firefox 32bit
            string firefoxPath32 = GetRegistryValue(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\App Paths\firefox.exe", string.Empty);
            if (!string.IsNullOrEmpty(firefoxPath32) && File.Exists(firefoxPath32))
            {
                browsers.Add(new Browser
                {
                    Name = "Mozilla Firefox (32-bit)",
                    ExecutablePath = firefoxPath32,
                    Type = BrowserType.Firefox,
                    DisplayOrder = 4
                });
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logService?.LogError($"Firefox検出エラー（アクセス権限なし）: {ex.Message}", "WindowsRegistryService", ex);
        }
        catch (System.Security.SecurityException ex)
        {
            _logService?.LogError($"Firefox検出エラー（セキュリティ例外）: {ex.Message}", "WindowsRegistryService", ex);
        }
        catch (ArgumentException ex)
        {
            _logService?.LogError($"Firefox検出エラー（引数例外）: {ex.Message}", "WindowsRegistryService", ex);
        }

        return browsers;
    }

    private IEnumerable<Browser> DetectEdge()
    {
        List<Browser> browsers = [];

        try
        {
            // Edge Chromium
            string edgePath = GetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\msedge.exe", string.Empty);
            _logService?.LogDebug($"Edge パス: {edgePath}", "WindowsRegistryService");

            if (!string.IsNullOrEmpty(edgePath) && File.Exists(edgePath))
            {
                Browser edge = new()
                {
                    Name = "Microsoft Edge",
                    ExecutablePath = edgePath,
                    Type = BrowserType.Edge,
                    DisplayOrder = 5
                };
                browsers.Add(edge);
                _logService?.LogDebug($"Edge 追加: {edge.Name}, ID: {edge.Id}, パス: {edge.ExecutablePath}", "WindowsRegistryService");
            }
            else
            {
                _logService?.LogDebug($"Edge が見つかりません - パス: {edgePath}, 存在: {File.Exists(edgePath)}", "WindowsRegistryService");
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logService?.LogError($"Edge検出エラー（アクセス権限なし）: {ex.Message}", "WindowsRegistryService", ex);
        }
        catch (System.Security.SecurityException ex)
        {
            _logService?.LogError($"Edge検出エラー（セキュリティ例外）: {ex.Message}", "WindowsRegistryService", ex);
        }
        catch (ArgumentException ex)
        {
            _logService?.LogError($"Edge検出エラー（引数例外）: {ex.Message}", "WindowsRegistryService", ex);
        }

        return browsers;
    }

    private IEnumerable<Browser> DetectOpera()
    {
        List<Browser> browsers = [];

        try
        {
            // Opera
            string operaPath = GetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\opera.exe", string.Empty);
            if (!string.IsNullOrEmpty(operaPath) && File.Exists(operaPath))
            {
                browsers.Add(new Browser
                {
                    Name = "Opera",
                    ExecutablePath = operaPath,
                    Type = BrowserType.Opera,
                    DisplayOrder = 6
                });
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logService?.LogError($"Opera検出エラー（アクセス権限なし）: {ex.Message}", "WindowsRegistryService", ex);
        }
        catch (System.Security.SecurityException ex)
        {
            _logService?.LogError($"Opera検出エラー（セキュリティ例外）: {ex.Message}", "WindowsRegistryService", ex);
        }
        catch (ArgumentException ex)
        {
            _logService?.LogError($"Opera検出エラー（引数例外）: {ex.Message}", "WindowsRegistryService", ex);
        }

        return browsers;
    }

    private IEnumerable<Browser> DetectBrave
    {
        get
        {
            List<Browser> browsers = [];

            try
            {
                // Brave
                string bravePath = GetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\brave.exe", string.Empty);
                if (!string.IsNullOrEmpty(bravePath) && File.Exists(bravePath))
                {
                    browsers.Add(new Browser
                    {
                        Name = "Brave Browser",
                        ExecutablePath = bravePath,
                        Type = BrowserType.Brave,
                        DisplayOrder = 7
                    });
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logService?.LogError($"Brave検出エラー（アクセス権限なし）: {ex.Message}", "WindowsRegistryService", ex);
            }
            catch (System.Security.SecurityException ex)
            {
                _logService?.LogError($"Brave検出エラー（セキュリティ例外）: {ex.Message}", "WindowsRegistryService", ex);
            }
            catch (ArgumentException ex)
            {
                _logService?.LogError($"Brave検出エラー（引数例外）: {ex.Message}", "WindowsRegistryService", ex);
            }

            return browsers;
        }
    }

    private IEnumerable<Browser> DetectVivaldi()
    {
        List<Browser> browsers = [];

        try
        {
            // Vivaldi
            string vivaldiPath = GetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\vivaldi.exe", string.Empty);
            if (!string.IsNullOrEmpty(vivaldiPath) && File.Exists(vivaldiPath))
            {
                browsers.Add(new Browser
                {
                    Name = "Vivaldi",
                    ExecutablePath = vivaldiPath,
                    Type = BrowserType.Vivaldi,
                    DisplayOrder = 8
                });
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logService?.LogError($"Vivaldi検出エラー（アクセス権限なし）: {ex.Message}", "WindowsRegistryService", ex);
        }
        catch (System.Security.SecurityException ex)
        {
            _logService?.LogError($"Vivaldi検出エラー（セキュリティ例外）: {ex.Message}", "WindowsRegistryService", ex);
        }
        catch (ArgumentException ex)
        {
            _logService?.LogError($"Vivaldi検出エラー（引数例外）: {ex.Message}", "WindowsRegistryService", ex);
        }

        return browsers;
    }

    private string GetRegistryValue(string keyPath, string valueName)
    {
        try
        {
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(keyPath);
            if (key != null)
            {
                string? value = key.GetValue(valueName) as string;
                return value ?? string.Empty;
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logService?.LogError($"レジストリ読み取りエラー（アクセス権限なし） {keyPath}: {ex.Message}", "WindowsRegistryService", ex);
        }
        catch (System.Security.SecurityException ex)
        {
            _logService?.LogError($"レジストリ読み取りエラー（セキュリティ例外） {keyPath}: {ex.Message}", "WindowsRegistryService", ex);
        }
        catch (ArgumentException ex)
        {
            _logService?.LogError($"レジストリ読み取りエラー（引数例外） {keyPath}: {ex.Message}", "WindowsRegistryService", ex);
        }

        return string.Empty;
    }
}
