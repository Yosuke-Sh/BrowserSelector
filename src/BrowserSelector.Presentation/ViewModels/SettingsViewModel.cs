using BrowserSelector.Core.Enums;
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Presentation.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace BrowserSelector.Presentation.ViewModels;

/// <summary>
/// 設定変更イベントの引数.
/// </summary>
public class SettingsChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsChangedEventArgs"/> class.
    /// </summary>
    /// <param name="settingType">設定タイプ.</param>
    /// <param name="oldValue">古い値.</param>
    /// <param name="newValue">新しい値.</param>
    public SettingsChangedEventArgs(string settingType, object? oldValue, object? newValue)
    {
        SettingType = settingType;
        OldValue = oldValue;
        NewValue = newValue;
    }

    /// <summary>
    /// Gets the setting type.
    /// </summary>
    public string SettingType { get; }

    /// <summary>
    /// Gets the old value.
    /// </summary>
    public object? OldValue { get; }

    /// <summary>
    /// Gets the new value.
    /// </summary>
    public object? NewValue { get; }
}

/// <summary>
/// ブラウザ変更イベントの引数.
/// </summary>
public class BrowserChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserChangedEventArgs"/> class.
    /// </summary>
    /// <param name="browser">変更されたブラウザ.</param>
    /// <param name="changeType">変更タイプ.</param>
    public BrowserChangedEventArgs(Browser browser, string changeType)
    {
        Browser = browser;
        ChangeType = changeType;
    }

    /// <summary>
    /// Gets the browser.
    /// </summary>
    public Browser Browser { get; }

    /// <summary>
    /// Gets the change type.
    /// </summary>
    public string ChangeType { get; }
}

