using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;

namespace BrowserSelector.Presentation.ViewModels;

/// <summary>
/// 言語管理ViewModel.
/// </summary>
public partial class LanguageManagementViewModel : ObservableObject
{
    private readonly ICustomLanguageService _customLanguageService;
    private readonly ILogService? _logService;

    [ObservableProperty]
    private ObservableCollection<LanguageInfo> _availableLanguages = [];

    [ObservableProperty]
    private LanguageInfo? _selectedLanguage;

    [ObservableProperty]
    private string _newLanguageCode = string.Empty;

    [ObservableProperty]
    private string _newLanguageName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<LanguageCodeInfo> _availableLanguageCodes = [];

    [ObservableProperty]
    private LanguageCodeInfo? _selectedLanguageCode;

    [ObservableProperty]
    private bool _isGeneratingTemplate;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageManagementViewModel"/> class.
    /// </summary>
    /// <param name="customLanguageService">customLanguageService.</param>
    /// <param name="logService">logService.</param>
    public LanguageManagementViewModel(ICustomLanguageService customLanguageService, ILogService? logService = null)
    {
        _customLanguageService = customLanguageService;
        _logService = logService;
        InitializeLanguageCodes();
    }

    /// <summary>
    /// 初期化.
    /// </summary>
    /// <returns> representing the asynchronous operation.</returns>
    public async Task InitializeAsync()
    {
        try
        {
            await RefreshLanguagesAsync().ConfigureAwait(false);
            _logService?.LogDebug("LanguageManagementViewModel初期化完了", "LanguageManagementViewModel");
        }
        catch (Exception ex)
        {
            _logService?.LogError($"言語管理の初期化に失敗しました: {ex.Message}", "LanguageManagementViewModel", ex);
            StatusMessage = $"初期化エラー: {ex.Message}";
        }
    }

    partial void OnSelectedLanguageCodeChanged(LanguageCodeInfo? value)
    {
        if (value != null)
        {
            NewLanguageCode = value.Code;
            NewLanguageName = value.NativeName;
        }
    }

