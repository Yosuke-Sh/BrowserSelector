using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using System.Globalization;
using System.Resources;

namespace BrowserSelector.Infrastructure.Localization;

/// <summary>
/// 多言語対応サービスの実装.
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly ResourceManager _resourceManager;
    private readonly ICustomLanguageService _customLanguageService;
    private readonly ILogService? _logService;
    private Dictionary<string, string> _customResources = [];
    private Dictionary<string, string> _jsonResources = [];

    public LocalizationService(ICustomLanguageService customLanguageService, ILogService? logService = null)
    {
        _resourceManager = new ResourceManager("BrowserSelector.Infrastructure.Localization.Resources", typeof(LocalizationService).Assembly);
        _customLanguageService = customLanguageService;
        _logService = logService;
        CurrentCulture = new CultureInfo("en-US");

        // 初期化時にJSONリソースとカスタムリソースを読み込み
        _ = Task.Run(async () =>
        {
            await LoadJsonResourcesAsync(CurrentCulture.Name).ConfigureAwait(false);
            await LoadCustomLanguageResourcesAsync(CurrentCulture.Name).ConfigureAwait(false);
        });
    }

    /// <inheritdoc/>
    public string GetString(string key)
    {
        _logService?.LogDebug($"GetString呼び出し: {key}, 現在のカルチャ: {CurrentCulture.Name}, カスタムリソース数: {_customResources.Count}, JSONリソース数: {_jsonResources.Count}", "LocalizationService");

        // カスタム言語リソースを優先
        if (_customResources.TryGetValue(key, out string? customValue))
        {
            _logService?.LogDebug($"カスタムリソースから取得: {key} = {customValue}", "LocalizationService");
            return customValue;
        }

        // JSONリソースを確認
        if (_jsonResources.TryGetValue(key, out string? jsonValue))
        {
            _logService?.LogDebug($"JSONリソースから取得: {key} = {jsonValue}", "LocalizationService");
            return jsonValue;
        }

        // フォールバック: デフォルトリソースを使用
        string? fallbackValue = _resourceManager.GetString(key, CurrentCulture);
        if (!string.IsNullOrEmpty(fallbackValue))
        {
            _logService?.LogDebug($"デフォルトリソースから取得: {key} = {fallbackValue}", "LocalizationService");
            return fallbackValue;
        }

        // リソースが見つからない場合はキーをそのまま返す
        _logService?.LogWarning($"リソースキーが見つかりません: {key}, カスタムリソース数: {_customResources.Count}, JSONリソース数: {_jsonResources.Count}", "LocalizationService");
        return key;
    }

    /// <inheritdoc/>
    public string GetString(string key, params object[] args)
    {
        string format = GetString(key);
        return string.Format(CultureInfo.InvariantCulture, format, args);
    }

    /// <inheritdoc/>
    public async Task SetLanguage(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);
        if (CurrentCulture.Equals(culture))
        {
            return;
        }

        CultureInfo oldCulture = CurrentCulture;
        CurrentCulture = culture;

        // JSONリソースを読み込み
        await LoadJsonResourcesAsync(culture.Name).ConfigureAwait(false);

        // カスタム言語リソースを読み込み
        await LoadCustomLanguageResourcesAsync(culture.Name).ConfigureAwait(false);

        LanguageChanged?.Invoke(this, new LanguageChangedEventArgs(oldCulture, culture));
        _logService?.LogInformation($"言語を {oldCulture.Name} から {culture.Name} に変更しました", "LocalizationService");
    }

    /// <inheritdoc/>
    public CultureInfo CurrentCulture { get; private set; }

    /// <inheritdoc/>
    public async Task<IEnumerable<CultureInfo>> GetSupportedLanguagesAsync()
    {
        List<CultureInfo> languages = [];

        try
        {
            IEnumerable<LanguageInfo> availableLanguages = await _customLanguageService.GetAvailableLanguagesAsync().ConfigureAwait(false);

            foreach (LanguageInfo languageInfo in availableLanguages)
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

    /// <inheritdoc/>
    public IEnumerable<CultureInfo> SupportedLanguages => new[]
    {
        new CultureInfo("en-US"),
        new CultureInfo("ja-JP")
    };

    /// <inheritdoc/>
    public event EventHandler<LanguageChangedEventArgs>? LanguageChanged;

    /// <summary>
    /// JSONファイルからリソースを読み込み（非同期版）.
    /// </summary>
    private async Task LoadJsonResourcesAsync(string cultureCode)
    {
        try
        {
            _jsonResources.Clear();

            System.Reflection.Assembly assembly = typeof(LocalizationService).Assembly;
            string resourceName = $"BrowserSelector.Infrastructure.Localization.{cultureCode}.json";

            using System.IO.Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                _logService?.LogDebug($"JSONリソースファイルが見つかりません: {resourceName}", "LocalizationService");
                return;
            }

            using System.IO.StreamReader reader = new(stream);
            string json = await reader.ReadToEndAsync().ConfigureAwait(false);

            CustomLanguageFile? languageFile = System.Text.Json.JsonSerializer.Deserialize<CustomLanguageFile>(json);
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
    /// カスタム言語リソースを読み込み.
    /// </summary>
    private async Task LoadCustomLanguageResourcesAsync(string cultureCode)
    {
        try
        {
            _customResources.Clear();

            // すべての言語でカスタムリソースを読み込み（デフォルト言語も含む）
            Dictionary<string, string>? customResources = await _customLanguageService.LoadCustomLanguageAsync(cultureCode).ConfigureAwait(false);
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