/// <summary>
/// 設定画面のViewModel.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    // フィールド（SA1201: イベントより前に配置）
    private readonly ISettingsService _settingsService;
    private readonly IBrowserService _browserService;
    private readonly ILocalizationService _localizationService;
    private readonly IUrlRuleService _urlRuleService;
    private readonly IExternalLinkService? _externalLinkService;
    private readonly IUpdateService? _updateService;

    [ObservableProperty]
    private bool _showFocusIndicator = true;

    /// <summary>
    /// 検出されたブラウザ一覧.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Browser> _detectedBrowsers = new();

    /// <summary>
    /// 利用可能な言語一覧.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<LanguageInfo> _availableLanguages = new();

    /// <summary>
    /// 選択された言語.
    /// </summary>
    [ObservableProperty]
    private LanguageInfo? _selectedLanguage;

    /// <summary>
    /// アプリケーション設定.
    /// </summary>
    [ObservableProperty]
    private AppSettings _appSettings = new();

    /// <summary>
    /// 視覚設定.
    /// </summary>
    [ObservableProperty]
    private VisualSettings _visualSettings = new();

    [ObservableProperty]
    private ObservableCollection<LogLevelInfo> _availableLogLevels = new();

    [ObservableProperty]
    private LogLevelInfo? _selectedLogLevel;

    [ObservableProperty]
    private LogSettings _logSettings = new();

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

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
    /// </summary>
    /// <param name="settingsService">設定サービス.</param>
    /// <param name="browserService">ブラウザサービス.</param>
    /// <param name="localizationService">ローカライゼーションサービス.</param>
    /// <param name="customLanguageService">カスタム言語サービス.</param>
    /// <param name="urlRuleService">URLルールサービス.</param>
    /// <param name="logService">ログサービス.</param>
    /// <param name="externalLinkService">
    /// 外部リンク（GitHubリポジトリ・Issues・リリース一覧、Phase E-2）を開くためのサービス。省略可（テスト互換のため）.
    /// 未指定の場合、Aboutセクションのリンクボタンは動作しない.
    /// </param>
    /// <param name="updateService">
    /// 「今すぐ確認」ボタン（Phase H-8）用のアップデートサービス。省略可（テスト互換のため）.
    /// 未指定の場合、「今すぐ確認」は失敗表示になる.
    /// </param>
    public SettingsViewModel(
        ISettingsService settingsService,
        IBrowserService browserService,
        ILocalizationService localizationService,
        ICustomLanguageService customLanguageService,
        IUrlRuleService urlRuleService,
        ILogService logService,
        IExternalLinkService? externalLinkService = null,
        IUpdateService? updateService = null)
    {
        _settingsService = settingsService;
        _browserService = browserService;
        _localizationService = localizationService;
        CustomLanguageService = customLanguageService;
        _urlRuleService = urlRuleService;
        LogService = logService;
        _externalLinkService = externalLinkService;
        _updateService = updateService;

        // 初期化処理（完了をInitializationTaskで外部から待機可能にする。
        // テストがコンストラクタ直後にコマンドを実行すると、この非同期初期化と
        // 競合しAppSettings/VisualSettingsが未読み込みのまま操作されうるため）
        InitializationTask = Task.Run(InitializeInternal);
    }

    /// <summary>
    /// 設定変更通知イベント
    /// </summary>
    public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    /// <summary>
    /// ブラウザ変更イベント.
    /// </summary>
    public event EventHandler<BrowserChangedEventArgs>? BrowserChanged;

    /// <summary>
    /// コンストラクタで開始したバックグラウンド初期化処理の完了を表す<see cref="Task"/>（テスト用）.
    /// </summary>
    public Task InitializationTask { get; }

    /// <summary>
    /// Gets the custom language service.
    /// </summary>
    public ICustomLanguageService CustomLanguageService { get; }

    /// <summary>
    /// Gets the log service.
    /// </summary>
    public ILogService LogService { get; }

    /// <summary>
    /// 初期化処理（テスト用）.
    /// </summary>
    public Task InitializeAsync()
    {
        return Task.Run(InitializeInternal);
    }

    /// <summary>
    /// 言語一覧を更新（外部から呼び出し可能）.
    /// </summary>
    /// <returns>representing the asynchronous operation.</returns>
    public async Task RefreshLanguagesAsync()
    {
        await InitializeLanguagesAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// 言語一覧を更新（同期版）.
    /// </summary>
    public void RefreshLanguages()
    {
        _ = Task.Run(InitializeLanguagesAsync);
    }

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

    /// <summary>
    /// 初期化処理.
    /// </summary>
    private async Task InitializeInternal()
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
            await InitializeLanguagesAsync().ConfigureAwait(false);
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

            // 言語リストを初期化
            await InitializeLanguagesAsync().ConfigureAwait(false);

            // プロパティ変更イベントを監視
            PropertyChanged += OnPropertyChanged;
            LogService?.LogDebug("プロパティ変更イベント監視開始", "SettingsViewModel");

            LogService?.LogDebug("SettingsViewModel初期化完了", "SettingsViewModel");
        }
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            LogService?.LogError($"設定画面の初期化エラー: {ex.Message}", "SettingsViewModel", ex);
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// 言語リストを初期化.
    /// </summary>
    private async Task InitializeLanguagesAsync()
    {
        try
        {
            // カスタム言語サービスから利用可能な言語を取得（ローカライズ不要の表示名）
            IEnumerable<Core.Models.LanguageInfo> availableLanguages = await CustomLanguageService.GetAvailableLanguagesAsync().ConfigureAwait(false);

            // UIスレッドでコレクションを更新
            Application.Current?.Dispatcher.Invoke(() =>
            {
                AvailableLanguages.Clear();

                foreach (Core.Models.LanguageInfo languageInfo in availableLanguages)
                {
                    AvailableLanguages.Add(new LanguageInfo(languageInfo.CultureCode, languageInfo.DisplayName));
                }

                // 現在の言語を選択
                SelectedLanguage = AvailableLanguages.FirstOrDefault(l => l.CultureCode == AppSettings.Language)
                                  ?? AvailableLanguages.First();
            });

            LogService?.LogDebug($"言語リスト初期化完了: {AvailableLanguages.Count}個の言語", "SettingsViewModel");
        }
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
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
        #pragma warning restore CA1031
    }

    /// <summary>
    /// ブラウザリストを更新.
    /// </summary>
    private async Task RefreshBrowsersAsync()
    {
        try
        {
            IEnumerable<Browser> browsers = await _browserService.GetAllBrowsersAsync().ConfigureAwait(false);

            // UIスレッドでコレクションを更新
            Application.Current?.Dispatcher.Invoke(() =>
            {
                DetectedBrowsers.Clear();
                foreach (Browser browser in browsers)
                {
                    DetectedBrowsers.Add(browser);
                }
            });
        }
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception)
        {
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// URLルールリストを更新.
    /// </summary>
    private async Task RefreshUrlRulesAsync()
    {
        try
        {
            IEnumerable<UrlRule> rules = await _urlRuleService.GetAllRulesAsync().ConfigureAwait(false);

            // UIスレッドでコレクションを更新
            Application.Current?.Dispatcher.Invoke(() =>
            {
                UrlRules.Clear();
                foreach (UrlRule rule in rules)
                {
                    UrlRules.Add(rule);
                }
            });
        }
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception)
        {
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// ログ設定の読み込み.
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
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception)
        {
            // エラー時はデフォルト設定を使用
            await RefreshLogSettingsAsync().ConfigureAwait(false);
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// ログ設定の初期化.
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
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception)
        {
        }
        #pragma warning restore CA1031

        return Task.CompletedTask;
    }

    /// <summary>
    /// ログレベルの初期化.
    /// </summary>
    private void InitializeLogLevels()
    {
        // UIスレッドでコレクションを更新
        Application.Current?.Dispatcher.Invoke(() =>
        {
            AvailableLogLevels.Clear();
            AvailableLogLevels.Add(new LogLevelInfo(LogLevel.Trace, LocalizedLogHelper.GetString("LogLevel.Trace")));
            AvailableLogLevels.Add(new LogLevelInfo(LogLevel.Debug, LocalizedLogHelper.GetString("LogLevel.Debug")));
            AvailableLogLevels.Add(new LogLevelInfo(LogLevel.Information, LocalizedLogHelper.GetString("LogLevel.Information")));
            AvailableLogLevels.Add(new LogLevelInfo(LogLevel.Warning, LocalizedLogHelper.GetString("LogLevel.Warning")));
            AvailableLogLevels.Add(new LogLevelInfo(LogLevel.Error, LocalizedLogHelper.GetString("LogLevel.Error")));
            AvailableLogLevels.Add(new LogLevelInfo(LogLevel.Critical, LocalizedLogHelper.GetString("LogLevel.Critical")));
        });
    }

    /// <summary>
    /// プロパティ変更時の処理.
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
    /// 視覚設定を更新.
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
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            LogService?.LogError($"視覚設定更新エラー: {ex.Message}", "SettingsViewModel", ex);
        }
        #pragma warning restore CA1031
    }



    /// <summary>
    /// 現在表示中のウィンドウへ視覚設定を即時適用.
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
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            LogService?.LogError($"視覚設定即時適用エラー: {ex.Message}", "SettingsViewModel", ex);
        }
        #pragma warning restore CA1031
    }

    #region Commands

    /// <summary>
    /// ブラウザを再検出するコマンド.
    /// </summary>
    [RelayCommand]
    private async Task RefreshBrowsers()
    {
        try
        {
            LogService?.LogInformation("ブラウザ再検出開始", "SettingsViewModel");

            // 明示的にブラウザ検出を実行
            IEnumerable<Browser> browsers = await _browserService.DetectBrowsersAsync().ConfigureAwait(false);

            // UIスレッドでコレクションを更新
            Application.Current?.Dispatcher.Invoke(() =>
            {
                DetectedBrowsers.Clear();
                foreach (Browser browser in browsers)
                {
                    DetectedBrowsers.Add(browser);
                }
            });

            LogService?.LogInformation($"ブラウザ再検出完了: {browsers.Count()}個のブラウザを検出", "SettingsViewModel");

            _ = LocalizedMessageBox.Show($"ブラウザ {browsers.Count()} 個を検出しました。", "完了");
        }
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            LogService?.LogError($"ブラウザ再検出エラー: {ex.Message}", "SettingsViewModel", ex);

            _ = LocalizedMessageBox.ShowError($"ブラウザの再検出中にエラーが発生しました: {ex.Message}");
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// ブラウザを追加するコマンド.
    /// </summary>
    [RelayCommand]
    private async Task AddBrowser()
    {
        try
        {
            LogService?.LogInformation("ブラウザ追加開始", "SettingsViewModel");

            Views.BrowserEditDialog dialog = new(null, LogService)
            {
                Owner = ActiveWindowLocator.GetActiveWindow()
            };
            if (dialog.ShowDialog() == true)
            {
                Browser newBrowser = dialog.Browser;

                if (DetectedBrowsers.Any(b =>
                    b.ExecutablePath.Equals(newBrowser.ExecutablePath, StringComparison.OrdinalIgnoreCase) &&
                    b.Arguments.Equals(newBrowser.Arguments, StringComparison.Ordinal)))
                {
                    LogService?.LogWarning($"ブラウザ追加失敗（実行ファイルパスと起動引数が重複）: {newBrowser.ExecutablePath} {newBrowser.Arguments}", "SettingsViewModel");
                    _ = LocalizedMessageBox.ShowError("同じ実行ファイル・起動引数の組み合わせのブラウザが既に登録されています。");
                    return;
                }

                newBrowser.DisplayOrder = DetectedBrowsers.Count + 1;

                bool result = await _browserService.AddBrowserAsync(newBrowser).ConfigureAwait(false);
                if (result)
                {
                    await RefreshBrowsersAsync().ConfigureAwait(false);
                    LogService?.LogInformation("ブラウザ追加完了", "SettingsViewModel");
                    _ = LocalizedMessageBox.Show("ブラウザを追加しました。", "完了");

                    // ブラウザ変更イベントを発生
                    BrowserChanged?.Invoke(this, new BrowserChangedEventArgs(newBrowser, "Added"));
                }
                else
                {
                    LogService?.LogWarning($"ブラウザ追加失敗: Name={newBrowser.Name}, ExecutablePath={newBrowser.ExecutablePath}", "SettingsViewModel");
                    _ = LocalizedMessageBox.ShowError("ブラウザの追加に失敗しました。実行ファイルのパスが正しいかご確認ください。");
                }
            }
        }
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            LogService?.LogError($"ブラウザ追加エラー: {ex.Message}", "SettingsViewModel", ex);
            _ = LocalizedMessageBox.ShowError($"ブラウザの追加中にエラーが発生しました: {ex.Message}");
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// URLルールを更新するコマンド.
    /// </summary>
    [RelayCommand]
    private async Task RefreshUrlRules()
    {
        await RefreshUrlRulesAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// URLルールを追加するコマンド.
    /// </summary>
    [RelayCommand]
    private async Task AddUrlRule()
    {
        try
        {
            LogService?.LogInformation("URLルール追加開始", "SettingsViewModel");

            Views.UrlRuleEditDialog dialog = new(_browserService, LogService!)
            {
                Owner = ActiveWindowLocator.GetActiveWindow()
            };
            if (dialog.ShowDialog() == true)
            {
                UrlRule newRule = dialog.UrlRule;
                bool result = await _urlRuleService.AddRuleAsync(newRule).ConfigureAwait(false);
                if (result)
                {
                    await RefreshUrlRulesAsync().ConfigureAwait(false);
                    LogService?.LogInformation("URLルール追加完了", "SettingsViewModel");
                    _ = LocalizedMessageBox.Show("URLルールを追加しました。", "完了");
                }
                else
                {
                    LogService?.LogWarning("URLルール追加失敗", "SettingsViewModel");
                    _ = LocalizedMessageBox.ShowError("URLルールの追加に失敗しました。", "エラー");
                }
            }
        }
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            LogService?.LogError($"URLルール追加エラー: {ex.Message}", "SettingsViewModel", ex);
            _ = LocalizedMessageBox.ShowError($"URLルールの追加中にエラーが発生しました: {ex.Message}", "エラー");
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// URLルールを編集するコマンド.
    /// </summary>
    [RelayCommand]
    private async Task EditUrlRule(UrlRule rule)
    {
        try
        {
            LogService?.LogInformation($"URLルール編集開始: {rule.Pattern}", "SettingsViewModel");

            Views.UrlRuleEditDialog dialog = new(rule, _browserService, LogService!)
            {
                Owner = ActiveWindowLocator.GetActiveWindow()
            };
            if (dialog.ShowDialog() == true)
            {
                UrlRule updatedRule = dialog.UrlRule;
                bool result = await _urlRuleService.UpdateRuleAsync(updatedRule).ConfigureAwait(false);
                if (result)
                {
                    await RefreshUrlRulesAsync().ConfigureAwait(false);
                    LogService?.LogInformation("URLルール編集完了", "SettingsViewModel");
                    _ = LocalizedMessageBox.Show("URLルールを更新しました。", "完了");
                }
                else
                {
                    LogService?.LogWarning("URLルール編集失敗", "SettingsViewModel");
                    _ = LocalizedMessageBox.ShowError("URLルールの更新に失敗しました。", "エラー");
                }
            }
        }
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            LogService?.LogError($"URLルール編集エラー: {ex.Message}", "SettingsViewModel", ex);
            _ = LocalizedMessageBox.ShowError($"URLルールの編集中にエラーが発生しました: {ex.Message}", "エラー");
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// URLルールを削除するコマンド.
    /// </summary>
    [RelayCommand]
    private async Task RemoveUrlRule(UrlRule rule)
    {
        try
        {
            LogService?.LogInformation($"URLルール削除開始: {rule.Pattern}", "SettingsViewModel");

            MessageBoxResult result = LocalizedMessageBox.ShowConfirm(
                $"URLルール「{rule.Pattern}」を削除しますか？",
                "削除確認");

            if (result == MessageBoxResult.Yes)
            {
                bool deleteResult = await _urlRuleService.DeleteRuleAsync(rule.Id).ConfigureAwait(false);
                if (deleteResult)
                {
                    await RefreshUrlRulesAsync().ConfigureAwait(false);
                    LogService?.LogInformation("URLルール削除完了", "SettingsViewModel");
                    _ = LocalizedMessageBox.Show("URLルールを削除しました。", "完了");
                }
                else
                {
                    LogService?.LogWarning("URLルール削除失敗", "SettingsViewModel");
                    _ = LocalizedMessageBox.ShowError("URLルールの削除に失敗しました。", "エラー");
                }
            }
        }
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            LogService?.LogError($"URLルール削除エラー: {ex.Message}", "SettingsViewModel", ex);
            _ = LocalizedMessageBox.ShowError($"URLルールの削除中にエラーが発生しました: {ex.Message}", "エラー");
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// URLルールをテストするコマンド.
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
            Uri testUrlUri = new(TestUrl);
            Browser? matchingBrowser = await _urlRuleService.FindMatchingBrowserAsync(testUrlUri, DetectedBrowsers).ConfigureAwait(false);
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
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            TestResult = LocalizedLogHelper.GetString("Settings.UrlRules.TestError", ex.Message);
            TestResultColor = Brushes.Red;
        }
        #pragma warning restore CA1031
    }



    /// <summary>
    /// 背景色を選択するコマンド.
    /// </summary>
    [RelayCommand]
    private void SelectBackgroundColor()
    {
        using System.Windows.Forms.ColorDialog colorDialog = new();
        if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            Color color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
            VisualSettings.BackgroundColor = color;
        }
    }

    /// <summary>
    /// グラデーション開始色を選択するコマンド.
    /// </summary>
    [RelayCommand]
    private void SelectGradientStartColor()
    {
        using System.Windows.Forms.ColorDialog colorDialog = new();
        if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            Color color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
            VisualSettings.GradientStartColor = color;
        }
    }

    /// <summary>
    /// グラデーション終了色を選択するコマンド.
    /// </summary>
    [RelayCommand]
    private void SelectGradientEndColor()
    {
        using System.Windows.Forms.ColorDialog colorDialog = new();
        if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            Color color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
            VisualSettings.GradientEndColor = color;
        }
    }



    /// <summary>
    /// フォーカス色を選択するコマンド.
    /// </summary>
    [RelayCommand]
    private void SelectFocusColor()
    {
        using System.Windows.Forms.ColorDialog colorDialog = new();
        if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            Color color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
            FocusColor = color;
        }
    }

    /// <summary>
    /// ブラウザボタン背景色を選択するコマンド.
    /// </summary>
    [RelayCommand]
    private void SelectBrowserButtonBackgroundColor()
    {
        using System.Windows.Forms.ColorDialog colorDialog = new();
        if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            Color color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
            VisualSettings.BrowserButtonBackgroundColor = color;
        }
    }

    /// <summary>
    /// ブラウザボタンテキスト色を選択するコマンド.
    /// </summary>
    [RelayCommand]
    private void SelectBrowserButtonForegroundColor()
    {
        using System.Windows.Forms.ColorDialog colorDialog = new();
        if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            Color color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
            VisualSettings.BrowserButtonForegroundColor = color;
        }
    }

    /// <summary>
    /// 設定をリセットするコマンド.
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
                await InitializeLanguagesAsync().ConfigureAwait(false);
            }
        }
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception)
        {
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// 設定をインポートするコマンド.
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
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            LogService?.LogError($"設定インポートエラー: {ex.Message}", "SettingsViewModel", ex);
            _ = LocalizedMessageBox.ShowError($"設定のインポート中にエラーが発生しました: {ex.Message}");
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// 設定をエクスポートするコマンド.
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
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            LogService?.LogError($"設定エクスポートエラー: {ex.Message}", "SettingsViewModel", ex);
            _ = LocalizedMessageBox.ShowError($"設定のエクスポート中にエラーが発生しました: {ex.Message}");
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// 設定を保存するコマンド.
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

            // null参照チェック
            if (AppSettings == null)
            {
                LogService?.LogError("AppSettingsがnullです", "SettingsViewModel");
                return;
            }

            if (VisualSettings == null)
            {
                LogService?.LogError("VisualSettingsがnullです", "SettingsViewModel");
                return;
            }

            if (LogSettings == null)
            {
                LogService?.LogError("LogSettingsがnullです", "SettingsViewModel");
                return;
            }

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

                // UIスレッドで設定変更通知とウィンドウ操作を実行
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        // 設定変更通知を送信
                        SettingsChanged?.Invoke(this, new SettingsChangedEventArgs("VisualSettings", null, VisualSettings));

                        // メイン画面へ反映
                        ApplyVisualToActiveWindow(VisualSettings);

                        LogService?.LogDebug("メイン画面への反映完了", "SettingsViewModel");

                        // 成功時はウィンドウを閉じる
                        Window? window = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this);
                        if (window != null)
                        {
                            LogService?.LogDebug($"設定ウィンドウを閉じる: {window.GetType().Name}", "SettingsViewModel");
                            window.DialogResult = true;
                            window.Close();
                        }
                        else
                        {
                            LogService?.LogWarning("設定ウィンドウが見つかりません", "SettingsViewModel");
                        }
                    }
                    // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
                    #pragma warning disable CA1031
                    catch (Exception ex)
                    {
                        LogService?.LogError($"UIスレッドでの処理エラー: {ex.Message}", "SettingsViewModel", ex);
                    }
                    #pragma warning restore CA1031
                });
            }
            else
            {
                LogService?.LogWarning($"設定保存に失敗: AppSettings={appSettingsResult}, VisualSettings={visualSettingsResult}, LogSettings={logSettingsResult}", "SettingsViewModel");
            }

            LogService?.LogDebug("SaveSettings完了", "SettingsViewModel");
        }
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            LogService?.LogError($"設定保存エラー: {ex.Message}", "SettingsViewModel", ex);
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// キャンセルコマンド.
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
    /// ログフォルダを選択するコマンド.
    /// </summary>
    [RelayCommand]
    private void SelectLogFolder()
    {
        try
        {
            using System.Windows.Forms.FolderBrowserDialog folderDialog = new()
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
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception)
        {
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// ログを表示するコマンド.
    /// </summary>
    [RelayCommand]
    private void ViewLogs()
    {
        try
        {
            string logContent = LogService.GetLogContent();
            Views.LogViewerWindow logWindow = new(logContent)
            {
                Owner = ActiveWindowLocator.GetActiveWindow()
            };
            _ = logWindow.ShowDialog();
        }
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception)
        {
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// ログをクリアするコマンド.
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
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception)
        {
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// 古いログを削除するコマンド.
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
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception)
        {
        }
        #pragma warning restore CA1031
    }

    #endregion



    /// <summary>
    /// ブラウザを編集するコマンド.
    /// </summary>
    [RelayCommand]
    private async Task EditBrowserAsync(Browser? browser = null)
    {
        try
        {
            Browser? targetBrowser = browser ?? SelectedBrowser;
            if (targetBrowser == null)
            {
                _ = LocalizedMessageBox.ShowWarning("編集するブラウザを選択してください。", "警告");
                return;
            }

            LogService?.LogInformation($"ブラウザ編集開始: {targetBrowser.Name}", "SettingsViewModel");

            Views.BrowserEditDialog dialog = new(targetBrowser, LogService)
            {
                Owner = ActiveWindowLocator.GetActiveWindow()
            };
            if (dialog.ShowDialog() == true)
            {
                Browser updatedBrowser = dialog.Browser;

                bool result = await _browserService.UpdateBrowserAsync(updatedBrowser).ConfigureAwait(false);
                if (result)
                {
                    await RefreshBrowsersAsync().ConfigureAwait(false);
                    LogService?.LogInformation($"ブラウザ編集完了: {updatedBrowser.Name}", "SettingsViewModel");
                    _ = LocalizedMessageBox.Show("ブラウザを更新しました。", "完了");

                    // ブラウザ変更イベントを発生
                    BrowserChanged?.Invoke(this, new BrowserChangedEventArgs(updatedBrowser, "Updated"));
                }
                else
                {
                    LogService?.LogWarning("ブラウザ更新失敗", "SettingsViewModel");
                    _ = LocalizedMessageBox.ShowError("ブラウザの更新に失敗しました。", "エラー");
                }
            }
        }
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            LogService?.LogError($"ブラウザ編集エラー: {ex.Message}", "SettingsViewModel", ex);
            _ = LocalizedMessageBox.ShowError($"ブラウザの編集中にエラーが発生しました: {ex.Message}", "エラー");
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// ブラウザを削除するコマンド.
    /// </summary>
    [RelayCommand]
    private async Task RemoveBrowserAsync(Browser? browser = null)
    {
        try
        {
            Browser? targetBrowser = browser ?? SelectedBrowser;
            if (targetBrowser == null)
            {
                _ = LocalizedMessageBox.ShowWarning("削除するブラウザを選択してください。", "警告");
                return;
            }

            LogService?.LogInformation($"ブラウザ削除開始: {targetBrowser.Name}", "SettingsViewModel");

            // システム検出ブラウザも削除可能にする
            // if (targetBrowser.Type != BrowserType.Custom)
            // {
            //     LocalizedMessageBox.ShowWarning("システムで検出されたブラウザは削除できません。", "警告");
            //     return;
            // }

            MessageBoxResult result = LocalizedMessageBox.ShowConfirm($"ブラウザ「{targetBrowser.Name}」を削除しますか？", "確認");

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
                    _ = LocalizedMessageBox.Show("ブラウザを削除しました。", "完了");

                    // ブラウザ変更イベントを発生
                    BrowserChanged?.Invoke(this, new BrowserChangedEventArgs(targetBrowser, "Removed"));
                }
                else
                {
                    LogService?.LogWarning("ブラウザ削除失敗", "SettingsViewModel");
                    _ = LocalizedMessageBox.ShowError("ブラウザの削除に失敗しました。", "エラー");
                }
            }
        }
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            LogService?.LogError($"ブラウザ削除エラー: {ex.Message}", "SettingsViewModel", ex);
            _ = LocalizedMessageBox.ShowError($"ブラウザの削除中にエラーが発生しました: {ex.Message}", "エラー");
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// ブラウザを上に移動するコマンド.
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
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            LogService?.LogError($"ブラウザ順序変更エラー: {ex.Message}", "SettingsViewModel", ex);
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// ブラウザを下に移動するコマンド.
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
        // CA1031: RelayCommandハンドラーの最上位try-catch。WPFダイアログ表示やサービス呼び出しなど例外種別が多岐にわたり、UIスレッドをクラッシュさせないための最終防御であるため意図的に汎用catchとする。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            LogService?.LogError($"ブラウザ順序変更エラー: {ex.Message}", "SettingsViewModel", ex);
        }
        #pragma warning restore CA1031
    }

    #endregion
}

/// <summary>
/// 言語情報を表すクラス.
/// </summary>
public class LanguageInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageInfo"/> class.
    /// </summary>
    /// <param name="cultureCode">カルチャーコード.</param>
    /// <param name="displayName">表示名.</param>
    public LanguageInfo(string cultureCode, string displayName)
    {
        CultureCode = cultureCode;
        DisplayName = displayName;
    }

    /// <summary>
    /// Gets or sets the culture code.
    /// </summary>
    public string CultureCode { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; }

}


/// <summary>
/// ログレベル情報を表すクラス.
/// </summary>
public class LogLevelInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogLevelInfo"/> class.
    /// </summary>
    /// <param name="logLevel">ログレベル.</param>
    /// <param name="displayName">表示名.</param>
    public LogLevelInfo(LogLevel logLevel, string displayName)
    {
        LogLevel = logLevel;
        DisplayName = displayName;
    }

    /// <summary>
    /// Gets or sets the log level.
    /// </summary>
    public LogLevel LogLevel { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; }

}
