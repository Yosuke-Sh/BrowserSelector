using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using System.IO;
using System.Text.Json;
using System.IO.Compression;

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
    private readonly ILogService? _logService;

    public SettingsService(ILogService? logService = null)
    {
        _logService = logService;
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
        _logService?.LogTrace("アプリケーション設定読み込み開始", "SettingsService");
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
            
            // Traceレベルで詳細な設定情報を出力
            _logService?.LogTrace($"アプリケーション設定読み込み完了: Language={result.Language}, CloseAfterUrlRuleMatch={result.CloseAfterUrlRuleMatch}", "SettingsService");
            return result;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"アプリケーション設定の読み込みエラー: {ex.Message}", "SettingsService", ex);
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
            _logService?.LogError($"アプリケーション設定の保存エラー: {ex.Message}", "SettingsService", ex);
            return false;
        }
    }

    public async Task<VisualSettings> LoadVisualSettingsAsync()
    {
        _logService?.LogTrace("視覚設定読み込み開始", "SettingsService");
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
            
            // Traceレベルで詳細な設定情報を出力
            _logService?.LogTrace($"視覚設定読み込み完了: BackgroundColor={result.BackgroundColor}, UseBackgroundGradient={result.UseBackgroundGradient}, GradientDirection={result.GradientDirection}, InitialWindowWidth={result.InitialWindowWidth}, InitialWindowHeight={result.InitialWindowHeight}, ShowLogo={result.ShowLogo}, ShowUrlInput={result.ShowUrlInput}, BrowserButtonOpacity={result.BrowserButtonOpacity}", "SettingsService");
            return result;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"視覚設定の読み込みエラー: {ex.Message}", "SettingsService", ex);
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
            _logService?.LogError($"視覚設定の保存エラー: {ex.Message}", "SettingsService", ex);
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
            _logService?.LogError($"設定リセットエラー: {ex.Message}", "SettingsService", ex);
            return false;
        }
    }

    public async Task<bool> ImportSettingsAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return false;

            _logService?.LogInformation("設定ファイル群のインポート開始", "SettingsService");

            // ZIPファイルかどうかを判定
            if (Path.GetExtension(filePath).ToLower() == ".zip")
            {
                return await ImportSettingsFromZipAsync(filePath);
            }
            else
            {
                // 従来のJSONファイル形式（後方互換性）
                return await ImportSettingsFromJsonAsync(filePath);
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"設定インポートエラー: {ex.Message}", "SettingsService", ex);
            return false;
        }
    }

    /// <summary>
    /// ZIPファイルから設定ファイル群をインポート
    /// </summary>
    private async Task<bool> ImportSettingsFromZipAsync(string zipFilePath)
    {
        using var fileStream = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);

        var importedFiles = new List<string>();

        // ZIP内の各ファイルを処理
        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name))
                continue; // ディレクトリエントリをスキップ

            var targetPath = GetTargetPathForEntry(entry.FullName);
            if (targetPath == null)
                continue; // サポートされていないファイルをスキップ

            try
            {
                // ディレクトリを作成
                var targetDirectory = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                // ファイルを展開
                using var entryStream = entry.Open();
                using var targetStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write);
                await entryStream.CopyToAsync(targetStream);

                importedFiles.Add(entry.FullName);
                _logService?.LogDebug($"インポート完了: {entry.FullName} -> {targetPath}", "SettingsService");
            }
            catch (Exception ex)
            {
                _logService?.LogWarning($"ファイルインポートエラー: {entry.FullName} - {ex.Message}", "SettingsService");
            }
        }

        _logService?.LogInformation($"設定ファイル群のインポート完了: {importedFiles.Count}個のファイルをインポート", "SettingsService");
        return importedFiles.Count > 0;
    }

    /// <summary>
    /// エントリ名から対象パスを取得
    /// </summary>
    private string? GetTargetPathForEntry(string entryName)
    {
        return entryName.ToLower() switch
        {
            "appsettings.json" => _appSettingsPath,
            "visualsettings.json" => _visualSettingsPath,
            "logsettings.json" => _logSettingsPath,
            "urlrules.json" => Path.Combine(_settingsDirectory, "urlrules.json"),
            "export-info.json" => null, // エクスポート情報ファイルは無視
            _ when entryName.StartsWith("Languages/", StringComparison.OrdinalIgnoreCase) => 
                Path.Combine(_settingsDirectory, entryName),
            _ => null // サポートされていないファイル
        };
    }

    /// <summary>
    /// 従来のJSONファイル形式からインポート（後方互換性）
    /// </summary>
    private async Task<bool> ImportSettingsFromJsonAsync(string filePath)
    {
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

    public async Task<bool> ExportSettingsAsync(string filePath)
    {
        try
        {
            _logService?.LogInformation("設定ファイル群のエクスポート開始", "SettingsService");
            
            // ZIPファイルとして設定ファイル群をエクスポート
            await ExportSettingsAsZipAsync(filePath);
            
            _logService?.LogInformation($"設定ファイル群のエクスポート完了: {filePath}", "SettingsService");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"設定エクスポートエラー: {ex.Message}", "SettingsService", ex);
            return false;
        }
    }

    /// <summary>
    /// 設定ファイル群をZIP形式でエクスポート
    /// </summary>
    private async Task ExportSettingsAsZipAsync(string zipFilePath)
    {
        using var fileStream = new FileStream(zipFilePath, FileMode.Create);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Create);

        // 設定ファイルをZIPに追加
        await AddFileToZipAsync(archive, _appSettingsPath, "appsettings.json");
        await AddFileToZipAsync(archive, _visualSettingsPath, "visualsettings.json");
        await AddFileToZipAsync(archive, _logSettingsPath, "logsettings.json");
        
        // URLルールファイルを追加
        var urlRulesPath = Path.Combine(_settingsDirectory, "urlrules.json");
        await AddFileToZipAsync(archive, urlRulesPath, "urlrules.json");

        // 言語ファイルを追加
        var languagesPath = Path.Combine(_settingsDirectory, "Languages");
        if (Directory.Exists(languagesPath))
        {
            var languageFiles = Directory.GetFiles(languagesPath, "*.json");
            foreach (var languageFile in languageFiles)
            {
                var fileName = Path.GetFileName(languageFile);
                var zipEntryName = Path.Combine("Languages", fileName);
                await AddFileToZipAsync(archive, languageFile, zipEntryName);
            }
        }

        // エクスポート情報ファイルを追加
        var exportInfo = new
        {
            ExportedAt = DateTime.Now,
            Version = "1.0",
            ExportedBy = "BrowserSelector",
            Description = "BrowserSelector設定ファイル群"
        };

        var exportInfoJson = JsonSerializer.Serialize(exportInfo, new JsonSerializerOptions { WriteIndented = true });
        var exportInfoEntry = archive.CreateEntry("export-info.json");
        using var exportInfoStream = exportInfoEntry.Open();
        using var exportInfoWriter = new StreamWriter(exportInfoStream);
        await exportInfoWriter.WriteAsync(exportInfoJson);
    }

    /// <summary>
    /// ファイルをZIPアーカイブに追加
    /// </summary>
    private async Task AddFileToZipAsync(ZipArchive archive, string filePath, string entryName)
    {
        if (File.Exists(filePath))
        {
            var entry = archive.CreateEntry(entryName);
            using var entryStream = entry.Open();
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            await fileStream.CopyToAsync(entryStream);
            _logService?.LogDebug($"ZIPに追加: {entryName}", "SettingsService");
        }
        else
        {
            _logService?.LogDebug($"ファイルが存在しません（スキップ）: {filePath}", "SettingsService");
        }
    }

    public async Task<LogSettings> LoadLogSettingsAsync()
    {
        _logService?.LogTrace("ログ設定読み込み開始", "SettingsService");
        try
        {
            if (File.Exists(_logSettingsPath))
            {
                var json = await File.ReadAllTextAsync(_logSettingsPath);
                var settings = JsonSerializer.Deserialize<LogSettings>(json);
                var result = settings ?? new LogSettings();
                
                // Traceレベルで詳細な設定情報を出力
                _logService?.LogTrace($"ログ設定読み込み完了: EnableLogging={result.EnableLogging}, LogLevel={result.LogLevel}, EnableConsoleLogging={result.EnableConsoleLogging}, EnableFileLogging={result.EnableFileLogging}, LogOutputFolder={result.LogOutputFolder}", "SettingsService");
                return result;
            }
            else
            {
                var defaultSettings = new LogSettings();
                _logService?.LogTrace("ログ設定ファイルが存在しないため、デフォルト設定を使用", "SettingsService");
                return defaultSettings;
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"ログ設定読み込みエラー: {ex.Message}", "SettingsService", ex);
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
            _logService?.LogError($"ログ設定保存エラー: {ex.Message}", "SettingsService", ex);
            return false;
        }
    }
}