    /// <summary>
    /// 言語コードリストを初期化.
    /// </summary>
    private void InitializeLanguageCodes()
    {
        AvailableLanguageCodes.Clear();

        // 主要な言語コードを追加
        LanguageCodeInfo[] languageCodes = new[]
        {
            new LanguageCodeInfo("zh-CN", "Chinese (Simplified)", "中文 (简体)"),
            new LanguageCodeInfo("zh-TW", "Chinese (Traditional)", "中文 (繁體)"),
            new LanguageCodeInfo("ko-KR", "Korean", "한국어"),
            new LanguageCodeInfo("ja-JP", "Japanese", "日本語"),
            new LanguageCodeInfo("fr-FR", "French", "Français"),
            new LanguageCodeInfo("de-DE", "German", "Deutsch"),
            new LanguageCodeInfo("es-ES", "Spanish", "Español"),
            new LanguageCodeInfo("it-IT", "Italian", "Italiano"),
            new LanguageCodeInfo("pt-BR", "Portuguese (Brazil)", "Português (Brasil)"),
            new LanguageCodeInfo("pt-PT", "Portuguese (Portugal)", "Português (Portugal)"),
            new LanguageCodeInfo("ru-RU", "Russian", "Русский"),
            new LanguageCodeInfo("ar-SA", "Arabic", "العربية"),
            new LanguageCodeInfo("hi-IN", "Hindi", "हिन्दी"),
            new LanguageCodeInfo("th-TH", "Thai", "ไทย"),
            new LanguageCodeInfo("vi-VN", "Vietnamese", "Tiếng Việt"),
            new LanguageCodeInfo("nl-NL", "Dutch", "Nederlands"),
            new LanguageCodeInfo("sv-SE", "Swedish", "Svenska"),
            new LanguageCodeInfo("no-NO", "Norwegian", "Norsk"),
            new LanguageCodeInfo("da-DK", "Danish", "Dansk"),
            new LanguageCodeInfo("fi-FI", "Finnish", "Suomi"),
            new LanguageCodeInfo("pl-PL", "Polish", "Polski"),
            new LanguageCodeInfo("tr-TR", "Turkish", "Türkçe"),
            new LanguageCodeInfo("cs-CZ", "Czech", "Čeština"),
            new LanguageCodeInfo("hu-HU", "Hungarian", "Magyar"),
            new LanguageCodeInfo("ro-RO", "Romanian", "Română"),
            new LanguageCodeInfo("bg-BG", "Bulgarian", "Български"),
            new LanguageCodeInfo("hr-HR", "Croatian", "Hrvatski"),
            new LanguageCodeInfo("sk-SK", "Slovak", "Slovenčina"),
            new LanguageCodeInfo("sl-SI", "Slovenian", "Slovenščina"),
            new LanguageCodeInfo("et-EE", "Estonian", "Eesti"),
            new LanguageCodeInfo("lv-LV", "Latvian", "Latviešu"),
            new LanguageCodeInfo("lt-LT", "Lithuanian", "Lietuvių"),
            new LanguageCodeInfo("uk-UA", "Ukrainian", "Українська"),
            new LanguageCodeInfo("be-BY", "Belarusian", "Беларуская"),
            new LanguageCodeInfo("mk-MK", "Macedonian", "Македонски"),
            new LanguageCodeInfo("sq-AL", "Albanian", "Shqip"),
            new LanguageCodeInfo("sr-RS", "Serbian", "Српски"),
            new LanguageCodeInfo("bs-BA", "Bosnian", "Bosanski"),
            new LanguageCodeInfo("mt-MT", "Maltese", "Malti"),
            new LanguageCodeInfo("is-IS", "Icelandic", "Íslenska"),
            new LanguageCodeInfo("ga-IE", "Irish", "Gaeilge"),
            new LanguageCodeInfo("cy-GB", "Welsh", "Cymraeg"),
            new LanguageCodeInfo("eu-ES", "Basque", "Euskera"),
            new LanguageCodeInfo("ca-ES", "Catalan", "Català"),
            new LanguageCodeInfo("gl-ES", "Galician", "Galego"),
            new LanguageCodeInfo("he-IL", "Hebrew", "עברית"),
            new LanguageCodeInfo("fa-IR", "Persian", "فارسی"),
            new LanguageCodeInfo("ur-PK", "Urdu", "اردو"),
            new LanguageCodeInfo("bn-BD", "Bengali", "বাংলা"),
            new LanguageCodeInfo("ta-IN", "Tamil", "தமிழ்"),
            new LanguageCodeInfo("te-IN", "Telugu", "తెలుగు"),
            new LanguageCodeInfo("ml-IN", "Malayalam", "മലയാളം"),
            new LanguageCodeInfo("kn-IN", "Kannada", "ಕನ್ನಡ"),
            new LanguageCodeInfo("gu-IN", "Gujarati", "ગુજરાતી"),
            new LanguageCodeInfo("pa-IN", "Punjabi", "ਪੰਜਾਬੀ"),
            new LanguageCodeInfo("mr-IN", "Marathi", "मराठी"),
            new LanguageCodeInfo("ne-NP", "Nepali", "नेपाली"),
            new LanguageCodeInfo("si-LK", "Sinhala", "සිංහල"),
            new LanguageCodeInfo("my-MM", "Burmese", "မြန်မာ"),
            new LanguageCodeInfo("km-KH", "Khmer", "ខ្មែរ"),
            new LanguageCodeInfo("lo-LA", "Lao", "ລາວ"),
            new LanguageCodeInfo("ka-GE", "Georgian", "ქართული"),
            new LanguageCodeInfo("hy-AM", "Armenian", "Հայերեն"),
            new LanguageCodeInfo("az-AZ", "Azerbaijani", "Azərbaycan"),
            new LanguageCodeInfo("kk-KZ", "Kazakh", "Қазақ"),
            new LanguageCodeInfo("ky-KG", "Kyrgyz", "Кыргызча"),
            new LanguageCodeInfo("uz-UZ", "Uzbek", "O'zbek"),
            new LanguageCodeInfo("tg-TJ", "Tajik", "Тоҷикӣ"),
            new LanguageCodeInfo("mn-MN", "Mongolian", "Монгол"),
            new LanguageCodeInfo("bo-CN", "Tibetan", "བོད་ཡིག"),
            new LanguageCodeInfo("dz-BT", "Dzongkha", "རྫོང་ཁ"),
            new LanguageCodeInfo("sw-KE", "Swahili", "Kiswahili"),
            new LanguageCodeInfo("am-ET", "Amharic", "አማርኛ"),
            new LanguageCodeInfo("ti-ET", "Tigrinya", "ትግርኛ"),
            new LanguageCodeInfo("om-ET", "Oromo", "Afaan Oromoo"),
            new LanguageCodeInfo("so-SO", "Somali", "Soomaali"),
            new LanguageCodeInfo("ha-NG", "Hausa", "Hausa"),
            new LanguageCodeInfo("yo-NG", "Yoruba", "Yorùbá"),
            new LanguageCodeInfo("ig-NG", "Igbo", "Igbo"),
            new LanguageCodeInfo("zu-ZA", "Zulu", "IsiZulu"),
            new LanguageCodeInfo("xh-ZA", "Xhosa", "IsiXhosa"),
            new LanguageCodeInfo("af-ZA", "Afrikaans", "Afrikaans"),
            new LanguageCodeInfo("st-ZA", "Sotho", "Sesotho"),
            new LanguageCodeInfo("tn-ZA", "Tswana", "Setswana"),
            new LanguageCodeInfo("ss-ZA", "Swati", "SiSwati"),
            new LanguageCodeInfo("ve-ZA", "Venda", "Tshivenḓa"),
            new LanguageCodeInfo("ts-ZA", "Tsonga", "Xitsonga"),
            new LanguageCodeInfo("nr-ZA", "Ndebele", "IsiNdebele"),
            new LanguageCodeInfo("nso-ZA", "Northern Sotho", "Sesotho sa Leboa")
        };

        foreach (LanguageCodeInfo? lang in languageCodes)
        {
            AvailableLanguageCodes.Add(lang);
        }
    }

