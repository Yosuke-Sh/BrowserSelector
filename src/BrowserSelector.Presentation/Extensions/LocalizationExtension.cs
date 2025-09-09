using BrowserSelector.Core.Services;
using System.Windows.Markup;

namespace BrowserSelector.Presentation.Extensions;

/// <summary>
/// XAMLで多言語化文字列を取得するためのMarkupExtension.
/// </summary>
public class LocalizationExtension : MarkupExtension
{
    private static ILocalizationService? _localizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationExtension"/> class.
    /// </summary>
    public LocalizationExtension()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationExtension"/> class.
    /// </summary>
    /// <param name="key"></param>
    public LocalizationExtension(string key)
    {
        Key = key;
    }

    /// <summary>
    /// Gets or sets リソースキー.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets パラメータ（文字列フォーマット用）.
    /// </summary>
    public IList<object>? Parameters { get; }

    /// <summary>
    /// ローカライゼーションサービスを設定.
    /// </summary>
    public static void SetLocalizationService(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Key))
        {
            return string.Empty;
        }

        // サービスはApp.xaml.csで設定される

        if (_localizationService == null)
        {
            return Key;
        }

        try
        {
            return Parameters != null && Parameters.Count > 0
                ? _localizationService.GetString(Key, Parameters)
                : _localizationService.GetString(Key);
        }
        catch
        {
            return Key;
        }
    }
}
