using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Text.Json;

namespace BrowserSelector.Infrastructure.Services;

/// <summary>
/// カスタム言語ファイル管理サービスの実装.
/// </summary>
public class CustomLanguageService : ICustomLanguageService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly string _customLanguageFolder;
    private readonly ILogService? _logService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomLanguageService"/> class.
    /// </summary>
    /// <param name="logService">logService.</param>
    public CustomLanguageService(ILogService? logService = null)
    {
        _logService = logService;

        // カスタム言語フォルダのパスを設定
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string appFolder = System.IO.Path.Combine(appDataPath, "BrowserSelector");
        _customLanguageFolder = System.IO.Path.Combine(appFolder, "Languages");

        // フォルダが存在しない場合は作成
        if (!System.IO.Directory.Exists(_customLanguageFolder))
        {
            _ = System.IO.Directory.CreateDirectory(_customLanguageFolder);
            _logService?.LogDebug($"カスタム言語フォルダを作成しました: {_customLanguageFolder}", "CustomLanguageService");
        }

        // 初期起動時にデフォルト言語ファイルを配置
        _ = Task.Run(async () => await EnsureDefaultLanguageFilesAsync().ConfigureAwait(false));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<LanguageInfo>> GetAvailableLanguagesAsync()
    {
        List<LanguageInfo> languages = [];

        try
        {
            // 言語ファイルから読み込み（デフォルト言語も含む）
            if (System.IO.Directory.Exists(_customLanguageFolder))
            {
                string[] languageFiles = System.IO.Directory.GetFiles(_customLanguageFolder, "*.json");

                foreach (string filePath in languageFiles)
                {
                    try
                    {
                        CustomLanguageFile? languageFile = await LoadLanguageFileAsync(filePath).ConfigureAwait(false);
                        if (languageFile != null)
                        {
                            // 表示名はローカライズ不要（英語はEnglish、日本語は日本語）
                            string displayName = GetLocalizedDisplayName(languageFile.CultureCode, languageFile.DisplayName);
                            languages.Add(new LanguageInfo(languageFile.CultureCode, displayName));
                        }
                    }
                    catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
                    {
                        _logService?.LogWarning($"言語ファイルの読み込みに失敗しました: {filePath} - {ex.Message}", "CustomLanguageService");
                    }
                }
            }

            // 言語ファイルが存在しない場合はデフォルト言語を追加
            if (languages.Count == 0)
            {
                languages.Add(new LanguageInfo("en-US", "English"));
                languages.Add(new LanguageInfo("ja-JP", "日本語"));
            }

            _logService?.LogDebug($"利用可能な言語数: {languages.Count}", "CustomLanguageService");
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException)
        {
            _logService?.LogError($"利用可能な言語の取得に失敗しました: {ex.Message}", "CustomLanguageService", ex);
        }

        return languages;
    }

    /// <inheritdoc/>
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
            if (!await ValidateLanguageFileAsync(languageFilePath).ConfigureAwait(false))
            {
                _logService?.LogWarning($"無効な言語ファイルです: {languageFilePath}", "CustomLanguageService");
                return false;
            }

            // 言語ファイルを読み込み
            CustomLanguageFile? languageFile = await LoadLanguageFileAsync(languageFilePath).ConfigureAwait(false);
            if (languageFile == null)
            {
                return false;
            }

            // カスタム言語フォルダにコピー
            string fileName = $"{languageFile.CultureCode}.json";
            string targetPath = System.IO.Path.Combine(_customLanguageFolder, fileName);

            System.IO.File.Copy(languageFilePath, targetPath, true);

            _logService?.LogInformation($"カスタム言語ファイルを追加しました: {languageFile.CultureCode} - {languageFile.DisplayName}", "CustomLanguageService");
            return true;
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
        {
            _logService?.LogError($"カスタム言語ファイルの追加に失敗しました: {ex.Message}", "CustomLanguageService", ex);
            return false;
        }
    }

    /// <inheritdoc/>
    public Task<bool> RemoveCustomLanguageAsync(string cultureCode)
    {
        try
        {
            string fileName = $"{cultureCode}.json";
            string filePath = System.IO.Path.Combine(_customLanguageFolder, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                _logService?.LogWarning($"削除対象の言語ファイルが存在しません: {filePath}", "CustomLanguageService");
                return Task.FromResult(false);
            }

            System.IO.File.Delete(filePath);
            _logService?.LogInformation($"カスタム言語ファイルを削除しました: {cultureCode}", "CustomLanguageService");
            return Task.FromResult(true);
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or UnauthorizedAccessException or IOException)
        {
            _logService?.LogError($"カスタム言語ファイルの削除に失敗しました: {ex.Message}", "CustomLanguageService", ex);
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateLanguageFileAsync(string languageFilePath)
    {
        try
        {
            CustomLanguageFile? languageFile = await LoadLanguageFileAsync(languageFilePath).ConfigureAwait(false);
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
                _ = new CultureInfo(languageFile.CultureCode);
            }
            catch (Exception ex) when (ex is ArgumentException or CultureNotFoundException)
            {
                _logService?.LogWarning($"無効なカルチャーコードです: {languageFile.CultureCode} - {ex.Message}", "CustomLanguageService");
                return false;
            }

            _logService?.LogDebug($"言語ファイルの検証が完了しました: {languageFile.CultureCode}", "CustomLanguageService");
            return true;
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
        {
            _logService?.LogError($"言語ファイルの検証に失敗しました: {ex.Message}", "CustomLanguageService", ex);
            return false;
        }
    }

    /// <inheritdoc/>
    public string GetCustomLanguageFolder()
    {
        return _customLanguageFolder;
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, string>?> LoadCustomLanguageAsync(string cultureCode)
    {
        try
        {
            string fileName = $"{cultureCode}.json";
            string filePath = System.IO.Path.Combine(_customLanguageFolder, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                _logService?.LogDebug($"カスタム言語ファイルが存在しません: {filePath}", "CustomLanguageService");
                return null;
            }

            CustomLanguageFile? languageFile = await LoadLanguageFileAsync(filePath).ConfigureAwait(false);
            return languageFile?.Resources;
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
        {
            _logService?.LogError($"カスタム言語の読み込みに失敗しました: {ex.Message}", "CustomLanguageService", ex);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SaveCustomLanguageAsync(string cultureCode, string displayName, Dictionary<string, string> resources)
    {
        try
        {
            CustomLanguageFile languageFile = new()
            {
                CultureCode = cultureCode,
                DisplayName = displayName,
                Resources = resources,
                UpdatedAt = DateTime.Now
            };

            string fileName = $"{cultureCode}.json";
            string filePath = System.IO.Path.Combine(_customLanguageFolder, fileName);

            string json = JsonSerializer.Serialize(languageFile, JsonOptions);

            await System.IO.File.WriteAllTextAsync(filePath, json).ConfigureAwait(false);

            _logService?.LogInformation($"カスタム言語ファイルを保存しました: {cultureCode} - {displayName}", "CustomLanguageService");
            return true;
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
        {
            _logService?.LogError($"カスタム言語ファイルの保存に失敗しました: {ex.Message}", "CustomLanguageService", ex);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> GenerateLanguageTemplateAsync(string cultureCode, string displayName)
    {
        try
        {
            // 既存のリソースキーを取得
            IEnumerable<string> resourceKeys = await GetAvailableResourceKeysAsync().ConfigureAwait(false);

            // テンプレート用のリソース辞書を作成（英語のデフォルト値を埋め込み）
            Dictionary<string, string> templateResources = [];

            // 英語リソースを取得してデフォルト値として使用
            ResourceManager englishResourceManager = new("BrowserSelector.Infrastructure.Localization.Resources", typeof(CustomLanguageService).Assembly);
            CultureInfo englishCulture = new("en-US");

            foreach (string key in resourceKeys)
            {
                // 英語のデフォルト値を取得
                string? englishValue = englishResourceManager.GetString(key, englishCulture);
                if (!string.IsNullOrEmpty(englishValue))
                {
                    templateResources[key] = englishValue; // 英語のデフォルト値を埋め込み
                }
                else
                {
                    templateResources[key] = $"[{key}]"; // フォールバック
                }
            }

            // テンプレートファイルを作成
            CustomLanguageFile languageFile = new()
            {
                CultureCode = cultureCode,
                DisplayName = displayName,
                Resources = templateResources,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Version = "1.0",
                Description = $"BrowserSelector {displayName} Language Template",
                Author = "User Generated"
            };

            string fileName = $"{cultureCode}.json";
            string filePath = System.IO.Path.Combine(_customLanguageFolder, fileName);

            string json = JsonSerializer.Serialize(languageFile, JsonOptions);

            await System.IO.File.WriteAllTextAsync(filePath, json).ConfigureAwait(false);

            _logService?.LogInformation($"言語ファイルテンプレートを生成しました: {cultureCode} - {displayName}", "CustomLanguageService");
            return true;
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
        {
            _logService?.LogError($"言語ファイルテンプレートの生成に失敗しました: {ex.Message}", "CustomLanguageService", ex);
            return false;
        }
    }

    /// <inheritdoc/>
    public Task<IEnumerable<string>> GetAvailableResourceKeysAsync()
    {
        try
        {
            // デフォルトの英語リソースからキーを取得
            ResourceManager resourceManager = new("BrowserSelector.Infrastructure.Localization.Resources", typeof(CustomLanguageService).Assembly);
            CultureInfo englishCulture = new("en-US");

            List<string> resourceKeys = [];

            // リソースファイルからキーを抽出（リフレクションを使用）
            ResourceSet? resourceSet = resourceManager.GetResourceSet(englishCulture, true, true);
            if (resourceSet != null)
            {
                System.Collections.IDictionaryEnumerator enumerator = resourceSet.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (enumerator.Key is string key)
                    {
                        resourceKeys.Add(key);
                    }
                }
            }

            _logService?.LogDebug($"利用可能なリソースキー数: {resourceKeys.Count}", "CustomLanguageService");
            return Task.FromResult<IEnumerable<string>>(resourceKeys.OrderBy(k => k));
        }
        catch (Exception ex) when (ex is MissingManifestResourceException or ArgumentException)
        {
            _logService?.LogError($"リソースキーの取得に失敗しました: {ex.Message}", "CustomLanguageService", ex);
            return Task.FromResult<IEnumerable<string>>([]);
        }
    }

    /// <summary>
    /// 言語選択コンボボックス用の表示名を取得（ローカライズ不要）.
    /// </summary>
    private static string GetLocalizedDisplayName(string cultureCode, string originalDisplayName)
    {
        // 言語選択コンボボックスはローカライズ不要
        // 英語は「English」、日本語は「日本語」と表示
        return cultureCode switch
        {
            "en-US" => "English",
            "ja-JP" => "日本語",
            _ => originalDisplayName // その他の言語は元の表示名を使用
        };
    }

    /// <summary>
    /// デフォルト言語ファイルが存在しない場合に配置する（高速版）.
    /// </summary>
    private async Task EnsureDefaultLanguageFilesAsync()
    {
        try
        {
            // 高速同期処理
            await SyncLanguageFilesFastAsync().ConfigureAwait(false);
            _logService?.LogDebug("言語ファイルの高速同期完了", "CustomLanguageService");
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException)
        {
            _logService?.LogError($"言語ファイルの同期に失敗しました: {ex.Message}", "CustomLanguageService", ex);
        }
    }

    /// <summary>
    /// 言語ファイルの高速同期処理.
    /// </summary>
    private async Task SyncLanguageFilesFastAsync()
    {
        try
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string[] defaultLanguages = new[] { "en-US", "ja-JP" };

            // 並列処理で高速化
            IEnumerable<Task> tasks = defaultLanguages.Select(async cultureCode =>
            {
                try
                {
                    string targetPath = System.IO.Path.Combine(_customLanguageFolder, $"{cultureCode}.json");

                    // ファイルが存在しない場合は即座にコピー
                    if (!System.IO.File.Exists(targetPath))
                    {
                        await CopyEmbeddedLanguageFileAsync(cultureCode, targetPath).ConfigureAwait(false);
                        _logService?.LogDebug($"言語ファイルを新規配置: {cultureCode}", "CustomLanguageService");
                        return;
                    }

                    // ファイルが存在する場合は軽量チェック
                    if (await ShouldUpdateLanguageFileAsync(assembly, cultureCode, targetPath).ConfigureAwait(false))
                    {
                        await CopyEmbeddedLanguageFileAsync(cultureCode, targetPath).ConfigureAwait(false);
                        _logService?.LogDebug($"言語ファイルを更新: {cultureCode}", "CustomLanguageService");
                    }
                    else
                    {
                        _logService?.LogDebug($"言語ファイルは最新: {cultureCode}", "CustomLanguageService");
                    }
                }
                catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException)
                {
                    _logService?.LogError($"言語ファイル同期エラー ({cultureCode}): {ex.Message}", "CustomLanguageService", ex);
                }
            });

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException)
        {
            _logService?.LogError($"高速同期処理エラー: {ex.Message}", "CustomLanguageService", ex);
        }
    }

    /// <summary>
    /// 言語ファイルの更新が必要かどうかを軽量チェック.
    /// </summary>
    private Task<bool> ShouldUpdateLanguageFileAsync(System.Reflection.Assembly assembly, string cultureCode, string targetPath)
    {
        try
        {
            // 埋め込みリソースのサイズを取得
            string resourceName = $"BrowserSelector.Infrastructure.Localization.{cultureCode}.json";
            using System.IO.Stream? embeddedStream = assembly.GetManifestResourceStream(resourceName);
            if (embeddedStream == null)
            {
                _logService?.LogWarning($"埋め込みリソースが見つかりません: {resourceName}", "CustomLanguageService");
                return Task.FromResult(false);
            }

            long embeddedSize = embeddedStream.Length;

            // 既存ファイルのサイズを取得
            System.IO.FileInfo fileInfo = new(targetPath);
            if (!fileInfo.Exists)
            {
                return Task.FromResult(true); // ファイルが存在しない場合は更新が必要
            }

            long existingSize = fileInfo.Length;

            // サイズが異なる場合は更新が必要
            if (embeddedSize != existingSize)
            {
                _logService?.LogDebug($"ファイルサイズが異なります: {cultureCode} (埋め込み: {embeddedSize}, 既存: {existingSize})", "CustomLanguageService");
                return Task.FromResult(true);
            }

            // サイズが同じでも、より詳細なチェックが必要な場合はここで実装
            // 現在はサイズ比較のみで高速化

            // オプション: ハッシュ値による詳細チェック（必要に応じて有効化）
            return Task.FromResult(false);
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or UnauthorizedAccessException or IOException)
        {
            _logService?.LogWarning($"更新チェックエラー ({cultureCode}): {ex.Message}", "CustomLanguageService");
            return Task.FromResult(false); // エラーの場合は更新しない
        }
    }

    /// <summary>
    /// 埋め込みリソースから言語ファイルをコピー.
    /// </summary>
    private async Task CopyEmbeddedLanguageFileAsync(string cultureCode, string targetPath)
    {
        try
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string resourceName = $"BrowserSelector.Infrastructure.Localization.{cultureCode}.json";

            using System.IO.Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                _logService?.LogWarning($"埋め込みリソースが見つかりません: {resourceName}", "CustomLanguageService");
                return;
            }

            using System.IO.FileStream fileStream = new(targetPath, System.IO.FileMode.Create);
            await stream.CopyToAsync(fileStream).ConfigureAwait(false);

            _logService?.LogDebug($"言語ファイルを配置しました: {targetPath}", "CustomLanguageService");
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or IOException)
        {
            _logService?.LogError($"言語ファイルのコピーに失敗しました: {cultureCode} -> {targetPath} - {ex.Message}", "CustomLanguageService", ex);
        }
    }

    /// <summary>
    /// 言語ファイルを読み込み.
    /// </summary>
    private async Task<CustomLanguageFile?> LoadLanguageFileAsync(string filePath)
    {
        try
        {
            string json = await System.IO.File.ReadAllTextAsync(filePath).ConfigureAwait(false);
            CustomLanguageFile? languageFile = JsonSerializer.Deserialize<CustomLanguageFile>(json);
            return languageFile;
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException or UnauthorizedAccessException or IOException or JsonException)
        {
            _logService?.LogError($"言語ファイルの読み込みに失敗しました: {filePath} - {ex.Message}", "CustomLanguageService", ex);
            return null;
        }
    }
}
