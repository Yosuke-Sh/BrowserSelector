using BrowserSelector.Core.Services;
using System.Globalization;
using System.Resources;

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

    public LocalizationService(ICustomLanguageService customLanguageService, ILogService? logService = null)
    {
        _resourceManager = new ResourceManager("BrowserSelector.Infrastructure.Localization.Resources", typeof(LocalizationService).Assembly);
        _customLanguageService = customLanguageService;
        _logService = logService;
        _currentCulture = new CultureInfo("en-US");
    }

    public string GetString(string key)
    {
        // カスタム言語リソースを優先
        if (_customResources.TryGetValue(key, out var customValue))
        {
            return customValue;
        }

        // デフォルトリソースを使用
        return _resourceManager.GetString(key, _currentCulture) ?? key;
    }

    public string GetString(string key, params object[] args)
    {
        var format = GetString(key);
        return string.Format(format, args);
    }

    public async void SetLanguage(CultureInfo culture)
    {
        if (_currentCulture.Equals(culture))
            return;

        var oldCulture = _currentCulture;
        _currentCulture = culture;
        
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
