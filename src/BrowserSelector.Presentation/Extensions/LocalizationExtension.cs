using BrowserSelector.Core.Services;
using System.Windows.Markup;

namespace BrowserSelector.Presentation.Extensions;

/// <summary>
/// XAMLで多言語化文字列を取得するためのMarkupExtension
/// </summary>
public class LocalizationExtension : MarkupExtension
{
    private static ILocalizationService? _localizationService;

    /// <summary>
    /// リソースキー
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// パラメータ（文字列フォーマット用）
    /// </summary>
    public object[]? Parameters { get; set; }

    public LocalizationExtension()
    {
    }

    public LocalizationExtension(string key)
    {
        Key = key;
    }

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
            return Parameters != null && Parameters.Length > 0
                ? _localizationService.GetString(Key, Parameters)
                : _localizationService.GetString(Key);
        }
        catch
        {
            return Key;
        }
    }

    /// <summary>
    /// ローカライゼーションサービスを設定
    /// </summary>
    public static void SetLocalizationService(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }
}
