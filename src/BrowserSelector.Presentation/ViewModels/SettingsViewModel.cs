using BrowserSelector.Core.Enums;
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Presentation.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace BrowserSelector.Presentation.ViewModels;

/// <summary>
/// 設定変更イベントの引数
/// </summary>
public class SettingsChangedEventArgs : EventArgs
{
    public string SettingType { get; }
    public object? OldValue { get; }
    public object? NewValue { get; }

    public SettingsChangedEventArgs(string settingType, object? oldValue, object? newValue)
    {
        SettingType = settingType;
        OldValue = oldValue;
        NewValue = newValue;
    }
}

/// <summary>
/// 設定画面のViewModel
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IBrowserService _browserService;
    private readonly ILocalizationService _localizationService;
    private readonly IUrlRuleService _urlRuleService;
    [ObservableProperty]
    private AppSettings _appSettings = new();

    [ObservableProperty]
    private VisualSettings _visualSettings = new();

    [ObservableProperty]
    private ObservableCollection<Browser> _detectedBrowsers = new();

    [ObservableProperty]
    private ObservableCollection<LanguageInfo> _availableLanguages = new();

    [ObservableProperty]
    private LanguageInfo? _selectedLanguage;

    partial void OnSelectedLanguageChanged(LanguageInfo? value)
    {
        if (value != null && value.CultureCode != AppSettings.Language)
        {
            // 言語設定を更新
            AppSettings.Language = value.CultureCode;

            // ローカライゼーションサービスに言語変更を通知（非同期で実行）
            var culture = new System.Globalization.CultureInfo(value.CultureCode);
            _ = Task.Run(async () =>
            {
                await _localizationService.SetLanguage(culture).ConfigureAwait(false);
                _ = await _settingsService.SaveAppSettingsAsync(AppSettings).ConfigureAwait(false);
            });

            LocalizedLogHelper.LogLanguageChanged(LogService, "SettingsViewModel", value.CultureCode);
        }
    }

    [ObservableProperty]
    private ObservableCollection<LogLevelInfo> _availableLogLevels = new();

    [ObservableProperty]
    private LogLevelInfo? _selectedLogLevel;

    [ObservableProperty]
    private LogSettings _logSettings = new();

    /// <summary>
    /// 設定変更通知イベント
    /// </summary>
    public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    [ObservableProperty]
    private bool _showFocusIndicator = true;

    [ObservableProperty]
    private Color _focusColor = Colors.Black;

    [ObservableProperty]
    private double _focusThickness = 2.0;

    [ObservableProperty]
    private double _focusWidth = 100.0;

    // URLルール関連
    [ObservableProperty]
    private ObservableCollection<UrlRule> _urlRules = new();

    [ObservableProperty]
    private string _testUrl = string.Empty;

    [ObservableProperty]
    private string _testResult = string.Empty;

    [ObservableProperty]
    private Brush _testResultColor = Brushes.Black;

    [ObservableProperty]
    private string _currentLogFilePath = string.Empty;

    // ブラウザ管理用のプロパティ
    [ObservableProperty]
    private Browser? _selectedBrowser;

    [ObservableProperty]
    private bool _isBrowserDialogOpen = false;



    public SettingsViewModel(
        ISettingsService settingsService,
        IBrowserService browserService,
        ILocalizationService localizationService,
        ICustomLanguageService customLanguageService,
        IUrlRuleService urlRuleService,
        ILogService logService)
    {
        _settingsService = settingsService;
        _browserService = browserService;
        _localizationService = localizationService;
        CustomLanguageService = customLanguageService;
        _urlRuleService = urlRuleService;
        LogService = logService;



        InitializeAsync();
    }

    /// <summary>
    /// 初期化処理
    /// </summary>
    private async void InitializeAsync()
    {
        try
        {
            LogService?.LogDebug("SettingsViewModel初期化開始", "SettingsViewModel");

            // 設定を読み込み
            AppSettings = await _settingsService.LoadAppSettingsAsync().ConfigureAwait(false);
            LogService?.LogDebug($"AppSettings読み込み完了: Language={AppSettings.Language}", "SettingsViewModel");

            VisualSettings = await _settingsService.LoadVisualSettingsAsync().ConfigureAwait(false);

            // グラデーション設定の初期化（デフォルト値の設定）
            LogService?.LogDebug($"初期化前のGradientDirection: {VisualSettings.GradientDirection}", "SettingsViewModel");

            if (VisualSettings.UseBackgroundGradient)
            {
                LogService?.LogDebug("グラデーションが有効です。初期値を設定します。", "SettingsViewModel");

                if (VisualSettings.GradientStartColor == Colors.Transparent)
                {
                    VisualSettings.GradientStartColor = Colors.White;
                    LogService?.LogDebug("グラデーション開始色をWhiteに設定", "SettingsViewModel");
                }
                if (VisualSettings.GradientEndColor == Colors.Transparent)
                {
                    VisualSettings.GradientEndColor = Colors.LightGray;
                    LogService?.LogDebug("グラデーション終了色をLightGrayに設定", "SettingsViewModel");
                }
                // グラデーション方向の初期値を確実に設定
                if (VisualSettings.GradientDirection == 0)
                {
                    VisualSettings.GradientDirection = BrowserSelector.Core.Enums.GradientDirection.Vertical;
                    LogService?.LogDebug("グラデーション方向をVerticalに設定（初期値0から変更）", "SettingsViewModel");
                }
                else
                {
                    LogService?.LogDebug($"グラデーション方向は既に設定済み: {VisualSettings.GradientDirection}", "SettingsViewModel");
                }
            }
            else
            {
                LogService?.LogDebug("グラデーションは無効です", "SettingsViewModel");
            }

            LogService?.LogDebug($"VisualSettings読み込み完了: BackgroundColor={VisualSettings.BackgroundColor}, UseBackgroundGradient={VisualSettings.UseBackgroundGradient}, GradientDirection={VisualSettings.GradientDirection}", "SettingsViewModel");

            // 言語リストを初期化
            InitializeLanguages();
            LogService?.LogDebug("言語リスト初期化完了", "SettingsViewModel");

            // ブラウザリストを更新
            await RefreshBrowsersAsync().ConfigureAwait(false);
            LogService?.LogDebug("ブラウザリスト更新完了", "SettingsViewModel");

            // URLルールリストを更新
            await RefreshUrlRulesAsync().ConfigureAwait(false);
            LogService?.LogDebug("URLルールリスト更新完了", "SettingsViewModel");

            // ログレベルの初期化（先に実行）
            InitializeLogLevels();
            LogService?.LogDebug("ログレベル初期化完了", "SettingsViewModel");

            // ログ設定の読み込み
            await LoadLogSettingsAsync().ConfigureAwait(false);
            LogService?.LogDebug("ログ設定読み込み完了", "SettingsViewModel");

            // プロパティ変更イベントを監視
            PropertyChanged += OnPropertyChanged;
            LogService?.LogDebug("プロパティ変更イベント監視開始", "SettingsViewModel");

            LogService?.LogDebug("SettingsViewModel初期化完了", "SettingsViewModel");
        }
        catch (Exception ex)
        {
            LogService?.LogError($"設定画面の初期化エラー: {ex.Message}", "SettingsViewModel", ex);
        }
    }

    /// <summary>
    /// 言語リストを初期化
    /// </summary>
    private async void InitializeLanguages()
    {
        try
        {
            AvailableLanguages.Clear();

            // カスタム言語サービスから利用可能な言語を取得（ローカライズ不要の表示名）
            IEnumerable<Core.Models.LanguageInfo> availableLanguages = await CustomLanguageService.GetAvailableLanguagesAsync().ConfigureAwait(false);

            foreach (Core.Models.LanguageInfo languageInfo in availableLanguages)
            {
                AvailableLanguages.Add(new LanguageInfo(languageInfo.CultureCode, languageInfo.DisplayName));
            }

            // 現在の言語を選択
            SelectedLanguage = AvailableLanguages.FirstOrDefault(l => l.CultureCode == AppSettings.Language)
                              ?? AvailableLanguages.First();

            LogService?.LogDebug($"言語リスト初期化完了: {AvailableLanguages.Count}個の言語", "SettingsViewModel");
        }
        catch (Exception ex)
        {
            LogService?.LogError($"言語リストの初期化に失敗しました: {ex.Message}", "SettingsViewModel", ex);

            // フォールバック: デフォルト言語のみ
            AvailableLanguages.Clear();
            AvailableLanguages.Add(new LanguageInfo("en-US", "English"));
            AvailableLanguages.Add(new LanguageInfo("ja-JP", "日本語"));
            SelectedLanguage = AvailableLanguages.FirstOrDefault(l => l.CultureCode == AppSettings.Language)
                              ?? AvailableLanguages.First();
        }
    }

    /// <summary>
    /// 言語一覧を更新（外部から呼び出し可能）
    /// </summary>
    public async Task RefreshLanguagesAsync()
    {
        await InitializeLanguagesAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// 言語一覧を更新（同期版）
    /// </summary>
    public void RefreshLanguages()
    {
        _ = Task.Run(InitializeLanguagesAsync);
    }

    /// <summary>
    /// 言語リストを初期化（非同期版）
    /// </summary>
    private async Task InitializeLanguagesAsync()
    {
        try
        {
            AvailableLanguages.Clear();

            // カスタム言語サービスから利用可能な言語を取得（ローカライズ不要の表示名）
            IEnumerable<Core.Models.LanguageInfo> availableLanguages = await CustomLanguageService.GetAvailableLanguagesAsync().ConfigureAwait(false);

            foreach (Core.Models.LanguageInfo languageInfo in availableLanguages)
            {
                AvailableLanguages.Add(new LanguageInfo(languageInfo.CultureCode, languageInfo.DisplayName));
            }

            // 現在の言語を選択
            SelectedLanguage = AvailableLanguages.FirstOrDefault(l => l.CultureCode == AppSettings.Language)
                              ?? AvailableLanguages.First();

            LogService?.LogDebug($"言語リスト初期化完了: {AvailableLanguages.Count}個の言語", "SettingsViewModel");
        }
        catch (Exception ex)
        {
            LogService?.LogError($"言語リストの初期化に失敗しました: {ex.Message}", "SettingsViewModel", ex);

            // フォールバック: デフォルト言語のみ
            AvailableLanguages.Clear();
            AvailableLanguages.Add(new LanguageInfo("en-US", "English"));
            AvailableLanguages.Add(new LanguageInfo("ja-JP", "日本語"));
            SelectedLanguage = AvailableLanguages.FirstOrDefault(l => l.CultureCode == AppSettings.Language)
                              ?? AvailableLanguages.First();
        }
    }

    /// <summary>
    /// カスタム言語サービス（外部からアクセス可能）
    /// </summary>
    public ICustomLanguageService CustomLanguageService { get; }

    /// <summary>
    /// ログサービス（外部からアクセス可能）
    /// </summary>
    public ILogService LogService { get; }

    /// <summary>
    /// ブラウザリストを更新
    /// </summary>
    private async Task RefreshBrowsersAsync()
    {
        try
        {
            IEnumerable<Browser> browsers = await _browserService.GetAllBrowsersAsync().ConfigureAwait(false);
            DetectedBrowsers.Clear();
            foreach (Browser browser in browsers)
            {
                DetectedBrowsers.Add(browser);
            }
        }
        catch (Exception)
        {
        }
    }

    /// <summary>
    /// URLルールリストを更新
    /// </summary>
    private async Task RefreshUrlRulesAsync()
    {
        try
        {
            IEnumerable<UrlRule> rules = await _urlRuleService.GetAllRulesAsync().ConfigureAwait(false);
            UrlRules.Clear();
            foreach (UrlRule rule in rules)
            {
                UrlRules.Add(rule);
            }
        }
        catch (Exception)
        {
        }
    }

    /// <summary>
    /// ログ設定の読み込み
    /// </summary>
    private async Task LoadLogSettingsAsync()
    {
        try
        {
            // ログ設定を読み込み
            LogSettings = await _settingsService.LoadLogSettingsAsync().ConfigureAwait(false);

            // 現在のログファイルパスを設定
            CurrentLogFilePath = LogService.GetLogFilePath();

            // ログサービスの設定を更新
            LogService.UpdateSettings(LogSettings);

            // 選択されたログレベルを設定
            SelectedLogLevel = AvailableLogLevels.FirstOrDefault(l => l.LogLevel == LogSettings.LogLevel);

            // デバッグ情報を出力
        }
        catch (Exception)
        {
            // エラー時はデフォルト設定を使用
            await RefreshLogSettingsAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// ログ設定の初期化
    /// </summary>
    private Task RefreshLogSettingsAsync()
    {
        try
        {
            // ログ設定の初期化
            LogSettings.EnableLogging = true;
            LogSettings.LogLevel = LogLevel.Information; // デフォルトをInformationに変更
            LogSettings.LogOutputFolder = LogSettings.GetDefaultLogFolder();
            LogSettings.MaxLogFileSize = 10;
            LogSettings.LogRetentionDays = 30;
            LogSettings.EnableConsoleLogging = false;
            LogSettings.EnableFileLogging = true;
            LogSettings.LogFilePrefix = "BrowserSelector";
            LogSettings.LogFileSuffix = "log";

            // 現在のログファイルパスを設定
            CurrentLogFilePath = LogService.GetLogFilePath();

            // ログサービスの設定を更新
            LogService.UpdateSettings(LogSettings);

            // 選択されたログレベルを設定
            SelectedLogLevel = AvailableLogLevels.FirstOrDefault(l => l.LogLevel == LogSettings.LogLevel);

            // デバッグ情報を出力
        }
        catch (Exception)
        {
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// ログレベルの初期化
    /// </summary>
    private void InitializeLogLevels()
    {
        AvailableLogLevels.Clear();
        AvailableLogLevels.Add(new LogLevelInfo(LogLevel.Trace, LocalizedLogHelper.GetString("LogLevel.Trace")));
        AvailableLogLevels.Add(new LogLevelInfo(LogLevel.Debug, LocalizedLogHelper.GetString("LogLevel.Debug")));
        AvailableLogLevels.Add(new LogLevelInfo(LogLevel.Information, LocalizedLogHelper.GetString("LogLevel.Information")));
        AvailableLogLevels.Add(new LogLevelInfo(LogLevel.Warning, LocalizedLogHelper.GetString("LogLevel.Warning")));
        AvailableLogLevels.Add(new LogLevelInfo(LogLevel.Error, LocalizedLogHelper.GetString("LogLevel.Error")));
        AvailableLogLevels.Add(new LogLevelInfo(LogLevel.Critical, LocalizedLogHelper.GetString("LogLevel.Critical")));
    }

    /// <summary>
    /// プロパティ変更時の処理
    /// </summary>
    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // リアルタイムプレビューの更新
        if (e.PropertyName is (nameof(VisualSettings)) or
            "VisualSettings.BackgroundColor" or
            "VisualSettings.UseBackgroundGradient" or
            "VisualSettings.GradientStartColor" or
            "VisualSettings.GradientEndColor" or
            "VisualSettings.GradientDirection")
        {
            LogService?.LogDebug($"視覚設定プロパティ変更検知: {e.PropertyName}", "SettingsViewModel");

            // グラデーション方向の変更を詳細にログ出力
            if (e.PropertyName == "VisualSettings.GradientDirection")
            {
                LogService?.LogDebug($"グラデーション方向が変更されました: {VisualSettings.GradientDirection}", "SettingsViewModel");
            }

            // グラデーションチェックボックスが有効になった時のデフォルト値設定
            if (e.PropertyName == "VisualSettings.UseBackgroundGradient" && VisualSettings.UseBackgroundGradient)
            {
                LogService?.LogDebug("グラデーションチェックボックスが有効になりました。デフォルト値を設定します。", "SettingsViewModel");

                if (VisualSettings.GradientStartColor == Colors.Transparent)
                {
                    VisualSettings.GradientStartColor = Colors.White;
                    LogService?.LogDebug("グラデーション開始色をWhiteに設定", "SettingsViewModel");
                }
                if (VisualSettings.GradientEndColor == Colors.Transparent)
                {
                    VisualSettings.GradientEndColor = Colors.LightGray;
                    LogService?.LogDebug("グラデーション終了色をLightGrayに設定", "SettingsViewModel");
                }
                if (VisualSettings.GradientDirection == 0) // デフォルト値
                {
                    VisualSettings.GradientDirection = BrowserSelector.Core.Enums.GradientDirection.Vertical;
                    LogService?.LogDebug("グラデーション方向をVerticalに設定", "SettingsViewModel");
                }
                else
                {
                    LogService?.LogDebug($"グラデーション方向は既に設定済み: {VisualSettings.GradientDirection}", "SettingsViewModel");
                }
            }

            await UpdateVisualSettingsAsync().ConfigureAwait(false);
        }

        // 言語変更時の処理
        if (e.PropertyName == nameof(SelectedLanguage) && SelectedLanguage != null)
        {
            AppSettings.Language = SelectedLanguage.CultureCode;
            _ = Task.Run(async () => await _localizationService.SetLanguage(new System.Globalization.CultureInfo(SelectedLanguage.CultureCode)).ConfigureAwait(false));
        }

        // ログ設定の変更時の処理
        if (e.PropertyName == nameof(SelectedLogLevel) && SelectedLogLevel != null)
        {
            LogSettings.LogLevel = SelectedLogLevel.LogLevel;
            LogService?.UpdateSettings(LogSettings);
            LogService?.LogInformation($"ログレベルが変更されました: {SelectedLogLevel.DisplayName}", "SettingsViewModel");
        }
    }

    /// <summary>
    /// 視覚設定を更新
    /// </summary>
    private async Task UpdateVisualSettingsAsync()
    {
        try
        {
            LogService?.LogDebug("UpdateVisualSettingsAsync開始", "SettingsViewModel");
            LogService?.LogDebug($"保存対象: UseBackgroundGradient={VisualSettings.UseBackgroundGradient}, GradientDirection={VisualSettings.GradientDirection}, StartColor={VisualSettings.GradientStartColor}, EndColor={VisualSettings.GradientEndColor}", "SettingsViewModel");

            // グラデーション方向の詳細ログ
            if (VisualSettings.UseBackgroundGradient)
            {
                LogService?.LogDebug($"グラデーション設定詳細: 方向={VisualSettings.GradientDirection} (値={Convert.ToInt32(VisualSettings.GradientDirection)})", "SettingsViewModel");
            }

            bool result = await _settingsService.SaveVisualSettingsAsync(VisualSettings).ConfigureAwait(false);
            LogService?.LogDebug($"設定保存結果: {result}", "SettingsViewModel");

            if (result)
            {
                ApplyVisualToActiveWindow(VisualSettings);
                LogService?.LogDebug("視覚設定の適用完了", "SettingsViewModel");
            }
            else
            {
                LogService?.LogWarning("設定の保存に失敗しました", "SettingsViewModel");
            }
        }
        catch (Exception ex)
        {
            LogService?.LogError($"視覚設定更新エラー: {ex.Message}", "SettingsViewModel", ex);
        }
    }



    /// <summary>
    /// 現在表示中のウィンドウへ視覚設定を即時適用
    /// </summary>
    private void ApplyVisualToActiveWindow(VisualSettings settings)
    {
        try
        {
            LogService?.LogDebug("ApplyVisualToActiveWindow開始", "SettingsViewModel");
            LogService?.LogDebug($"受領値: BackgroundColor={settings.BackgroundColor}", "SettingsViewModel");

            Window? window = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                         ?? Application.Current?.MainWindow;
            if (window == null)
            {
                LogService?.LogDebug("適用対象のウィンドウが見つかりません", "SettingsViewModel");
                return;
            }

            LogService?.LogDebug($"適用対象ウィンドウ: {window.GetType().Name}, Title={window.Title}", "SettingsViewModel");

            // 適用前の値を記録
            Brush beforeBackground = window.Background;
            LogService?.LogDebug($"適用前の背景: {beforeBackground}", "SettingsViewModel");



            // 背景色 / グラデーション
            if (settings.UseBackgroundGradient)
            {
                LogService?.LogDebug($"グラデーション設定開始: 方向={settings.GradientDirection}, 開始色={settings.GradientStartColor}, 終了色={settings.GradientEndColor}", "SettingsViewModel");

                // グラデーション方向に応じてStartPointとEndPointを設定
                System.Windows.Point startPoint, endPoint;
                switch (settings.GradientDirection)
                {
                    case BrowserSelector.Core.Enums.GradientDirection.Horizontal:
                        startPoint = new System.Windows.Point(0, 0);
                        endPoint = new System.Windows.Point(1, 0);
                        LogService?.LogDebug("水平方向グラデーションを設定", "SettingsViewModel");
                        break;
                    case BrowserSelector.Core.Enums.GradientDirection.Diagonal:
                        startPoint = new System.Windows.Point(0, 0);
                        endPoint = new System.Windows.Point(1, 1);
                        LogService?.LogDebug("斜め方向グラデーションを設定", "SettingsViewModel");
                        break;
                    default: // Vertical
                        startPoint = new System.Windows.Point(0, 0);
                        endPoint = new System.Windows.Point(0, 1);
                        LogService?.LogDebug("垂直方向グラデーションを設定", "SettingsViewModel");
                        break;
                }

                LinearGradientBrush gradientBrush = new()
                {
                    StartPoint = startPoint,
                    EndPoint = endPoint,
                    GradientStops =
                    [
                        new GradientStop(settings.GradientStartColor, 0),
                        new GradientStop(settings.GradientEndColor, 1)
                    ]
                };

                window.Background = gradientBrush;
                LogService?.LogDebug($"背景グラデーション設定完了: 方向={settings.GradientDirection}, 開始色={settings.GradientStartColor}, 終了色={settings.GradientEndColor}, 適用後={window.Background}", "SettingsViewModel");
            }
            else
            {
                LogService?.LogDebug($"背景色設定開始: 設定値={settings.BackgroundColor}", "SettingsViewModel");

                SolidColorBrush newBrush = new(settings.BackgroundColor);
                window.Background = newBrush;

                LogService?.LogDebug($"背景色設定完了: 設定値={settings.BackgroundColor}, 適用後={window.Background}", "SettingsViewModel");
            }



            LogService?.LogDebug("ApplyVisualToActiveWindow完了", "SettingsViewModel");
        }
        catch (Exception ex)
        {
            LogService?.LogError($"視覚設定即時適用エラー: {ex.Message}", "SettingsViewModel", ex);
        }
    }

    #region Commands

    /// <summary>
    /// ブラウザを再検出するコマンド
    /// </summary>
    [RelayCommand]
    private async Task RefreshBrowsers()
    {
        try
        {
            LogService?.LogInformation("ブラウザ再検出開始", "SettingsViewModel");

            // 明示的にブラウザ検出を実行
            IEnumerable<Browser> browsers = await _browserService.DetectBrowsersAsync().ConfigureAwait(false);
            DetectedBrowsers.Clear();
            foreach (Browser browser in browsers)
            {
                DetectedBrowsers.Add(browser);
            }

            LogService?.LogInformation($"ブラウザ再検出完了: {browsers.Count()}個のブラウザを検出", "SettingsViewModel");

            _ = LocalizedMessageBox.Show($"ブラウザ {browsers.Count()} 個を検出しました。", "完了");
        }
        catch (Exception ex)
        {
            LogService?.LogError($"ブラウザ再検出エラー: {ex.Message}", "SettingsViewModel", ex);

            _ = LocalizedMessageBox.ShowError($"ブラウザの再検出中にエラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// ブラウザを追加するコマンド
    /// </summary>
    [RelayCommand]
    private async Task AddBrowser()
    {
        try
        {
            LogService?.LogInformation("ブラウザ追加開始", "SettingsViewModel");

            Views.BrowserEditDialog dialog = new(null, false, LogService);
            if (dialog.ShowDialog() == true)
            {
                Browser newBrowser = dialog.Browser;
                newBrowser.DisplayOrder = DetectedBrowsers.Count + 1;

                bool result = await _browserService.AddBrowserAsync(newBrowser).ConfigureAwait(false);
                if (result)
                {
                    await RefreshBrowsersAsync().ConfigureAwait(false);
                    LogService?.LogInformation("ブラウザ追加完了", "SettingsViewModel");
                    _ = LocalizedMessageBox.Show("ブラウザを追加しました。", "完了");
                }
                else
                {
                    LogService?.LogWarning("ブラウザ追加失敗", "SettingsViewModel");
                    _ = LocalizedMessageBox.ShowError("ブラウザの追加に失敗しました。");
                }
            }
        }
        catch (Exception ex)
        {
            LogService?.LogError($"ブラウザ追加エラー: {ex.Message}", "SettingsViewModel", ex);
            _ = LocalizedMessageBox.ShowError($"ブラウザの追加中にエラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// URLルールを更新するコマンド
    /// </summary>
    [RelayCommand]
    private async Task RefreshUrlRules()
    {
        await RefreshUrlRulesAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// URLルールを追加するコマンド
    /// </summary>
    [RelayCommand]
    private async Task AddUrlRule()
    {
        try
        {
            LogService?.LogInformation("URLルール追加開始", "SettingsViewModel");

            Views.UrlRuleEditDialog dialog = new(_browserService, LogService!);
            if (dialog.ShowDialog() == true)
            {
                UrlRule newRule = dialog.UrlRule;
                bool result = await _urlRuleService.AddRuleAsync(newRule).ConfigureAwait(false);
                if (result)
                {
                    await RefreshUrlRulesAsync().ConfigureAwait(false);
                    LogService?.LogInformation("URLルール追加完了", "SettingsViewModel");
                    _ = MessageBox.Show("URLルールを追加しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    LogService?.LogWarning("URLルール追加失敗", "SettingsViewModel");
                    _ = MessageBox.Show("URLルールの追加に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            LogService?.LogError($"URLルール追加エラー: {ex.Message}", "SettingsViewModel", ex);
            _ = MessageBox.Show($"URLルールの追加中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// URLルールを編集するコマンド
    /// </summary>
    [RelayCommand]
    private async Task EditUrlRule(UrlRule rule)
    {
        try
        {
            LogService?.LogInformation($"URLルール編集開始: {rule.Pattern}", "SettingsViewModel");

            Views.UrlRuleEditDialog dialog = new(rule, _browserService, LogService!);
            if (dialog.ShowDialog() == true)
            {
                UrlRule updatedRule = dialog.UrlRule;
                bool result = await _urlRuleService.UpdateRuleAsync(updatedRule).ConfigureAwait(false);
                if (result)
                {
                    await RefreshUrlRulesAsync().ConfigureAwait(false);
                    LogService?.LogInformation("URLルール編集完了", "SettingsViewModel");
                    _ = MessageBox.Show("URLルールを更新しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    LogService?.LogWarning("URLルール編集失敗", "SettingsViewModel");
                    _ = MessageBox.Show("URLルールの更新に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            LogService?.LogError($"URLルール編集エラー: {ex.Message}", "SettingsViewModel", ex);
            _ = MessageBox.Show($"URLルールの編集中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// URLルールを削除するコマンド
    /// </summary>
    [RelayCommand]
    private async Task RemoveUrlRule(UrlRule rule)
    {
        try
        {
            LogService?.LogInformation($"URLルール削除開始: {rule.Pattern}", "SettingsViewModel");

            MessageBoxResult result = MessageBox.Show(
                $"URLルール「{rule.Pattern}」を削除しますか？",
                "削除確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                bool deleteResult = await _urlRuleService.DeleteRuleAsync(rule.Id).ConfigureAwait(false);
                if (deleteResult)
                {
                    await RefreshUrlRulesAsync().ConfigureAwait(false);
                    LogService?.LogInformation("URLルール削除完了", "SettingsViewModel");
                    _ = MessageBox.Show("URLルールを削除しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    LogService?.LogWarning("URLルール削除失敗", "SettingsViewModel");
                    _ = MessageBox.Show("URLルールの削除に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            LogService?.LogError($"URLルール削除エラー: {ex.Message}", "SettingsViewModel", ex);
            _ = MessageBox.Show($"URLルールの削除中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// URLルールをテストするコマンド
    /// </summary>
    [RelayCommand]
    private async Task TestUrlRule()
    {
        if (string.IsNullOrWhiteSpace(TestUrl))
        {
            TestResult = LocalizedLogHelper.GetString("Settings.UrlRules.EnterTestUrl");
            TestResultColor = Brushes.Red;
            return;
        }

        try
        {
            Browser? matchingBrowser = await _urlRuleService.FindMatchingBrowserAsync(TestUrl, DetectedBrowsers).ConfigureAwait(false);
            if (matchingBrowser != null)
            {
                TestResult = LocalizedLogHelper.GetString("Settings.UrlRules.MatchFound", matchingBrowser.Name);
                TestResultColor = Brushes.Green;
            }
            else
            {
                TestResult = LocalizedLogHelper.GetString("Settings.UrlRules.NoMatchFound");
                TestResultColor = Brushes.Orange;
            }
        }
        catch (Exception ex)
        {
            TestResult = LocalizedLogHelper.GetString("Settings.UrlRules.TestError", ex.Message);
            TestResultColor = Brushes.Red;
        }
    }



    /// <summary>
    /// 背景色を選択するコマンド
    /// </summary>
    [RelayCommand]
    private void SelectBackgroundColor()
    {
        System.Windows.Forms.ColorDialog colorDialog = new();
        if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            Color color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
            VisualSettings.BackgroundColor = color;
        }
    }

    /// <summary>
    /// グラデーション開始色を選択するコマンド
    /// </summary>
    [RelayCommand]
    private void SelectGradientStartColor()
    {
        System.Windows.Forms.ColorDialog colorDialog = new();
        if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            Color color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
            VisualSettings.GradientStartColor = color;
        }
    }

    /// <summary>
    /// グラデーション終了色を選択するコマンド
    /// </summary>
    [RelayCommand]
    private void SelectGradientEndColor()
    {
        System.Windows.Forms.ColorDialog colorDialog = new();
        if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            Color color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
            VisualSettings.GradientEndColor = color;
        }
    }



    /// <summary>
    /// フォーカス色を選択するコマンド
    /// </summary>
    [RelayCommand]
    private void SelectFocusColor()
    {
        System.Windows.Forms.ColorDialog colorDialog = new();
        if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            Color color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
            FocusColor = color;
        }
    }

    /// <summary>
    /// ブラウザボタン背景色を選択するコマンド
    /// </summary>
    [RelayCommand]
    private void SelectBrowserButtonBackgroundColor()
    {
        System.Windows.Forms.ColorDialog colorDialog = new();
        if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            Color color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
            VisualSettings.BrowserButtonBackgroundColor = color;
        }
    }

    /// <summary>
    /// ブラウザボタンテキスト色を選択するコマンド
    /// </summary>
    [RelayCommand]
    private void SelectBrowserButtonForegroundColor()
    {
        System.Windows.Forms.ColorDialog colorDialog = new();
        if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            Color color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
            VisualSettings.BrowserButtonForegroundColor = color;
        }
    }

    /// <summary>
    /// 設定をリセットするコマンド
    /// </summary>
    [RelayCommand]
    private async Task ResetSettings()
    {
        try
        {
            bool result = await _settingsService.ResetSettingsAsync().ConfigureAwait(false);
            if (result)
            {
                // 設定を再読み込み
                AppSettings = await _settingsService.LoadAppSettingsAsync().ConfigureAwait(false);
                VisualSettings = await _settingsService.LoadVisualSettingsAsync().ConfigureAwait(false);
                InitializeLanguages();
            }
        }
        catch (Exception)
        {
        }
    }

    /// <summary>
    /// 設定をインポートするコマンド
    /// </summary>
    [RelayCommand]
    private async Task ImportSettings()
    {
        try
        {
            LogService?.LogInformation("設定インポート開始", "SettingsViewModel");

            OpenFileDialog openFileDialog = new()
            {
                Filter = "ZIP files (*.zip)|*.zip|JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "設定ファイル群を選択"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                bool result = await _settingsService.ImportSettingsAsync(openFileDialog.FileName).ConfigureAwait(false);
                if (result)
                {
                    LogService?.LogInformation($"設定インポート完了: {openFileDialog.FileName}", "SettingsViewModel");

                    // 設定を再読み込み
                    AppSettings = await _settingsService.LoadAppSettingsAsync().ConfigureAwait(false);
                    VisualSettings = await _settingsService.LoadVisualSettingsAsync().ConfigureAwait(false);
                    await InitializeLanguagesAsync().ConfigureAwait(false);

                    _ = LocalizedMessageBox.Show("設定ファイル群をインポートしました。", "完了");
                }
                else
                {
                    LogService?.LogWarning("設定インポート失敗", "SettingsViewModel");
                    _ = LocalizedMessageBox.ShowError("設定のインポートに失敗しました。");
                }
            }
        }
        catch (Exception ex)
        {
            LogService?.LogError($"設定インポートエラー: {ex.Message}", "SettingsViewModel", ex);
            _ = LocalizedMessageBox.ShowError($"設定のインポート中にエラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// 設定をエクスポートするコマンド
    /// </summary>
    [RelayCommand]
    private async Task ExportSettings()
    {
        try
        {
            LogService?.LogInformation("設定エクスポート開始", "SettingsViewModel");

            SaveFileDialog saveFileDialog = new()
            {
                Filter = "ZIP files (*.zip)|*.zip|JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "設定ファイル群を保存",
                FileName = $"browserselector_settings_{DateTime.Now:yyyyMMdd_HHmmss}.zip"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                bool result = await _settingsService.ExportSettingsAsync(saveFileDialog.FileName).ConfigureAwait(false);
                if (result)
                {
                    LogService?.LogInformation($"設定エクスポート完了: {saveFileDialog.FileName}", "SettingsViewModel");
                    _ = LocalizedMessageBox.Show("設定ファイル群をエクスポートしました。", "完了");
                }
                else
                {
                    LogService?.LogWarning("設定エクスポート失敗", "SettingsViewModel");
                    _ = LocalizedMessageBox.ShowError("設定のエクスポートに失敗しました。");
                }
            }
        }
        catch (Exception ex)
        {
            LogService?.LogError($"設定エクスポートエラー: {ex.Message}", "SettingsViewModel", ex);
            _ = LocalizedMessageBox.ShowError($"設定のエクスポート中にエラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// 設定を保存するコマンド
    /// </summary>
    [RelayCommand]
    private async Task SaveSettings()
    {
        LogService?.LogDebug("SaveSettingsコマンド実行開始", "SettingsViewModel");
        await SaveSettingsInternal().ConfigureAwait(false);
    }

    private async Task SaveSettingsInternal()
    {
        try
        {
            LogService?.LogDebug("SaveSettings開始", "SettingsViewModel");
            LogService?.LogDebug($"保存対象VisualSettings: BackgroundColor={VisualSettings.BackgroundColor}", "SettingsViewModel");

            // アプリケーション設定を保存
            bool appSettingsResult = await _settingsService.SaveAppSettingsAsync(AppSettings).ConfigureAwait(false);
            LogService?.LogDebug($"AppSettings保存結果: {appSettingsResult}", "SettingsViewModel");

            // 視覚設定を保存
            bool visualSettingsResult = await _settingsService.SaveVisualSettingsAsync(VisualSettings).ConfigureAwait(false);
            LogService?.LogDebug($"VisualSettings保存結果: {visualSettingsResult}", "SettingsViewModel");

            // ログ設定を保存
            bool logSettingsResult = await _settingsService.SaveLogSettingsAsync(LogSettings).ConfigureAwait(false);
            LogService?.LogDebug($"LogSettings保存結果: {logSettingsResult}", "SettingsViewModel");

            if (appSettingsResult && visualSettingsResult && logSettingsResult)
            {
                LogService?.LogDebug("設定保存成功、メイン画面への反映開始", "SettingsViewModel");

                // 設定変更通知を送信
                SettingsChanged?.Invoke(this, new SettingsChangedEventArgs("VisualSettings", null, VisualSettings));

                // メイン画面へ反映
                ApplyVisualToActiveWindow(VisualSettings);

                LogService?.LogDebug("メイン画面への反映完了", "SettingsViewModel");

                // 成功時はウィンドウを閉じる
                if (Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this) is Window window)
                {
                    LogService?.LogDebug($"設定ウィンドウを閉じる: {window.GetType().Name}", "SettingsViewModel");
                    window.DialogResult = true;
                    window.Close();
                }
            }
            else
            {
                LogService?.LogWarning($"設定保存に失敗: AppSettings={appSettingsResult}, VisualSettings={visualSettingsResult}", "SettingsViewModel");
            }

            LogService?.LogDebug("SaveSettings完了", "SettingsViewModel");
        }
        catch (Exception ex)
        {
            LogService?.LogError($"設定保存エラー: {ex.Message}", "SettingsViewModel", ex);
        }
    }

    /// <summary>
    /// キャンセルコマンド
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        // ウィンドウを閉じる
        if (Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this) is Window window)
        {
            window.DialogResult = false;
            window.Close();
        }
    }

    #region ログ関連コマンド

    /// <summary>
    /// ログフォルダを選択するコマンド
    /// </summary>
    [RelayCommand]
    private void SelectLogFolder()
    {
        try
        {
            System.Windows.Forms.FolderBrowserDialog folderDialog = new()
            {
                Description = "ログ出力フォルダを選択してください",
                SelectedPath = LogSettings.LogOutputFolder
            };

            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LogSettings.LogOutputFolder = folderDialog.SelectedPath;
                CurrentLogFilePath = LogService.GetLogFilePath();
                LogService.UpdateSettings(LogSettings);
            }
        }
        catch (Exception)
        {
        }
    }

    /// <summary>
    /// ログを表示するコマンド
    /// </summary>
    [RelayCommand]
    private void ViewLogs()
    {
        try
        {
            string logContent = LogService.GetLogContent();
            Views.LogViewerWindow logWindow = new(logContent);
            _ = logWindow.ShowDialog();
        }
        catch (Exception)
        {
        }
    }

    /// <summary>
    /// ログをクリアするコマンド
    /// </summary>
    [RelayCommand]
    private void ClearLogs()
    {
        try
        {
            MessageBoxResult result = LocalizedMessageBox.ShowLogClearConfirm();

            if (result == MessageBoxResult.Yes)
            {
                LogService.ClearLogs();
                CurrentLogFilePath = LogService.GetLogFilePath();
                _ = LocalizedMessageBox.ShowLogClearComplete();
            }
        }
        catch (Exception)
        {
        }
    }

    /// <summary>
    /// 古いログを削除するコマンド
    /// </summary>
    [RelayCommand]
    private void CleanupOldLogs()
    {
        try
        {
            MessageBoxResult result = LocalizedMessageBox.ShowOldLogDeleteConfirm();

            if (result == MessageBoxResult.Yes)
            {
                LogService.CleanupOldLogs();
                _ = LocalizedMessageBox.ShowOldLogDeleteComplete();
            }
        }
        catch (Exception)
        {
        }
    }

    #endregion



    /// <summary>
    /// ブラウザを編集するコマンド
    /// </summary>
    [RelayCommand]
    private async Task EditBrowserAsync(Browser? browser = null)
    {
        try
        {
            Browser? targetBrowser = browser ?? SelectedBrowser;
            if (targetBrowser == null)
            {
                _ = MessageBox.Show("編集するブラウザを選択してください。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            LogService?.LogInformation($"ブラウザ編集開始: {targetBrowser.Name}", "SettingsViewModel");

            // システムで検出されたブラウザは、順序・アイコン・パラメーターのみ編集可能
            if (targetBrowser.Type != BrowserType.Custom)
            {
                // システムブラウザの場合は、編集可能な項目を制限
                Browser limitedBrowser = new()
                {
                    Id = targetBrowser.Id,
                    Name = targetBrowser.Name,
                    ExecutablePath = targetBrowser.ExecutablePath,
                    Type = targetBrowser.Type,
                    DisplayOrder = targetBrowser.DisplayOrder,
                    IconPath = targetBrowser.IconPath,
                    Arguments = targetBrowser.Arguments,
                    IsEnabled = targetBrowser.IsEnabled,
                    IsDefault = targetBrowser.IsDefault
                };

                Views.BrowserEditDialog systemBrowserDialog = new(limitedBrowser, true, LogService); // システムブラウザフラグ
                if (systemBrowserDialog.ShowDialog() == true)
                {
                    Browser updatedBrowser = systemBrowserDialog.Browser;

                    // システムブラウザの場合は、編集可能な項目のみ更新
                    targetBrowser.DisplayOrder = updatedBrowser.DisplayOrder;
                    targetBrowser.IconPath = updatedBrowser.IconPath;
                    targetBrowser.Arguments = updatedBrowser.Arguments;
                    targetBrowser.IsEnabled = updatedBrowser.IsEnabled;

                    bool result = await _browserService.UpdateBrowserAsync(targetBrowser).ConfigureAwait(false);
                    if (result)
                    {
                        await RefreshBrowsersAsync().ConfigureAwait(false);
                        LogService?.LogInformation($"システムブラウザ編集完了: {updatedBrowser.Name}", "SettingsViewModel");
                        _ = MessageBox.Show("ブラウザ設定を更新しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        LogService?.LogWarning("システムブラウザ更新失敗", "SettingsViewModel");
                        _ = MessageBox.Show("ブラウザ設定の更新に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                return;
            }

            Views.BrowserEditDialog dialog = new(targetBrowser, false, LogService);
            if (dialog.ShowDialog() == true)
            {
                Browser updatedBrowser = dialog.Browser;

                bool result = await _browserService.UpdateBrowserAsync(updatedBrowser).ConfigureAwait(false);
                if (result)
                {
                    await RefreshBrowsersAsync().ConfigureAwait(false);
                    LogService?.LogInformation($"ブラウザ編集完了: {updatedBrowser.Name}", "SettingsViewModel");
                    _ = MessageBox.Show("ブラウザを更新しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    LogService?.LogWarning("ブラウザ更新失敗", "SettingsViewModel");
                    _ = MessageBox.Show("ブラウザの更新に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            LogService?.LogError($"ブラウザ編集エラー: {ex.Message}", "SettingsViewModel", ex);
            _ = MessageBox.Show($"ブラウザの編集中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// ブラウザを削除するコマンド
    /// </summary>
    [RelayCommand]
    private async Task RemoveBrowserAsync(Browser? browser = null)
    {
        try
        {
            Browser? targetBrowser = browser ?? SelectedBrowser;
            if (targetBrowser == null)
            {
                _ = MessageBox.Show("削除するブラウザを選択してください。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            LogService?.LogInformation($"ブラウザ削除開始: {targetBrowser.Name}", "SettingsViewModel");

            // システム検出ブラウザも削除可能にする
            // if (targetBrowser.Type != BrowserType.Custom)
            // {
            //     MessageBox.Show("システムで検出されたブラウザは削除できません。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            //     return;
            // }

            MessageBoxResult result = MessageBox.Show($"ブラウザ「{targetBrowser.Name}」を削除しますか？", "確認",
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                bool deleteResult = await _browserService.RemoveBrowserAsync(targetBrowser.Id).ConfigureAwait(false);
                if (deleteResult)
                {
                    await RefreshBrowsersAsync().ConfigureAwait(false);
                    if (SelectedBrowser == targetBrowser)
                    {
                        SelectedBrowser = null;
                    }
                    LogService?.LogInformation($"ブラウザ削除完了: {targetBrowser.Name}", "SettingsViewModel");
                    _ = MessageBox.Show("ブラウザを削除しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    LogService?.LogWarning("ブラウザ削除失敗", "SettingsViewModel");
                    _ = MessageBox.Show("ブラウザの削除に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            LogService?.LogError($"ブラウザ削除エラー: {ex.Message}", "SettingsViewModel", ex);
            _ = MessageBox.Show($"ブラウザの削除中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// ブラウザを上に移動するコマンド
    /// </summary>
    [RelayCommand]
    private async Task MoveBrowserUpAsync(Browser browser)
    {
        try
        {
            int currentIndex = DetectedBrowsers.IndexOf(browser);
            if (currentIndex > 0)
            {
                Browser previousBrowser = DetectedBrowsers[currentIndex - 1];

                // DisplayOrderを交換
                (previousBrowser.DisplayOrder, browser.DisplayOrder) = (browser.DisplayOrder, previousBrowser.DisplayOrder);

                // コレクション内の順序を更新
                DetectedBrowsers.Move(currentIndex, currentIndex - 1);

                // サービスに保存
                _ = await _browserService.UpdateBrowserAsync(browser).ConfigureAwait(false);
                _ = await _browserService.UpdateBrowserAsync(previousBrowser).ConfigureAwait(false);

                LogService?.LogInformation($"ブラウザ順序変更: {browser.Name} を上に移動", "SettingsViewModel");
            }
        }
        catch (Exception ex)
        {
            LogService?.LogError($"ブラウザ順序変更エラー: {ex.Message}", "SettingsViewModel", ex);
        }
    }

    /// <summary>
    /// ブラウザを下に移動するコマンド
    /// </summary>
    [RelayCommand]
    private async Task MoveBrowserDownAsync(Browser browser)
    {
        try
        {
            int currentIndex = DetectedBrowsers.IndexOf(browser);
            if (currentIndex < DetectedBrowsers.Count - 1)
            {
                Browser nextBrowser = DetectedBrowsers[currentIndex + 1];

                // DisplayOrderを交換
                (nextBrowser.DisplayOrder, browser.DisplayOrder) = (browser.DisplayOrder, nextBrowser.DisplayOrder);

                // コレクション内の順序を更新
                DetectedBrowsers.Move(currentIndex, currentIndex + 1);

                // サービスに保存
                _ = await _browserService.UpdateBrowserAsync(browser).ConfigureAwait(false);
                _ = await _browserService.UpdateBrowserAsync(nextBrowser).ConfigureAwait(false);

                LogService?.LogInformation($"ブラウザ順序変更: {browser.Name} を下に移動", "SettingsViewModel");
            }
        }
        catch (Exception ex)
        {
            LogService?.LogError($"ブラウザ順序変更エラー: {ex.Message}", "SettingsViewModel", ex);
        }
    }

    #endregion
}

/// <summary>
/// 言語情報を表すクラス
/// </summary>
public class LanguageInfo
{
    public string CultureCode { get; set; }
    public string DisplayName { get; set; }

    public LanguageInfo(string cultureCode, string displayName)
    {
        CultureCode = cultureCode;
        DisplayName = displayName;
    }
}


/// <summary>
/// ログレベル情報を表すクラス
/// </summary>
public class LogLevelInfo
{
    public LogLevel LogLevel { get; set; }
    public string DisplayName { get; set; }

    public LogLevelInfo(LogLevel logLevel, string displayName)
    {
        LogLevel = logLevel;
        DisplayName = displayName;
    }
}
