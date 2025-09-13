using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using System.Text.Json;

namespace BrowserSelector.UnitTests;

/// <summary>
/// テスト用の設定サービス.
/// </summary>
public class TestSettingsService : ISettingsService
{
    private readonly string _settingsDirectory;
    private readonly string _appSettingsPath;
    private readonly string _visualSettingsPath;
    private readonly string _logSettingsPath;
    private readonly ILogService? _logService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestSettingsService"/> class.
    /// </summary>
    /// <param name="logService"></param>
    /// <param name="tempDirectory"></param>
    public TestSettingsService(ILogService? logService, string tempDirectory)
    {
        _logService = logService;
        _settingsDirectory = tempDirectory;

        // 設定ディレクトリが存在しない場合は作成
        if (!Directory.Exists(_settingsDirectory))
        {
            _ = Directory.CreateDirectory(_settingsDirectory);
        }

        _appSettingsPath = Path.Combine(_settingsDirectory, "appsettings.json");
        _visualSettingsPath = Path.Combine(_settingsDirectory, "visualsettings.json");
        _logSettingsPath = Path.Combine(_settingsDirectory, "logsettings.json");
    }

    /// <inheritdoc/>
    public async Task<AppSettings> LoadAppSettingsAsync()
    {
        _logService?.LogTrace("アプリケーション設定読み込み開始", "TestSettingsService");
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

            _logService?.LogTrace($"アプリケーション設定読み込み完了: Language={result.Language}, EnableLogging={result.EnableLogging}", "TestSettingsService");
            return result;
        }
        catch (FileNotFoundException ex)
        {
            _logService?.LogError($"アプリケーション設定ファイルが見つかりません: {ex.Message}", "TestSettingsService", ex);
            return new AppSettings();
        }
        catch (JsonException ex)
        {
            _logService?.LogError($"アプリケーション設定JSON解析エラー: {ex.Message}", "TestSettingsService", ex);
            return new AppSettings();
        }
        catch (IOException ex)
        {
            _logService?.LogError($"アプリケーション設定ファイルI/Oエラー: {ex.Message}", "TestSettingsService", ex);
            return new AppSettings();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logService?.LogError($"アプリケーション設定ファイルアクセス権限エラー: {ex.Message}", "TestSettingsService", ex);
            throw; // アクセス権限エラーは再スロー
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SaveAppSettingsAsync(AppSettings settings)
    {
        try
        {
            JsonSerializerOptions options = new()
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string json = JsonSerializer.Serialize(settings, options);
            await File.WriteAllTextAsync(_appSettingsPath, json);
            return true;
        }
        catch (JsonException ex)
        {
            _logService?.LogError($"アプリケーション設定JSON保存エラー: {ex.Message}", "TestSettingsService", ex);
            return false;
        }
        catch (IOException ex)
        {
            _logService?.LogError($"アプリケーション設定ファイル保存I/Oエラー: {ex.Message}", "TestSettingsService", ex);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logService?.LogError($"アプリケーション設定ファイル保存アクセス権限エラー: {ex.Message}", "TestSettingsService", ex);
            throw; // アクセス権限エラーは再スロー
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
        catch (FileNotFoundException ex)
        {
            _logService?.LogError($"視覚設定ファイルが見つかりません: {ex.Message}", "TestSettingsService", ex);
            return new VisualSettings();
        }
        catch (JsonException ex)
        {
            _logService?.LogError($"視覚設定JSON解析エラー: {ex.Message}", "TestSettingsService", ex);
            return new VisualSettings();
        }
        catch (IOException ex)
        {
            _logService?.LogError($"視覚設定ファイルI/Oエラー: {ex.Message}", "TestSettingsService", ex);
            return new VisualSettings();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logService?.LogError($"視覚設定ファイルアクセス権限エラー: {ex.Message}", "TestSettingsService", ex);
            throw; // アクセス権限エラーは再スロー
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SaveVisualSettingsAsync(VisualSettings settings)
    {
        try
        {
            JsonSerializerOptions options = new()
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string json = JsonSerializer.Serialize(settings, options);
            await File.WriteAllTextAsync(_visualSettingsPath, json);
            return true;
        }
        catch (JsonException ex)
        {
            _logService?.LogError($"視覚設定JSON保存エラー: {ex.Message}", "TestSettingsService", ex);
            return false;
        }
        catch (IOException ex)
        {
            _logService?.LogError($"視覚設定ファイル保存I/Oエラー: {ex.Message}", "TestSettingsService", ex);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logService?.LogError($"視覚設定ファイル保存アクセス権限エラー: {ex.Message}", "TestSettingsService", ex);
            throw; // アクセス権限エラーは再スロー
        }
    }

    /// <inheritdoc/>
    public string GetSettingsFilePath()
    {
        return _settingsDirectory;
    }

    /// <inheritdoc/>
    public async Task<bool> ResetSettingsAsync()
    {
        try
        {
            // 設定ファイルを削除
            if (File.Exists(_appSettingsPath))
            {
                File.Delete(_appSettingsPath);
            }

            if (File.Exists(_visualSettingsPath))
            {
                File.Delete(_visualSettingsPath);
            }

            // デフォルト設定を作成
            _ = await SaveAppSettingsAsync(new AppSettings());
            _ = await SaveVisualSettingsAsync(new VisualSettings());

            return true;
        }
        catch (IOException ex)
        {
            _logService?.LogError($"設定リセットI/Oエラー: {ex.Message}", "TestSettingsService", ex);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logService?.LogError($"設定リセットアクセス権限エラー: {ex.Message}", "TestSettingsService", ex);
            throw; // アクセス権限エラーは再スロー
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ImportSettingsAsync(string filePath)
    {
        // テスト用の簡易実装
        return await Task.FromResult(false);
    }

    /// <inheritdoc/>
    public async Task<bool> ExportSettingsAsync(string filePath)
    {
        // テスト用の簡易実装
        return await Task.FromResult(false);
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
        catch (FileNotFoundException ex)
        {
            _logService?.LogError($"ログ設定ファイルが見つかりません: {ex.Message}", "TestSettingsService", ex);
            return new LogSettings();
        }
        catch (JsonException ex)
        {
            _logService?.LogError($"ログ設定JSON解析エラー: {ex.Message}", "TestSettingsService", ex);
            return new LogSettings();
        }
        catch (IOException ex)
        {
            _logService?.LogError($"ログ設定ファイルI/Oエラー: {ex.Message}", "TestSettingsService", ex);
            return new LogSettings();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logService?.LogError($"ログ設定ファイルアクセス権限エラー: {ex.Message}", "TestSettingsService", ex);
            throw; // アクセス権限エラーは再スロー
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SaveLogSettingsAsync(LogSettings settings)
    {
        try
        {
            JsonSerializerOptions options = new()
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string json = JsonSerializer.Serialize(settings, options);
            await File.WriteAllTextAsync(_logSettingsPath, json);
            return true;
        }
        catch (JsonException ex)
        {
            _logService?.LogError($"ログ設定JSON保存エラー: {ex.Message}", "TestSettingsService", ex);
            return false;
        }
        catch (IOException ex)
        {
            _logService?.LogError($"ログ設定ファイル保存I/Oエラー: {ex.Message}", "TestSettingsService", ex);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logService?.LogError($"ログ設定ファイル保存アクセス権限エラー: {ex.Message}", "TestSettingsService", ex);
            throw; // アクセス権限エラーは再スロー
        }
    }
}

