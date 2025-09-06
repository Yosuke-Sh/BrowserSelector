using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using System.Text.Json;

namespace BrowserSelector.Infrastructure.Services;

/// <summary>
/// カスタム言語ファイル管理サービスの実装
/// </summary>
public class CustomLanguageService : ICustomLanguageService
{
    private readonly string _customLanguageFolder;
    private readonly ILogService? _logService;

    public CustomLanguageService(ILogService? logService = null)
    {
        _logService = logService;
        
        // カスタム言語フォルダのパスを設定
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = System.IO.Path.Combine(appDataPath, "BrowserSelector");
        _customLanguageFolder = System.IO.Path.Combine(appFolder, "Languages");
        
        // フォルダが存在しない場合は作成
        if (!System.IO.Directory.Exists(_customLanguageFolder))
        {
            System.IO.Directory.CreateDirectory(_customLanguageFolder);
            _logService?.LogDebug($"カスタム言語フォルダを作成しました: {_customLanguageFolder}", "CustomLanguageService");
        }
    }

    public async Task<IEnumerable<LanguageInfo>> GetAvailableLanguagesAsync()
    {
        var languages = new List<LanguageInfo>();
        
        try
        {
            // デフォルト言語を追加
            languages.Add(new LanguageInfo("en-US", "English"));
            languages.Add(new LanguageInfo("ja-JP", "日本語"));

            // カスタム言語ファイルを検索
            if (System.IO.Directory.Exists(_customLanguageFolder))
            {
                var languageFiles = System.IO.Directory.GetFiles(_customLanguageFolder, "*.json");
                
                foreach (var filePath in languageFiles)
                {
                    try
                    {
                        var languageFile = await LoadLanguageFileAsync(filePath);
                        if (languageFile != null)
                        {
                            languages.Add(new LanguageInfo(languageFile.CultureCode, languageFile.DisplayName));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logService?.LogWarning($"言語ファイルの読み込みに失敗しました: {filePath} - {ex.Message}", "CustomLanguageService");
                    }
                }
            }

            _logService?.LogDebug($"利用可能な言語数: {languages.Count}", "CustomLanguageService");
        }
        catch (Exception ex)
        {
            _logService?.LogError($"利用可能な言語の取得に失敗しました: {ex.Message}", "CustomLanguageService", ex);
        }

        return languages;
    }

    public async Task<bool> AddCustomLanguageAsync(string languageFilePath)
    {
        try
        {
            if (!System.IO.File.Exists(languageFilePath))
            {
                _logService?.LogWarning($"言語ファイルが存在しません: {languageFilePath}", "CustomLanguageService");
                return false;
            }

            // ファイルの検証
            if (!await ValidateLanguageFileAsync(languageFilePath))
            {
                _logService?.LogWarning($"無効な言語ファイルです: {languageFilePath}", "CustomLanguageService");
                return false;
            }

            // 言語ファイルを読み込み
            var languageFile = await LoadLanguageFileAsync(languageFilePath);
            if (languageFile == null)
            {
                return false;
            }

            // カスタム言語フォルダにコピー
            var fileName = $"{languageFile.CultureCode}.json";
            var targetPath = System.IO.Path.Combine(_customLanguageFolder, fileName);
            
            System.IO.File.Copy(languageFilePath, targetPath, true);
            
            _logService?.LogInformation($"カスタム言語ファイルを追加しました: {languageFile.CultureCode} - {languageFile.DisplayName}", "CustomLanguageService");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"カスタム言語ファイルの追加に失敗しました: {ex.Message}", "CustomLanguageService", ex);
            return false;
        }
    }

