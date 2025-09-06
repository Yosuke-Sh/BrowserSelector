using BrowserSelector.Core.Services;
using BrowserSelector.Core.Models;
using System.Globalization;
using System.Resources;
using System.Text.Json;

namespace BrowserSelector.Infrastructure.Localization;

/// <summary>
/// 多言語対応サービスの実装
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly ResourceManager _resourceManager;
    private readonly ICustomLanguageService _customLanguageService;
    private readonly ILogService? _logService;
    private CultureInfo _currentCulture;
    private Dictionary<string, string> _customResources = new();
    private Dictionary<string, string> _jsonResources = new();

    public LocalizationService(ICustomLanguageService customLanguageService, ILogService? logService = null)
    {
        _resourceManager = new ResourceManager("BrowserSelector.Infrastructure.Localization.Resources", typeof(LocalizationService).Assembly);
        _customLanguageService = customLanguageService;
        _logService = logService;
        _currentCulture = new CultureInfo("en-US");
        
        // 初期化時にJSONリソースを読み込み
        _ = Task.Run(async () => await LoadJsonResourcesAsync(_currentCulture.Name));
    }

    public string GetString(string key)
    {
        _logService?.LogDebug($"GetString呼び出し: {key}, 現在のカルチャ: {_currentCulture.Name}", "LocalizationService");
        
        // カスタム言語リソースを優先
        if (_customResources.TryGetValue(key, out var customValue))
        {
            _logService?.LogDebug($"カスタムリソースから取得: {key} = {customValue}", "LocalizationService");
            return customValue;
        }

        // JSONリソースを確認
        if (_jsonResources.TryGetValue(key, out var jsonValue))
        {
            _logService?.LogDebug($"JSONリソースから取得: {key} = {jsonValue}", "LocalizationService");
            return jsonValue;
        }

        // フォールバック: デフォルトリソースを使用
        var fallbackValue = _resourceManager.GetString(key, _currentCulture);
        if (!string.IsNullOrEmpty(fallbackValue))
        {
            _logService?.LogDebug($"デフォルトリソースから取得: {key} = {fallbackValue}", "LocalizationService");
            return fallbackValue;
        }

        // リソースが見つからない場合はキーをそのまま返す
        _logService?.LogWarning($"リソースキーが見つかりません: {key}, JSONリソース数: {_jsonResources.Count}", "LocalizationService");
        return key;
    }

    public string GetString(string key, params object[] args)
    {
        var format = GetString(key);
        return string.Format(format, args);
    }

    public async Task SetLanguage(CultureInfo culture)
    {
        if (_currentCulture.Equals(culture))
            return;

        var oldCulture = _currentCulture;
        _currentCulture = culture;
        
        // JSONリソースを読み込み
        await LoadJsonResourcesAsync(culture.Name);
        
        // カスタム言語リソースを読み込み
        await LoadCustomLanguageResourcesAsync(culture.Name);
        
        LanguageChanged?.Invoke(this, new LanguageChangedEventArgs(oldCulture, culture));
        _logService?.LogInformation($"言語を {oldCulture.Name} から {culture.Name} に変更しました", "LocalizationService");
    }

    public CultureInfo CurrentCulture => _currentCulture;

    public async Task<IEnumerable<CultureInfo>> GetSupportedLanguagesAsync()
    {
        var languages = new List<CultureInfo>();
        
        try
        {
            var availableLanguages = await _customLanguageService.GetAvailableLanguagesAsync();
            
            foreach (var languageInfo in availableLanguages)
            {
                try
                {
                    languages.Add(new CultureInfo(languageInfo.CultureCode));
                }
                catch (Exception ex)
                {
                    _logService?.LogWarning($"無効なカルチャーコードです: {languageInfo.CultureCode} - {ex.Message}", "LocalizationService");
                }
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"サポート言語の取得に失敗しました: {ex.Message}", "LocalizationService", ex);
            
            // フォールバック: デフォルト言語のみ
            languages.Add(new CultureInfo("en-US"));
            languages.Add(new CultureInfo("ja-JP"));
        }

        return languages;
    }

    public IEnumerable<CultureInfo> SupportedLanguages => new[]
    {
        new CultureInfo("en-US"),
        new CultureInfo("ja-JP")
    };

    public event EventHandler<LanguageChangedEventArgs>? LanguageChanged;

    /// <summary>
    /// JSONファイルからリソースを読み込み（非同期版）
    /// </summary>
    private async Task LoadJsonResourcesAsync(string cultureCode)
    {
        try
        {
            _jsonResources.Clear();
            
            var assembly = typeof(LocalizationService).Assembly;
            var resourceName = $"BrowserSelector.Infrastructure.Localization.{cultureCode}.json";
            
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                _logService?.LogDebug($"JSONリソースファイルが見つかりません: {resourceName}", "LocalizationService");
                return;
            }

            using var reader = new System.IO.StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            
            var languageFile = System.Text.Json.JsonSerializer.Deserialize<CustomLanguageFile>(json);
            if (languageFile?.Resources != null)
            {
                _jsonResources = languageFile.Resources;
                _logService?.LogDebug($"JSONリソースを読み込みました: {cultureCode} ({languageFile.Resources.Count}個のリソース)", "LocalizationService");
            }
        }
        catch (Exception ex)
        {
            _logService?.LogWarning($"JSONリソースの読み込みに失敗しました: {cultureCode} - {ex.Message}", "LocalizationService");
        }
    }

    /// <summary>
    /// JSONファイルからリソースを読み込み（同期版）
    /// </summary>
    private Dictionary<string, string>? LoadJsonResources(string cultureCode)
    {
        try
        {
            var assembly = typeof(LocalizationService).Assembly;
            var resourceName = $"BrowserSelector.Infrastructure.Localization.{cultureCode}.json";
            
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                _logService?.LogDebug($"JSONリソースファイルが見つかりません: {resourceName}", "LocalizationService");
                return null;
            }

            using var reader = new System.IO.StreamReader(stream);
            var json = reader.ReadToEnd();
            
            var languageFile = System.Text.Json.JsonSerializer.Deserialize<CustomLanguageFile>(json);
            if (languageFile?.Resources != null)
            {
                _logService?.LogDebug($"JSONリソースを読み込みました: {cultureCode} ({languageFile.Resources.Count}個のリソース)", "LocalizationService");
                return languageFile.Resources;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logService?.LogWarning($"JSONリソースの読み込みに失敗しました: {cultureCode} - {ex.Message}", "LocalizationService");
            return null;
        }
    }

    /// <summary>
    /// カスタム言語リソースを読み込み
    /// </summary>
    private async Task LoadCustomLanguageResourcesAsync(string cultureCode)
    {
        try
        {
            _customResources.Clear();
            
            // デフォルト言語の場合はカスタムリソースを読み込まない
            if (cultureCode == "en-US" || cultureCode == "ja-JP")
            {
                _logService?.LogDebug($"デフォルト言語のためカスタムリソースを読み込みません: {cultureCode}", "LocalizationService");
                return;
            }

            var customResources = await _customLanguageService.LoadCustomLanguageAsync(cultureCode);
            if (customResources != null)
            {
                _customResources = customResources;
                _logService?.LogDebug($"カスタム言語リソースを読み込みました: {cultureCode} ({customResources.Count}個のリソース)", "LocalizationService");
            }
            else
            {
                _logService?.LogDebug($"カスタム言語リソースが見つかりません: {cultureCode}", "LocalizationService");
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"カスタム言語リソースの読み込みに失敗しました: {ex.Message}", "LocalizationService", ex);
        }
    }
}
