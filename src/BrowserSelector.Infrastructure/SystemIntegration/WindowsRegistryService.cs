using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using Microsoft.Win32;
using System.IO;

namespace BrowserSelector.Infrastructure.SystemIntegration;

/// <summary>
/// Windowsレジストリからブラウザ情報を取得するサービス
/// </summary>
public class WindowsRegistryService : IRegistryService
{
    private readonly ILogService? _logService;

    public WindowsRegistryService(ILogService? logService = null)
    {
        _logService = logService;
    }
    public Task<IEnumerable<Browser>> DetectBrowsersFromRegistryAsync()
    {
        var browsers = new List<Browser>();

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
            browsers.AddRange(DetectBrave());

            // Vivaldi検出
            browsers.AddRange(DetectVivaldi());
        }
        catch (Exception ex)
        {
            _logService?.LogError($"ブラウザ検出エラー: {ex.Message}", "WindowsRegistryService", ex);
        }

        // 重複を除去（同じパスのブラウザは最初に見つかったもののみ保持）
        _logService?.LogDebug($"重複除去前のブラウザ数: {browsers.Count}", "WindowsRegistryService");
        foreach (var browser in browsers)
        {
            _logService?.LogDebug($"重複除去前: {browser.Name}, ID: {browser.Id}, パス: {browser.ExecutablePath}", "WindowsRegistryService");
        }

        var uniqueBrowsers = browsers
            .Where(b => b.IsValid)
            .GroupBy(b => b.ExecutablePath?.ToLowerInvariant())
            .Select(g => g.First())
            .OrderBy(b => b.DisplayOrder);

        _logService?.LogDebug($"重複除去後のブラウザ数: {uniqueBrowsers.Count()}", "WindowsRegistryService");
        foreach (var browser in uniqueBrowsers)
        {
            _logService?.LogDebug($"重複除去後: {browser.Name}, ID: {browser.Id}, パス: {browser.ExecutablePath}", "WindowsRegistryService");
        }

        return Task.FromResult<IEnumerable<Browser>>(uniqueBrowsers);
    }

    private IEnumerable<Browser> DetectChrome()
    {
        var browsers = new List<Browser>();

        try
        {
            // Chrome 64bit
            var chromePath = GetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe", "");
            _logService?.LogDebug($"Chrome 64bit パス: {chromePath}", "WindowsRegistryService");

            if (!string.IsNullOrEmpty(chromePath) && File.Exists(chromePath))
            {
                var chrome64 = new Browser
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
            var chromePath32 = GetRegistryValue(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe", "");
            _logService?.LogDebug($"Chrome 32bit パス: {chromePath32}", "WindowsRegistryService");

            if (!string.IsNullOrEmpty(chromePath32) && File.Exists(chromePath32))
            {
                var chrome32 = new Browser
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
        catch (Exception ex)
        {
            _logService?.LogError($"Chrome検出エラー: {ex.Message}", "WindowsRegistryService", ex);
        }

        return browsers;
    }

    private IEnumerable<Browser> DetectFirefox()
    {
        var browsers = new List<Browser>();

        try
        {
            // Firefox 64bit
            var firefoxPath = GetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\firefox.exe", "");
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
            var firefoxPath32 = GetRegistryValue(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\App Paths\firefox.exe", "");
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
        catch (Exception ex)
        {
            _logService?.LogError($"Firefox検出エラー: {ex.Message}", "WindowsRegistryService", ex);
        }

        return browsers;
    }

    private IEnumerable<Browser> DetectEdge()
    {
        var browsers = new List<Browser>();

        try
        {
            // Edge Chromium
            var edgePath = GetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\msedge.exe", "");
            _logService?.LogDebug($"Edge パス: {edgePath}", "WindowsRegistryService");

            if (!string.IsNullOrEmpty(edgePath) && File.Exists(edgePath))
            {
                var edge = new Browser
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
        catch (Exception ex)
        {
            _logService?.LogError($"Edge検出エラー: {ex.Message}", "WindowsRegistryService", ex);
        }

        return browsers;
    }

    private IEnumerable<Browser> DetectOpera()
    {
        var browsers = new List<Browser>();

        try
        {
            // Opera
            var operaPath = GetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\opera.exe", "");
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
        catch (Exception ex)
        {
            _logService?.LogError($"Opera検出エラー: {ex.Message}", "WindowsRegistryService", ex);
        }

        return browsers;
    }

    private IEnumerable<Browser> DetectBrave()
    {
        var browsers = new List<Browser>();

        try
        {
            // Brave
            var bravePath = GetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\brave.exe", "");
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
        catch (Exception ex)
        {
            _logService?.LogError($"Brave検出エラー: {ex.Message}", "WindowsRegistryService", ex);
        }

        return browsers;
    }

    private IEnumerable<Browser> DetectVivaldi()
    {
        var browsers = new List<Browser>();

        try
        {
            // Vivaldi
            var vivaldiPath = GetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\vivaldi.exe", "");
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
        catch (Exception ex)
        {
            _logService?.LogError($"Vivaldi検出エラー: {ex.Message}", "WindowsRegistryService", ex);
        }

        return browsers;
    }

    private string GetRegistryValue(string keyPath, string valueName)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(keyPath);
            if (key != null)
            {
                var value = key.GetValue(valueName) as string;
                return value ?? string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"レジストリ読み取りエラー {keyPath}: {ex.Message}", "WindowsRegistryService", ex);
        }

        return string.Empty;
    }
}

