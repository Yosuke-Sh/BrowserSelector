using Microsoft.Win32;
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using System.IO;

namespace BrowserSelector.Infrastructure.SystemIntegration;

/// <summary>
/// Windowsレジストリからブラウザ情報を取得するサービス
/// </summary>
public class WindowsRegistryService : IRegistryService
{
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
            // ログ出力（後で実装）
            System.Diagnostics.Debug.WriteLine($"ブラウザ検出エラー: {ex.Message}");
        }

        return Task.FromResult<IEnumerable<Browser>>(browsers.Where(b => b.IsValid).OrderBy(b => b.DisplayOrder));
    }

    private IEnumerable<Browser> DetectChrome()
    {
        var browsers = new List<Browser>();
        
        try
        {
            // Chrome 64bit
            var chromePath = GetRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe", "");
            if (!string.IsNullOrEmpty(chromePath) && File.Exists(chromePath))
            {
                browsers.Add(new Browser
                {
                    Name = "Google Chrome",
                    ExecutablePath = chromePath,
                    Type = BrowserType.Chrome,
                    DisplayOrder = 1
                });
            }

            // Chrome 32bit
            var chromePath32 = GetRegistryValue(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe", "");
            if (!string.IsNullOrEmpty(chromePath32) && File.Exists(chromePath32))
            {
                browsers.Add(new Browser
                {
                    Name = "Google Chrome (32-bit)",
                    ExecutablePath = chromePath32,
                    Type = BrowserType.Chrome,
                    DisplayOrder = 2
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Chrome検出エラー: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine($"Firefox検出エラー: {ex.Message}");
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
            if (!string.IsNullOrEmpty(edgePath) && File.Exists(edgePath))
            {
                browsers.Add(new Browser
                {
                    Name = "Microsoft Edge",
                    ExecutablePath = edgePath,
                    Type = BrowserType.Edge,
                    DisplayOrder = 5
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Edge検出エラー: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine($"Opera検出エラー: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine($"Brave検出エラー: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine($"Vivaldi検出エラー: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine($"レジストリ読み取りエラー {keyPath}: {ex.Message}");
        }

        return string.Empty;
    }
}