    public async Task<bool> RemoveCustomLanguageAsync(string cultureCode)
    {
        try
        {
            var fileName = $"{cultureCode}.json";
            var filePath = System.IO.Path.Combine(_customLanguageFolder, fileName);
            
            if (!System.IO.File.Exists(filePath))
            {
                _logService?.LogWarning($"削除対象の言語ファイルが存在しません: {filePath}", "CustomLanguageService");
                return false;
            }

            System.IO.File.Delete(filePath);
            _logService?.LogInformation($"カスタム言語ファイルを削除しました: {cultureCode}", "CustomLanguageService");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"カスタム言語ファイルの削除に失敗しました: {ex.Message}", "CustomLanguageService", ex);
            return false;
        }
    }

    public async Task<bool> ValidateLanguageFileAsync(string languageFilePath)
    {
        try
        {
            var languageFile = await LoadLanguageFileAsync(languageFilePath);
            if (languageFile == null)
            {
                return false;
            }

            // 必須フィールドの検証
            if (string.IsNullOrWhiteSpace(languageFile.CultureCode))
            {
                _logService?.LogWarning("カルチャーコードが設定されていません", "CustomLanguageService");
                return false;
            }

            if (string.IsNullOrWhiteSpace(languageFile.DisplayName))
            {
                _logService?.LogWarning("表示名が設定されていません", "CustomLanguageService");
                return false;
            }

            if (languageFile.Resources == null || languageFile.Resources.Count == 0)
            {
                _logService?.LogWarning("リソースが設定されていません", "CustomLanguageService");
                return false;
            }

            // カルチャーコードの形式検証
            try
            {
                var culture = new System.Globalization.CultureInfo(languageFile.CultureCode);
            }
            catch
            {
                _logService?.LogWarning($"無効なカルチャーコードです: {languageFile.CultureCode}", "CustomLanguageService");
                return false;
            }

            _logService?.LogDebug($"言語ファイルの検証が完了しました: {languageFile.CultureCode}", "CustomLanguageService");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"言語ファイルの検証に失敗しました: {ex.Message}", "CustomLanguageService", ex);
            return false;
        }
    }

    public string GetCustomLanguageFolder()
    {
        return _customLanguageFolder;
    }

    public async Task<Dictionary<string, string>?> LoadCustomLanguageAsync(string cultureCode)
    {
        try
        {
            var fileName = $"{cultureCode}.json";
            var filePath = System.IO.Path.Combine(_customLanguageFolder, fileName);
            
            if (!System.IO.File.Exists(filePath))
            {
                _logService?.LogDebug($"カスタム言語ファイルが存在しません: {filePath}", "CustomLanguageService");
                return null;
            }

            var languageFile = await LoadLanguageFileAsync(filePath);
            return languageFile?.Resources;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"カスタム言語の読み込みに失敗しました: {ex.Message}", "CustomLanguageService", ex);
            return null;
        }
    }

    public async Task<bool> SaveCustomLanguageAsync(string cultureCode, string displayName, Dictionary<string, string> resources)
    {
        try
        {
            var languageFile = new CustomLanguageFile
            {
                CultureCode = cultureCode,
                DisplayName = displayName,
                Resources = resources,
                UpdatedAt = DateTime.Now
            };

            var fileName = $"{cultureCode}.json";
            var filePath = System.IO.Path.Combine(_customLanguageFolder, fileName);
            
            var json = JsonSerializer.Serialize(languageFile, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            
            await System.IO.File.WriteAllTextAsync(filePath, json);
            
            _logService?.LogInformation($"カスタム言語ファイルを保存しました: {cultureCode} - {displayName}", "CustomLanguageService");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"カスタム言語ファイルの保存に失敗しました: {ex.Message}", "CustomLanguageService", ex);
            return false;
        }
    }

    /// <summary>
    /// 言語ファイルを読み込み
    /// </summary>
    private async Task<CustomLanguageFile?> LoadLanguageFileAsync(string filePath)
    {
        try
        {
            var json = await System.IO.File.ReadAllTextAsync(filePath);
            var languageFile = JsonSerializer.Deserialize<CustomLanguageFile>(json);
            return languageFile;
        }
        catch (Exception ex)
        {
            _logService?.LogError($"言語ファイルの読み込みに失敗しました: {filePath} - {ex.Message}", "CustomLanguageService", ex);
            return null;
        }
    }
}