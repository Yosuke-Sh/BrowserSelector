using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using System.IO;
using System.Text.Json;

namespace BrowserSelector.IntegrationTests;

/// <summary>
/// テスト用の設定サービス
/// </summary>
public class TestSettingsService : ISettingsService
{
    private readonly string _settingsDirectory;
    private readonly string _appSettingsPath;
    private readonly string _visualSettingsPath;
    private readonly string _logSettingsPath;
    private readonly ILogService? _logService;

    public TestSettingsService(ILogService? logService, string tempDirectory)
    {
        _logService = logService;
        _settingsDirectory = tempDirectory;

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
        _logService?.LogTrace("アプリケーション設定読み込み開始", "TestSettingsService");
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
            var result = settings ?? new AppSettings();
            
            _logService?.LogTrace($"アプリケーション設定読み込み完了: Language={result.Language}, EnableLogging={result.EnableLogging}", "TestSettingsService");
            return result;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"アプリケーション設定の読み込みエラー: {ex.Message}", "TestSettingsService", ex);
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
            _logService?.LogError($"アプリケーション設定の保存エラー: {ex.Message}", "TestSettingsService", ex);
            return false;
        }
    }

    public async Task<VisualSettings> LoadVisualSettingsAsync()
    {
        _logService?.LogTrace("視覚設定読み込み開始", "TestSettingsService");
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
            var result = settings ?? new VisualSettings();
            
            _logService?.LogTrace($"視覚設定読み込み完了: BackgroundColor={result.BackgroundColor}", "TestSettingsService");
            return result;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"視覚設定の読み込みエラー: {ex.Message}", "TestSettingsService", ex);
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
            _logService?.LogError($"視覚設定の保存エラー: {ex.Message}", "TestSettingsService", ex);
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
            _logService?.LogError($"設定リセットエラー: {ex.Message}", "TestSettingsService", ex);
            return false;
        }
    }


    public async Task<LogSettings> LoadLogSettingsAsync()
    {
        _logService?.LogTrace("ログ設定読み込み開始", "TestSettingsService");
        try
        {
            if (!File.Exists(_logSettingsPath))
            {
                var defaultSettings = new LogSettings();
                await SaveLogSettingsAsync(defaultSettings);
                return defaultSettings;
            }

            var json = await File.ReadAllTextAsync(_logSettingsPath);
            var settings = JsonSerializer.Deserialize<LogSettings>(json);
            var result = settings ?? new LogSettings();
            
            _logService?.LogTrace($"ログ設定読み込み完了: LogLevel={result.LogLevel}", "TestSettingsService");
            return result;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"ログ設定の読み込みエラー: {ex.Message}", "TestSettingsService", ex);
            return new LogSettings();
        }
    }

    public async Task<bool> SaveLogSettingsAsync(LogSettings settings)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(settings, options);
            await File.WriteAllTextAsync(_logSettingsPath, json);
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"ログ設定の保存エラー: {ex.Message}", "TestSettingsService", ex);
            return false;
        }
    }

    public async Task<bool> ExportSettingsAsync(string filePath)
    {
        _logService?.LogTrace($"設定エクスポート開始: {filePath}", "TestSettingsService");
        try
        {
            // テスト用の簡単なエクスポート実装
            var exportData = new
            {
                AppSettings = await LoadAppSettingsAsync(),
                VisualSettings = await LoadVisualSettingsAsync(),
                LogSettings = await LoadLogSettingsAsync(),
                ExportDate = DateTime.Now
            };

            var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
            
            _logService?.LogTrace("設定エクスポート完了", "TestSettingsService");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"設定エクスポートエラー: {ex.Message}", "TestSettingsService", ex);
            return false;
        }
    }

    public async Task<bool> ImportSettingsAsync(string filePath)
    {
        _logService?.LogTrace($"設定インポート開始: {filePath}", "TestSettingsService");
        try
        {
            if (!File.Exists(filePath))
            {
                _logService?.LogWarning($"インポートファイルが存在しません: {filePath}", "TestSettingsService");
                return false;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var importData = JsonSerializer.Deserialize<JsonElement>(json);
            
            // 各設定をインポート
            if (importData.TryGetProperty("AppSettings", out var appSettingsElement))
            {
                var appSettings = JsonSerializer.Deserialize<AppSettings>(appSettingsElement.GetRawText());
                if (appSettings != null)
                {
                    await SaveAppSettingsAsync(appSettings);
                }
            }
            
            if (importData.TryGetProperty("VisualSettings", out var visualSettingsElement))
            {
                var visualSettings = JsonSerializer.Deserialize<VisualSettings>(visualSettingsElement.GetRawText());
                if (visualSettings != null)
                {
                    await SaveVisualSettingsAsync(visualSettings);
                }
            }
            
            if (importData.TryGetProperty("LogSettings", out var logSettingsElement))
            {
                var logSettings = JsonSerializer.Deserialize<LogSettings>(logSettingsElement.GetRawText());
                if (logSettings != null)
                {
                    await SaveLogSettingsAsync(logSettings);
                }
            }
            
            _logService?.LogTrace("設定インポート完了", "TestSettingsService");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"設定インポートエラー: {ex.Message}", "TestSettingsService", ex);
            return false;
        }
    }
}
