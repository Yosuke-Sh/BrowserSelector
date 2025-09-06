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
    private CultureInfo _currentCulture;

    public LocalizationService()
    {
        _resourceManager = new ResourceManager("BrowserSelector.Infrastructure.Localization.Resources", typeof(LocalizationService).Assembly);
        _currentCulture = new CultureInfo("en-US");
    }

    public string GetString(string key)
    {
        return _resourceManager.GetString(key, _currentCulture) ?? key;
    }

    public string GetString(string key, params object[] args)
    {
        var format = GetString(key);
        return string.Format(format, args);
    }

    public void SetLanguage(CultureInfo culture)
    {
        if (_currentCulture.Equals(culture))
            return;

        var oldCulture = _currentCulture;
        _currentCulture = culture;
        LanguageChanged?.Invoke(this, new LanguageChangedEventArgs(oldCulture, culture));
    }

    public CultureInfo CurrentCulture => _currentCulture;

    public IEnumerable<CultureInfo> SupportedLanguages => new[]
    {
        new CultureInfo("en-US"),
        new CultureInfo("ja-JP")
    };

    public event EventHandler<LanguageChangedEventArgs>? LanguageChanged;
}