    /// <summary>
    /// 言語一覧を更新.
    /// </summary>
    [RelayCommand]
    private async Task RefreshLanguagesAsync()
    {
        try
        {
            AvailableLanguages.Clear();
            IEnumerable<Core.Models.LanguageInfo> languages = await _customLanguageService.GetAvailableLanguagesAsync().ConfigureAwait(false);

            foreach (Core.Models.LanguageInfo language in languages)
            {
                AvailableLanguages.Add(new LanguageInfo(language.CultureCode, language.DisplayName));
            }

            StatusMessage = $"利用可能な言語: {AvailableLanguages.Count}個";
            _logService?.LogDebug($"言語一覧を更新しました: {AvailableLanguages.Count}個", "LanguageManagementViewModel");
        }
        catch (Exception ex)
        {
            _logService?.LogError($"言語一覧の更新に失敗しました: {ex.Message}", "LanguageManagementViewModel", ex);
            StatusMessage = $"更新エラー: {ex.Message}";
        }
    }

    /// <summary>
    /// 言語テンプレートを生成.
    /// </summary>
    [RelayCommand]
    private async Task GenerateLanguageTemplateAsync()
    {
        if (string.IsNullOrWhiteSpace(NewLanguageCode) || string.IsNullOrWhiteSpace(NewLanguageName))
        {
            StatusMessage = "言語コードと表示名を入力してください";
            return;
        }

        try
        {
            IsGeneratingTemplate = true;
            StatusMessage = "テンプレートを生成中...";

            // カルチャーコードの検証
            try
            {
                CultureInfo culture = new(NewLanguageCode);
            }
            catch
            {
                StatusMessage = "無効な言語コードです（例: zh-CN, ko-KR）";
                return;
            }

            bool success = await _customLanguageService.GenerateLanguageTemplateAsync(NewLanguageCode, NewLanguageName).ConfigureAwait(false);

            if (success)
            {
                StatusMessage = $"言語テンプレートを生成しました: {NewLanguageName} ({NewLanguageCode})";
                NewLanguageCode = string.Empty;
                NewLanguageName = string.Empty;
                await RefreshLanguagesAsync().ConfigureAwait(false);
                _logService?.LogInformation($"言語テンプレートを生成しました: {NewLanguageCode} - {NewLanguageName}", "LanguageManagementViewModel");
            }
            else
            {
                StatusMessage = "テンプレートの生成に失敗しました";
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"言語テンプレートの生成に失敗しました: {ex.Message}", "LanguageManagementViewModel", ex);
            StatusMessage = $"生成エラー: {ex.Message}";
        }
        finally
        {
            IsGeneratingTemplate = false;
        }
    }

