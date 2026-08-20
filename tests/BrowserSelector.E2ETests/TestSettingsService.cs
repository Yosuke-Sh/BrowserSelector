using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using System.Text.Json;

namespace BrowserSelector.E2ETests;

/// <summary>
/// テスト用の設定サービス実装
/// 実際のファイルシステムではなく、テスト用の一時ディレクトリを使用.
/// </summary>
internal sealed class TestSettingsService : ISettingsService
{
    private readonly ILogService? _logService;
    private readonly string _settingsDirectory;
    private readonly string _appSettingsPath;
    private readonly string _visualSettingsPath;
    private readonly string _logSettingsPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestSettingsService"/> class.
    /// </summary>
    /// <param name="logService"></param>
    /// <param name="tempDirectory"></param>
    public TestSettingsService(ILogService? logService, string tempDirectory)
    {
        _logService = logService;
        _settingsDirectory = tempDirectory;
        _appSettingsPath = Path.Combine(_settingsDirectory, "appsettings.json");
        _visualSettingsPath = Path.Combine(_settingsDirectory, "visualsettings.json");
        _logSettingsPath = Path.Combine(_settingsDirectory, "logsettings.json");
    }

    /// <inheritdoc/>
    public async Task<AppSettings> LoadAppSettingsAsync()
    {
        _logService?.LogTrace("アプリ設定読み込み開始", "TestSettingsService");
        try
        {
            if (!File.Exists(_appSettingsPath))
            {
                AppSettings defaultSettings = new();
                _ = await SaveAppSettingsAsync(defaultSettings);
                return defaultSettings;
            }

            string json = await File.ReadAllTextAsync(_appSettingsPath);
            AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(json);
            AppSettings result = settings ?? new AppSettings();

            _logService?.LogTrace($"アプリ設定読み込み完了: Language={result.Language}", "TestSettingsService");
            return result;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"アプリ設定の読み込みエラー: {ex.Message}", "TestSettingsService", ex);
            return new AppSettings();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SaveAppSettingsAsync(AppSettings settings)
    {
        _logService?.LogTrace($"アプリ設定保存開始: Language={settings.Language}", "TestSettingsService");
        try
        {
            _ = Directory.CreateDirectory(_settingsDirectory);
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            await File.WriteAllTextAsync(_appSettingsPath, json);

            _logService?.LogTrace("アプリ設定保存完了", "TestSettingsService");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"アプリ設定の保存エラー: {ex.Message}", "TestSettingsService", ex);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<VisualSettings> LoadVisualSettingsAsync()
    {
        _logService?.LogTrace("視覚設定読み込み開始", "TestSettingsService");
        try
        {
            if (!File.Exists(_visualSettingsPath))
            {
                VisualSettings defaultSettings = new();
                _ = await SaveVisualSettingsAsync(defaultSettings);
                return defaultSettings;
            }

            string json = await File.ReadAllTextAsync(_visualSettingsPath);
            VisualSettings? settings = JsonSerializer.Deserialize<VisualSettings>(json);
            VisualSettings result = settings ?? new VisualSettings();

            _logService?.LogTrace($"視覚設定読み込み完了: BackgroundColor={result.BackgroundColor}", "TestSettingsService");
            return result;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"視覚設定の読み込みエラー: {ex.Message}", "TestSettingsService", ex);
            return new VisualSettings();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SaveVisualSettingsAsync(VisualSettings settings)
    {
        _logService?.LogTrace($"視覚設定保存開始: BackgroundColor={settings.BackgroundColor}", "TestSettingsService");
        try
        {
            _ = Directory.CreateDirectory(_settingsDirectory);
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            await File.WriteAllTextAsync(_visualSettingsPath, json);

            _logService?.LogTrace("視覚設定保存完了", "TestSettingsService");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"視覚設定の保存エラー: {ex.Message}", "TestSettingsService", ex);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<LogSettings> LoadLogSettingsAsync()
    {
        _logService?.LogTrace("ログ設定読み込み開始", "TestSettingsService");
        try
        {
            if (!File.Exists(_logSettingsPath))
            {
                LogSettings defaultSettings = new();
                _ = await SaveLogSettingsAsync(defaultSettings);
                return defaultSettings;
            }

            string json = await File.ReadAllTextAsync(_logSettingsPath);
            LogSettings? settings = JsonSerializer.Deserialize<LogSettings>(json);
            LogSettings result = settings ?? new LogSettings();

            _logService?.LogTrace($"ログ設定読み込み完了: LogLevel={result.LogLevel}", "TestSettingsService");
            return result;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"ログ設定の読み込みエラー: {ex.Message}", "TestSettingsService", ex);
            return new LogSettings();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SaveLogSettingsAsync(LogSettings settings)
    {
        _logService?.LogTrace($"ログ設定保存開始: LogLevel={settings.LogLevel}", "TestSettingsService");
        try
        {
            _ = Directory.CreateDirectory(_settingsDirectory);
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            await File.WriteAllTextAsync(_logSettingsPath, json);

            _logService?.LogTrace("ログ設定保存完了", "TestSettingsService");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"ログ設定の保存エラー: {ex.Message}", "TestSettingsService", ex);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ResetSettingsAsync()
    {
        _logService?.LogTrace("設定リセット開始", "TestSettingsService");
        try
        {
            // デフォルト設定を保存
            _ = await SaveAppSettingsAsync(new AppSettings());
            _ = await SaveVisualSettingsAsync(new VisualSettings());
            _ = await SaveLogSettingsAsync(new LogSettings());

            _logService?.LogTrace("設定リセット完了", "TestSettingsService");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"設定リセットエラー: {ex.Message}", "TestSettingsService", ex);
            return false;
        }
    }

    /// <inheritdoc/>
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

            string json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
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

    /// <inheritdoc/>
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

            string json = await File.ReadAllTextAsync(filePath);
            JsonElement importData = JsonSerializer.Deserialize<JsonElement>(json);

            // 各設定をインポート
            if (importData.TryGetProperty("AppSettings", out JsonElement appSettingsElement))
            {
                AppSettings? appSettings = JsonSerializer.Deserialize<AppSettings>(appSettingsElement.GetRawText());
                if (appSettings != null)
                {
                    _ = await SaveAppSettingsAsync(appSettings);
                }
            }

            if (importData.TryGetProperty("VisualSettings", out JsonElement visualSettingsElement))
            {
                VisualSettings? visualSettings = JsonSerializer.Deserialize<VisualSettings>(visualSettingsElement.GetRawText());
                if (visualSettings != null)
                {
                    _ = await SaveVisualSettingsAsync(visualSettings);
                }
            }

            if (importData.TryGetProperty("LogSettings", out JsonElement logSettingsElement))
            {
                LogSettings? logSettings = JsonSerializer.Deserialize<LogSettings>(logSettingsElement.GetRawText());
                if (logSettings != null)
                {
                    _ = await SaveLogSettingsAsync(logSettings);
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

    /// <inheritdoc/>
    public string GetSettingsFilePath()
    {
        return _settingsDirectory;
    }
}
