using BrowserSelector.Core.Converters;
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace BrowserSelector.Infrastructure.Services;

/// <summary>
/// 設定管理サービスの実装.
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly string _settingsDirectory;
    private readonly string _appSettingsPath;
    private readonly string _visualSettingsPath;
    private readonly string _logSettingsPath;
    private readonly ILogService? _logService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsService"/> class.
    /// </summary>
    /// <param name="logService">logService.</param>
    public SettingsService(ILogService? logService = null)
    {
        _logService = logService;

        // ユーザーのアプリケーションデータフォルダに設定を保存
        _settingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BrowserSelector");

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
        _logService?.LogTrace("アプリケーション設定読み込み開始", "SettingsService");
        try
        {
            if (!File.Exists(_appSettingsPath))
            {
                AppSettings defaultSettings = new();
                _ = await SaveAppSettingsAsync(defaultSettings).ConfigureAwait(false);
                return defaultSettings;
            }

            string json = await File.ReadAllTextAsync(_appSettingsPath).ConfigureAwait(false);
            AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(json);
            AppSettings result = settings ?? new AppSettings();

            // Traceレベルで詳細な設定情報を出力
            _logService?.LogTrace($"アプリケーション設定読み込み完了: Language={result.Language}, CloseAfterUrlRuleMatch={result.CloseAfterUrlRuleMatch}", "SettingsService");
            return result;
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
        {
            _logService?.LogError($"アプリケーション設定の読み込みエラー: {ex.Message}", "SettingsService", ex);
            return new AppSettings();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SaveAppSettingsAsync(AppSettings settings)
    {
        const int maxRetries = 3;
        const int retryDelayMs = 100;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                string json = JsonSerializer.Serialize(settings, GetJsonSerializerOptions());

                // 一時ファイルを使用してアトミックな書き込みを実現
                string tempPath = _appSettingsPath + ".tmp";

                await File.WriteAllTextAsync(tempPath, json).ConfigureAwait(false);

                // アトミックな移動でファイルロックを回避
                if (File.Exists(_appSettingsPath))
                {
                    File.Replace(tempPath, _appSettingsPath, null);
                }
                else
                {
                    File.Move(tempPath, _appSettingsPath);
                }

                _logService?.LogDebug($"アプリケーション設定保存成功 (試行 {attempt})", "SettingsService");
                return true;
            }
            catch (IOException ex) when (attempt < maxRetries)
            {
                _logService?.LogWarning($"アプリケーション設定保存リトライ {attempt}/{maxRetries}: {ex.Message}", "SettingsService");
                await Task.Delay(retryDelayMs * attempt).ConfigureAwait(false);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logService?.LogError($"アプリケーション設定の保存エラー（アクセス権限なし）: {ex.Message}", "SettingsService", ex);
                return false;
            }
            catch (System.Security.SecurityException ex)
            {
                _logService?.LogError($"アプリケーション設定の保存エラー（セキュリティ例外）: {ex.Message}", "SettingsService", ex);
                return false;
            }
            catch (ArgumentException ex)
            {
                _logService?.LogError($"アプリケーション設定の保存エラー（引数例外）: {ex.Message}", "SettingsService", ex);
                return false;
            }
            catch (System.Text.Json.JsonException ex)
            {
                _logService?.LogError($"アプリケーション設定の保存エラー（JSON例外）: {ex.Message}", "SettingsService", ex);
                return false;
            }
        }

        _logService?.LogError($"アプリケーション設定の保存に失敗 (最大試行回数に達しました)", "SettingsService");
        return false;
    }

    /// <inheritdoc/>
    public async Task<VisualSettings> LoadVisualSettingsAsync()
    {
        _logService?.LogTrace("視覚設定読み込み開始", "SettingsService");
        try
        {
            if (!File.Exists(_visualSettingsPath))
            {
                VisualSettings defaultSettings = new();
                _ = await SaveVisualSettingsAsync(defaultSettings).ConfigureAwait(false);
                return defaultSettings;
            }

            string json = await File.ReadAllTextAsync(_visualSettingsPath).ConfigureAwait(false);
            VisualSettings? settings = JsonSerializer.Deserialize<VisualSettings>(json);
            VisualSettings result = settings ?? new VisualSettings();

            // Traceレベルで詳細な設定情報を出力
            _logService?.LogTrace($"視覚設定読み込み完了: BackgroundColor={result.BackgroundColor}, UseBackgroundGradient={result.UseBackgroundGradient}, GradientDirection={result.GradientDirection}, InitialWindowWidth={result.InitialWindowWidth}, InitialWindowHeight={result.InitialWindowHeight}, ShowLogo={result.ShowLogo}, ShowUrlInput={result.ShowUrlInput}, BrowserButtonOpacity={result.BrowserButtonOpacity}", "SettingsService");
            return result;
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
        {
            _logService?.LogError($"視覚設定の読み込みエラー: {ex.Message}", "SettingsService", ex);
            return new VisualSettings();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SaveVisualSettingsAsync(VisualSettings settings)
    {
        const int maxRetries = 3;
        const int retryDelayMs = 100;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                JsonSerializerOptions options = GetJsonSerializerOptions();
                string json = JsonSerializer.Serialize(settings, options);

                // 一時ファイルを使用してアトミックな書き込みを実現
                string tempPath = _visualSettingsPath + ".tmp";

                await File.WriteAllTextAsync(tempPath, json).ConfigureAwait(false);

                // アトミックな移動でファイルロックを回避
                if (File.Exists(_visualSettingsPath))
                {
                    File.Replace(tempPath, _visualSettingsPath, null);
                }
                else
                {
                    File.Move(tempPath, _visualSettingsPath);
                }

                _logService?.LogDebug($"視覚設定保存成功 (試行 {attempt})", "SettingsService");
                return true;
            }
            catch (IOException ex) when (attempt < maxRetries)
            {
                _logService?.LogWarning($"視覚設定保存リトライ {attempt}/{maxRetries}: {ex.Message}", "SettingsService");
                await Task.Delay(retryDelayMs * attempt).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
            {
                _logService?.LogError($"視覚設定の保存エラー (試行 {attempt}): {ex.Message}", "SettingsService", ex);
                return false;
            }
        }

        _logService?.LogError($"視覚設定の保存に失敗 (最大試行回数に達しました)", "SettingsService");
        return false;
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
            _ = await SaveAppSettingsAsync(new AppSettings()).ConfigureAwait(false);
            _ = await SaveVisualSettingsAsync(new VisualSettings()).ConfigureAwait(false);

            return true;
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
        {
            _logService?.LogError($"設定リセットエラー: {ex.Message}", "SettingsService", ex);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ImportSettingsAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            _logService?.LogInformation("設定ファイル群のインポート開始", "SettingsService");

            // ZIPファイルかどうかを判定
            if (string.Equals(Path.GetExtension(filePath), ".zip", StringComparison.OrdinalIgnoreCase))
            {
                return await ImportSettingsFromZipAsync(filePath).ConfigureAwait(false);
            }
            else
            {
                // 従来のJSONファイル形式（後方互換性）
                return await ImportSettingsFromJsonAsync(filePath).ConfigureAwait(false);
            }
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException or InvalidDataException)
        {
            _logService?.LogError($"設定インポートエラー: {ex.Message}", "SettingsService", ex);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ExportSettingsAsync(string filePath)
    {
        try
        {
            _logService?.LogInformation("設定ファイル群のエクスポート開始", "SettingsService");

            // ZIPファイルとして設定ファイル群をエクスポート
            await ExportSettingsAsZipAsync(filePath).ConfigureAwait(false);

            _logService?.LogInformation($"設定ファイル群のエクスポート完了: {filePath}", "SettingsService");
            return true;
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
        {
            _logService?.LogError($"設定エクスポートエラー: {ex.Message}", "SettingsService", ex);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<LogSettings> LoadLogSettingsAsync()
    {
        _logService?.LogTrace("ログ設定読み込み開始", "SettingsService");
        try
        {
            if (File.Exists(_logSettingsPath))
            {
                string json = await File.ReadAllTextAsync(_logSettingsPath).ConfigureAwait(false);
                LogSettings? settings = JsonSerializer.Deserialize<LogSettings>(json);
                LogSettings result = settings ?? new LogSettings();

                // Traceレベルで詳細な設定情報を出力
                _logService?.LogTrace($"ログ設定読み込み完了: EnableLogging={result.EnableLogging}, LogLevel={result.LogLevel}, EnableConsoleLogging={result.EnableConsoleLogging}, EnableFileLogging={result.EnableFileLogging}, LogOutputFolder={result.LogOutputFolder}", "SettingsService");
                return result;
            }
            else
            {
                LogSettings defaultSettings = new();
                _logService?.LogTrace("ログ設定ファイルが存在しないため、デフォルト設定を使用", "SettingsService");
                return defaultSettings;
            }
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
        {
            _logService?.LogError($"ログ設定読み込みエラー: {ex.Message}", "SettingsService", ex);
            return new LogSettings();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SaveLogSettingsAsync(LogSettings settings)
    {
        try
        {
            string json = JsonSerializer.Serialize(settings, GetJsonSerializerOptions());
            await File.WriteAllTextAsync(_logSettingsPath, json).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
        {
            _logService?.LogError($"ログ設定保存エラー: {ex.Message}", "SettingsService", ex);
            return false;
        }
    }

    /// <summary>
    /// JSONシリアライザーオプションを取得.
    /// </summary>
    private static JsonSerializerOptions GetJsonSerializerOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new ColorJsonConverter() }
        };
    }

    /// <summary>
    /// ZIPファイルから設定ファイル群をインポート.
    /// </summary>
    private async Task<bool> ImportSettingsFromZipAsync(string zipFilePath)
    {
        using FileStream fileStream = new(zipFilePath, FileMode.Open, FileAccess.Read);
        using ZipArchive archive = new(fileStream, ZipArchiveMode.Read);

        List<string> importedFiles = [];

        // ZIP内の各ファイルを処理
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name))
            {
                continue; // ディレクトリエントリをスキップ
            }

            string? targetPath = GetTargetPathForEntry(entry.FullName);
            if (targetPath == null)
            {
                continue; // サポートされていないファイルをスキップ
            }

            try
            {
                // ディレクトリを作成
                string? targetDirectory = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
                {
                    _ = Directory.CreateDirectory(targetDirectory);
                }

                // ファイルを展開
                using Stream entryStream = entry.Open();
                using FileStream targetStream = new(targetPath, FileMode.Create, FileAccess.Write);
                await entryStream.CopyToAsync(targetStream).ConfigureAwait(false);

                importedFiles.Add(entry.FullName);
                _logService?.LogDebug($"インポート完了: {entry.FullName} -> {targetPath}", "SettingsService");
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
            {
                _logService?.LogWarning($"ファイルインポートエラー: {entry.FullName} - {ex.Message}", "SettingsService");
            }
        }

        _logService?.LogInformation($"設定ファイル群のインポート完了: {importedFiles.Count}個のファイルをインポート", "SettingsService");
        return importedFiles.Count > 0;
    }

    /// <summary>
    /// エントリ名から対象パスを取得.
    /// </summary>
    private string? GetTargetPathForEntry(string entryName)
    {
        return entryName switch
        {
            var name when string.Equals(name, "appsettings.json", StringComparison.OrdinalIgnoreCase) => _appSettingsPath,
            var name when string.Equals(name, "visualsettings.json", StringComparison.OrdinalIgnoreCase) => _visualSettingsPath,
            var name when string.Equals(name, "logsettings.json", StringComparison.OrdinalIgnoreCase) => _logSettingsPath,
            var name when string.Equals(name, "urlrules.json", StringComparison.OrdinalIgnoreCase) => Path.Combine(_settingsDirectory, "urlrules.json"),
            var name when string.Equals(name, "export-info.json", StringComparison.OrdinalIgnoreCase) => null, // エクスポート情報ファイルは無視
            _ when entryName.StartsWith("Languages/", StringComparison.OrdinalIgnoreCase) =>
                Path.Combine(_settingsDirectory, entryName),
            _ => null // サポートされていないファイル
        };
    }

    /// <summary>
    /// 従来のJSONファイル形式からインポート（後方互換性）.
    /// </summary>
    private async Task<bool> ImportSettingsFromJsonAsync(string filePath)
    {
        string json = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
        Dictionary<string, object>? importedData = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

        if (importedData == null)
        {
            return false;
        }

        // 設定の種類を判定してインポート
        if (importedData.ContainsKey("StartMinimized") || importedData.ContainsKey("Language"))
        {
            // アプリケーション設定
            AppSettings? appSettings = JsonSerializer.Deserialize<AppSettings>(json);
            if (appSettings != null)
            {
                _ = await SaveAppSettingsAsync(appSettings).ConfigureAwait(false);
            }
        }

        if (importedData.ContainsKey("Opacity") || importedData.ContainsKey("BackgroundColor"))
        {
            // 視覚設定
            VisualSettings? visualSettings = JsonSerializer.Deserialize<VisualSettings>(json);
            if (visualSettings != null)
            {
                _ = await SaveVisualSettingsAsync(visualSettings).ConfigureAwait(false);
            }
        }

        return true;
    }

    /// <summary>
    /// 設定ファイル群をZIP形式でエクスポート.
    /// </summary>
    private async Task ExportSettingsAsZipAsync(string zipFilePath)
    {
        using FileStream fileStream = new(zipFilePath, FileMode.Create);
        using ZipArchive archive = new(fileStream, ZipArchiveMode.Create);

        // 設定ファイルをZIPに追加
        await AddFileToZipAsync(archive, _appSettingsPath, "appsettings.json").ConfigureAwait(false);
        await AddFileToZipAsync(archive, _visualSettingsPath, "visualsettings.json").ConfigureAwait(false);
        await AddFileToZipAsync(archive, _logSettingsPath, "logsettings.json").ConfigureAwait(false);

        // URLルールファイルを追加
        string urlRulesPath = Path.Combine(_settingsDirectory, "urlrules.json");
        await AddFileToZipAsync(archive, urlRulesPath, "urlrules.json").ConfigureAwait(false);

        // 言語ファイルを追加
        string languagesPath = Path.Combine(_settingsDirectory, "Languages");
        if (Directory.Exists(languagesPath))
        {
            string[] languageFiles = Directory.GetFiles(languagesPath, "*.json");
            foreach (string languageFile in languageFiles)
            {
                string fileName = Path.GetFileName(languageFile);
                string zipEntryName = Path.Combine("Languages", fileName);
                await AddFileToZipAsync(archive, languageFile, zipEntryName).ConfigureAwait(false);
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

        string exportInfoJson = JsonSerializer.Serialize(exportInfo, GetJsonSerializerOptions());
        ZipArchiveEntry exportInfoEntry = archive.CreateEntry("export-info.json");
        using Stream exportInfoStream = exportInfoEntry.Open();
        using StreamWriter exportInfoWriter = new(exportInfoStream);
        await exportInfoWriter.WriteAsync(exportInfoJson).ConfigureAwait(false);
    }

    /// <summary>
    /// ファイルをZIPアーカイブに追加.
    /// </summary>
    private async Task AddFileToZipAsync(ZipArchive archive, string filePath, string entryName)
    {
        if (File.Exists(filePath))
        {
            ZipArchiveEntry entry = archive.CreateEntry(entryName);
            using Stream entryStream = entry.Open();
            using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read);
            await fileStream.CopyToAsync(entryStream).ConfigureAwait(false);
            _logService?.LogDebug($"ZIPに追加: {entryName}", "SettingsService");
        }
        else
        {
            _logService?.LogDebug($"ファイルが存在しません（スキップ）: {filePath}", "SettingsService");
        }
    }
}