    /// <summary>
    /// カスタム言語を削除.
    /// </summary>
    [RelayCommand]
    private async Task RemoveCustomLanguageAsync()
    {
        if (SelectedLanguage == null)
        {
            StatusMessage = "削除する言語を選択してください";
            return;
        }

        // デフォルト言語は削除不可
        if (SelectedLanguage.CultureCode is "en-US" or "ja-JP")
        {
            StatusMessage = "デフォルト言語は削除できません";
            return;
        }

        try
        {
            bool success = await _customLanguageService.RemoveCustomLanguageAsync(SelectedLanguage.CultureCode).ConfigureAwait(false);

            if (success)
            {
                StatusMessage = $"言語を削除しました: {SelectedLanguage.DisplayName}";
                await RefreshLanguagesAsync().ConfigureAwait(false);
                _logService?.LogInformation($"カスタム言語を削除しました: {SelectedLanguage.CultureCode}", "LanguageManagementViewModel");
            }
            else
            {
                StatusMessage = "言語の削除に失敗しました";
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"言語の削除に失敗しました: {ex.Message}", "LanguageManagementViewModel", ex);
            StatusMessage = $"削除エラー: {ex.Message}";
        }
    }

    /// <summary>
    /// 言語フォルダを開く.
    /// </summary>
    [RelayCommand]
    private void OpenLanguageFolder()
    {
        try
        {
            string folderPath = _customLanguageService.GetCustomLanguageFolder();
            _ = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = folderPath,
                UseShellExecute = true,
                Verb = "open"
            });

            StatusMessage = $"言語フォルダを開きました: {folderPath}";
            _logService?.LogDebug($"言語フォルダを開きました: {folderPath}", "LanguageManagementViewModel");
        }
        catch (Exception ex)
        {
            _logService?.LogError($"言語フォルダの表示に失敗しました: {ex.Message}", "LanguageManagementViewModel", ex);
            StatusMessage = $"フォルダ表示エラー: {ex.Message}";
        }
    }

    /// <summary>
    /// 言語ファイルを検証.
    /// </summary>
    [RelayCommand]
    private async Task ValidateLanguageFileAsync()
    {
        if (SelectedLanguage == null)
        {
            StatusMessage = "検証する言語を選択してください";
            return;
        }

        try
        {
            string fileName = $"{SelectedLanguage.CultureCode}.json";
            string filePath = System.IO.Path.Combine(_customLanguageService.GetCustomLanguageFolder(), fileName);

            if (!System.IO.File.Exists(filePath))
            {
                StatusMessage = "言語ファイルが見つかりません";
                return;
            }

            bool isValid = await _customLanguageService.ValidateLanguageFileAsync(filePath).ConfigureAwait(false);

            StatusMessage = isValid ? $"言語ファイルは有効です: {SelectedLanguage.DisplayName}" : $"言語ファイルに問題があります: {SelectedLanguage.DisplayName}";
        }
        catch (Exception ex)
        {
            _logService?.LogError($"言語ファイルの検証に失敗しました: {ex.Message}", "LanguageManagementViewModel", ex);
            StatusMessage = $"検証エラー: {ex.Message}";
        }
    }
}
