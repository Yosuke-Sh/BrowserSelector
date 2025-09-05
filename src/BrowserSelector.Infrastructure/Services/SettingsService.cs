using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using System.IO;
using System.Text.Json;

namespace BrowserSelector.Infrastructure.Services;

/// <summary>
/// 設定管理サービスの実装
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly string _settingsDirectory;
    private readonly string _appSettingsPath;
    private readonly string _visualSettingsPath;
    private readonly string _logSettingsPath;

    public SettingsService()
    {
        // ポータブルモードかどうかを判定
        var executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var executableDirectory = Path.GetDirectoryName(executablePath) ?? Environment.CurrentDirectory;
        var portableMarkerPath = Path.Combine(executableDirectory, "portable.txt");

        if (File.Exists(portableMarkerPath))
        {
            // ポータブルモード：実行ファイルと同じディレクトリに設定を保存
            _settingsDirectory = executableDirectory;
        }
        else
        {
            // 通常モード：ユーザーのアプリケーションデータフォルダに保存
            _settingsDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BrowserSelector");
        }

        // 設定ディレクトリが存在しない場合は作成
        if (!Directory.Exists(_settingsDirectory))
        {
            Directory.CreateDirectory(_settingsDirectory);
        }

        _appSettingsPath = Path.Combine(_settingsDirectory, "appsettings.json");
        _visualSettingsPath = Path.Combine(_settingsDirectory, "visualsettings.json");
        _logSettingsPath = Path.Combine(_settingsDirectory, "logsettings.json");
    }

    public async Task<AppSettings> LoadAppSettingsAsync()
    {
        try
        {
            if (!File.Exists(_appSettingsPath))
            {
                var defaultSettings = new AppSettings();
                await SaveAppSettingsAsync(defaultSettings);
                return defaultSettings;
            }

            var json = await File.ReadAllTextAsync(_appSettingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);
            return settings ?? new AppSettings();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"アプリケーション設定の読み込みエラー: {ex.Message}");
            return new AppSettings();
        }
    }

    public async Task<bool> SaveAppSettingsAsync(AppSettings settings)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(settings, options);
            await File.WriteAllTextAsync(_appSettingsPath, json);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"アプリケーション設定の保存エラー: {ex.Message}");
            return false;
        }
    }

    public async Task<VisualSettings> LoadVisualSettingsAsync()
    {
        try
        {
            if (!File.Exists(_visualSettingsPath))
            {
                var defaultSettings = new VisualSettings();
                await SaveVisualSettingsAsync(defaultSettings);
                return defaultSettings;
            }

            var json = await File.ReadAllTextAsync(_visualSettingsPath);
            var settings = JsonSerializer.Deserialize<VisualSettings>(json);
            return settings ?? new VisualSettings();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"視覚設定の読み込みエラー: {ex.Message}");
            return new VisualSettings();
        }
    }

    public async Task<bool> SaveVisualSettingsAsync(VisualSettings settings)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(settings, options);
            await File.WriteAllTextAsync(_visualSettingsPath, json);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"視覚設定の保存エラー: {ex.Message}");
            return false;
        }
    }

    public string GetSettingsFilePath()
    {
        return _settingsDirectory;
    }

    public async Task<bool> ResetSettingsAsync()
    {
        try
        {
            // 設定ファイルを削除
            if (File.Exists(_appSettingsPath))
                File.Delete(_appSettingsPath);

            if (File.Exists(_visualSettingsPath))
                File.Delete(_visualSettingsPath);

            // デフォルト設定を作成
            await SaveAppSettingsAsync(new AppSettings());
            await SaveVisualSettingsAsync(new VisualSettings());

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"設定リセットエラー: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ImportSettingsAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return false;

            var json = await File.ReadAllTextAsync(filePath);
            var importedData = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            if (importedData == null)
                return false;

            // 設定の種類を判定してインポート
            if (importedData.ContainsKey("StartMinimized") || importedData.ContainsKey("Language"))
            {
                // アプリケーション設定
                var appSettings = JsonSerializer.Deserialize<AppSettings>(json);
                if (appSettings != null)
                {
                    await SaveAppSettingsAsync(appSettings);
                }
            }

            if (importedData.ContainsKey("Opacity") || importedData.ContainsKey("BackgroundColor"))
            {
                // 視覚設定
                var visualSettings = JsonSerializer.Deserialize<VisualSettings>(json);
                if (visualSettings != null)
                {
                    await SaveVisualSettingsAsync(visualSettings);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"設定インポートエラー: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ExportSettingsAsync(string filePath)
    {
        try
        {
            var appSettings = await LoadAppSettingsAsync();
            var visualSettings = await LoadVisualSettingsAsync();

            var exportData = new
            {
                AppSettings = appSettings,
                VisualSettings = visualSettings,
                ExportedAt = DateTime.Now,
                Version = "1.0"
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(exportData, options);
            await File.WriteAllTextAsync(filePath, json);

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"設定エクスポートエラー: {ex.Message}");
            return false;
        }
    }

    public async Task<LogSettings> LoadLogSettingsAsync()
    {
        try
        {
            if (File.Exists(_logSettingsPath))
            {
                var json = await File.ReadAllTextAsync(_logSettingsPath);
                var settings = JsonSerializer.Deserialize<LogSettings>(json);
                return settings ?? new LogSettings();
            }
            else
            {
                return new LogSettings();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ログ設定読み込みエラー: {ex.Message}");
            return new LogSettings();
        }
    }

    public async Task<bool> SaveLogSettingsAsync(LogSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(_logSettingsPath, json);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ログ設定保存エラー: {ex.Message}");
            return false;
        }
    }
}
