using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Core.Enums;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;

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
    private readonly ILogService _logService;

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
        IUrlRuleService urlRuleService,
        ILogService logService)
    {
        _settingsService = settingsService;
        _browserService = browserService;
        _localizationService = localizationService;
        _urlRuleService = urlRuleService;
        _logService = logService;



        InitializeAsync();
    }

    /// <summary>
    /// 初期化処理
    /// </summary>
    private async void InitializeAsync()
    {
        try
        {
            _logService?.LogDebug("SettingsViewModel初期化開始", "SettingsViewModel");
            
            // 設定を読み込み
            AppSettings = await _settingsService.LoadAppSettingsAsync();
            _logService?.LogDebug($"AppSettings読み込み完了: Language={AppSettings.Language}", "SettingsViewModel");
            
            VisualSettings = await _settingsService.LoadVisualSettingsAsync();
            
            // グラデーション設定の初期化（デフォルト値の設定）
            _logService?.LogDebug($"初期化前のGradientDirection: {VisualSettings.GradientDirection}", "SettingsViewModel");
            
            if (VisualSettings.UseBackgroundGradient)
            {
                _logService?.LogDebug("グラデーションが有効です。初期値を設定します。", "SettingsViewModel");
                
                if (VisualSettings.GradientStartColor == Colors.Transparent)
                {
                    VisualSettings.GradientStartColor = Colors.White;
                    _logService?.LogDebug("グラデーション開始色をWhiteに設定", "SettingsViewModel");
                }
                if (VisualSettings.GradientEndColor == Colors.Transparent)
                {
                    VisualSettings.GradientEndColor = Colors.LightGray;
                    _logService?.LogDebug("グラデーション終了色をLightGrayに設定", "SettingsViewModel");
                }
                // グラデーション方向の初期値を確実に設定
                if (VisualSettings.GradientDirection == 0)
                {
                    VisualSettings.GradientDirection = BrowserSelector.Core.Enums.GradientDirection.Vertical;
                    _logService?.LogDebug("グラデーション方向をVerticalに設定（初期値0から変更）", "SettingsViewModel");
                }
                else
                {
                    _logService?.LogDebug($"グラデーション方向は既に設定済み: {VisualSettings.GradientDirection}", "SettingsViewModel");
                }
            }
            else
            {
                _logService?.LogDebug("グラデーションは無効です", "SettingsViewModel");
            }
            
            _logService?.LogDebug($"VisualSettings読み込み完了: BackgroundColor={VisualSettings.BackgroundColor}, UseBackgroundGradient={VisualSettings.UseBackgroundGradient}, GradientDirection={VisualSettings.GradientDirection}", "SettingsViewModel");

            // 言語リストを初期化
            InitializeLanguages();
            _logService?.LogDebug("言語リスト初期化完了", "SettingsViewModel");

            // ブラウザリストを更新
            await RefreshBrowsersAsync();
            _logService?.LogDebug("ブラウザリスト更新完了", "SettingsViewModel");

            // URLルールリストを更新
            await RefreshUrlRulesAsync();
            _logService?.LogDebug("URLルールリスト更新完了", "SettingsViewModel");
            
            // ログレベルの初期化（先に実行）
            InitializeLogLevels();
            _logService?.LogDebug("ログレベル初期化完了", "SettingsViewModel");
            
            // ログ設定の読み込み
            await LoadLogSettingsAsync();
            _logService?.LogDebug("ログ設定読み込み完了", "SettingsViewModel");

            // プロパティ変更イベントを監視
            PropertyChanged += OnPropertyChanged;
            _logService?.LogDebug("プロパティ変更イベント監視開始", "SettingsViewModel");
            
            _logService?.LogDebug("SettingsViewModel初期化完了", "SettingsViewModel");
        }
        catch (Exception ex)
        {
            _logService?.LogError($"設定画面の初期化エラー: {ex.Message}", "SettingsViewModel", ex);
        }
    }

    /// <summary>
    /// 言語リストを初期化
    /// </summary>
    private void InitializeLanguages()
    {
        AvailableLanguages.Clear();
        AvailableLanguages.Add(new LanguageInfo("ja-JP", "日本語"));
        AvailableLanguages.Add(new LanguageInfo("en-US", "English"));

        // 現在の言語を選択
        SelectedLanguage = AvailableLanguages.FirstOrDefault(l => l.CultureCode == AppSettings.Language) 
                          ?? AvailableLanguages.First();
    }

    /// <summary>
    /// ブラウザリストを更新
    /// </summary>
    private async Task RefreshBrowsersAsync()
    {
        try
        {
            var browsers = await _browserService.GetAllBrowsersAsync();
            DetectedBrowsers.Clear();
            foreach (var browser in browsers)
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
            var rules = await _urlRuleService.GetAllRulesAsync();
            UrlRules.Clear();
            foreach (var rule in rules)
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
            LogSettings = await _settingsService.LoadLogSettingsAsync();
            
            // 現在のログファイルパスを設定
            CurrentLogFilePath = _logService.GetLogFilePath();
            
            // ログサービスの設定を更新
            _logService.UpdateSettings(LogSettings);
            
            // 選択されたログレベルを設定
            SelectedLogLevel = AvailableLogLevels.FirstOrDefault(l => l.LogLevel == LogSettings.LogLevel);
            
            // デバッグ情報を出力
        }
        catch (Exception)
        {
            // エラー時はデフォルト設定を使用
            await RefreshLogSettingsAsync();
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
            CurrentLogFilePath = _logService.GetLogFilePath();
            
            // ログサービスの設定を更新
            _logService.UpdateSettings(LogSettings);
            
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
        AvailableLogLevels.Add(new LogLevelInfo(LogLevel.Trace, "トレース"));
        AvailableLogLevels.Add(new LogLevelInfo(LogLevel.Debug, "デバッグ"));
        AvailableLogLevels.Add(new LogLevelInfo(LogLevel.Information, "情報"));
        AvailableLogLevels.Add(new LogLevelInfo(LogLevel.Warning, "警告"));
        AvailableLogLevels.Add(new LogLevelInfo(LogLevel.Error, "エラー"));
        AvailableLogLevels.Add(new LogLevelInfo(LogLevel.Critical, "致命的エラー"));
    }

    /// <summary>
    /// プロパティ変更時の処理
    /// </summary>
    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // リアルタイムプレビューの更新
        if (e.PropertyName == nameof(VisualSettings) || 
            e.PropertyName == "VisualSettings.BackgroundColor" ||
            e.PropertyName == "VisualSettings.UseBackgroundGradient" ||
            e.PropertyName == "VisualSettings.GradientStartColor" ||
            e.PropertyName == "VisualSettings.GradientEndColor" ||
            e.PropertyName == "VisualSettings.GradientDirection")
        {
            _logService?.LogDebug($"視覚設定プロパティ変更検知: {e.PropertyName}", "SettingsViewModel");
            
            // グラデーション方向の変更を詳細にログ出力
            if (e.PropertyName == "VisualSettings.GradientDirection")
            {
                _logService?.LogDebug($"グラデーション方向が変更されました: {VisualSettings.GradientDirection}", "SettingsViewModel");
            }
            
            // グラデーションチェックボックスが有効になった時のデフォルト値設定
            if (e.PropertyName == "VisualSettings.UseBackgroundGradient" && VisualSettings.UseBackgroundGradient)
            {
                _logService?.LogDebug("グラデーションチェックボックスが有効になりました。デフォルト値を設定します。", "SettingsViewModel");
                
                if (VisualSettings.GradientStartColor == Colors.Transparent)
                {
                    VisualSettings.GradientStartColor = Colors.White;
                    _logService?.LogDebug("グラデーション開始色をWhiteに設定", "SettingsViewModel");
                }
                if (VisualSettings.GradientEndColor == Colors.Transparent)
                {
                    VisualSettings.GradientEndColor = Colors.LightGray;
                    _logService?.LogDebug("グラデーション終了色をLightGrayに設定", "SettingsViewModel");
                }
                if (VisualSettings.GradientDirection == 0) // デフォルト値
                {
                    VisualSettings.GradientDirection = BrowserSelector.Core.Enums.GradientDirection.Vertical;
                    _logService?.LogDebug("グラデーション方向をVerticalに設定", "SettingsViewModel");
                }
                else
                {
                    _logService?.LogDebug($"グラデーション方向は既に設定済み: {VisualSettings.GradientDirection}", "SettingsViewModel");
                }
            }
            
            await UpdateVisualSettingsAsync();
        }

        // 言語変更時の処理
        if (e.PropertyName == nameof(SelectedLanguage) && SelectedLanguage != null)
        {
            AppSettings.Language = SelectedLanguage.CultureCode;
            _localizationService.SetLanguage(new System.Globalization.CultureInfo(SelectedLanguage.CultureCode));
        }
        
        // ログ設定の変更時の処理
        if (e.PropertyName == nameof(SelectedLogLevel) && SelectedLogLevel != null)
        {
            LogSettings.LogLevel = SelectedLogLevel.LogLevel;
            _logService?.UpdateSettings(LogSettings);
            _logService?.LogInformation($"ログレベルが変更されました: {SelectedLogLevel.DisplayName}", "SettingsViewModel");
        }
    }

    /// <summary>
    /// 視覚設定を更新
    /// </summary>
    private async Task UpdateVisualSettingsAsync()
    {
        try
        {
            _logService?.LogDebug("UpdateVisualSettingsAsync開始", "SettingsViewModel");
            _logService?.LogDebug($"保存対象: UseBackgroundGradient={VisualSettings.UseBackgroundGradient}, GradientDirection={VisualSettings.GradientDirection}, StartColor={VisualSettings.GradientStartColor}, EndColor={VisualSettings.GradientEndColor}", "SettingsViewModel");
            
            // グラデーション方向の詳細ログ
            if (VisualSettings.UseBackgroundGradient)
            {
                _logService?.LogDebug($"グラデーション設定詳細: 方向={VisualSettings.GradientDirection} (値={Convert.ToInt32(VisualSettings.GradientDirection)})", "SettingsViewModel");
            }
            
            var result = await _settingsService.SaveVisualSettingsAsync(VisualSettings);
            _logService?.LogDebug($"設定保存結果: {result}", "SettingsViewModel");
            
            if (result)
            {
                ApplyVisualToActiveWindow(VisualSettings);
                _logService?.LogDebug("視覚設定の適用完了", "SettingsViewModel");
            }
            else
            {
                _logService?.LogWarning("設定の保存に失敗しました", "SettingsViewModel");
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"視覚設定更新エラー: {ex.Message}", "SettingsViewModel", ex);
        }
    }



    /// <summary>
    /// 現在表示中のウィンドウへ視覚設定を即時適用
    /// </summary>
    private void ApplyVisualToActiveWindow(VisualSettings settings)
    {
        try
        {
            _logService?.LogDebug("ApplyVisualToActiveWindow開始", "SettingsViewModel");
            _logService?.LogDebug($"受領値: BackgroundColor={settings.BackgroundColor}", "SettingsViewModel");
            
            var window = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                         ?? Application.Current?.MainWindow;
            if (window == null) 
            {
                _logService?.LogDebug("適用対象のウィンドウが見つかりません", "SettingsViewModel");
                return;
            }
            
            _logService?.LogDebug($"適用対象ウィンドウ: {window.GetType().Name}, Title={window.Title}", "SettingsViewModel");

            // 適用前の値を記録
            var beforeBackground = window.Background;
            _logService?.LogDebug($"適用前の背景: {beforeBackground}", "SettingsViewModel");



            // 背景色 / グラデーション
            if (settings.UseBackgroundGradient)
            {
                _logService?.LogDebug($"グラデーション設定開始: 方向={settings.GradientDirection}, 開始色={settings.GradientStartColor}, 終了色={settings.GradientEndColor}", "SettingsViewModel");
                
                // グラデーション方向に応じてStartPointとEndPointを設定
                System.Windows.Point startPoint, endPoint;
                switch (settings.GradientDirection)
                {
                    case BrowserSelector.Core.Enums.GradientDirection.Horizontal:
                        startPoint = new System.Windows.Point(0, 0);
                        endPoint = new System.Windows.Point(1, 0);
                        _logService?.LogDebug("水平方向グラデーションを設定", "SettingsViewModel");
                        break;
                    case BrowserSelector.Core.Enums.GradientDirection.Diagonal:
                        startPoint = new System.Windows.Point(0, 0);
                        endPoint = new System.Windows.Point(1, 1);
                        _logService?.LogDebug("斜め方向グラデーションを設定", "SettingsViewModel");
                        break;
                    default: // Vertical
                        startPoint = new System.Windows.Point(0, 0);
                        endPoint = new System.Windows.Point(0, 1);
                        _logService?.LogDebug("垂直方向グラデーションを設定", "SettingsViewModel");
                        break;
                }
                
                var gradientBrush = new LinearGradientBrush
                {
                    StartPoint = startPoint,
                    EndPoint = endPoint,
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(settings.GradientStartColor, 0),
                        new GradientStop(settings.GradientEndColor, 1)
                    }
                };
                
                window.Background = gradientBrush;
                _logService?.LogDebug($"背景グラデーション設定完了: 方向={settings.GradientDirection}, 開始色={settings.GradientStartColor}, 終了色={settings.GradientEndColor}, 適用後={window.Background}", "SettingsViewModel");
            }
            else
            {
                _logService?.LogDebug($"背景色設定開始: 設定値={settings.BackgroundColor}", "SettingsViewModel");
                
                var newBrush = new SolidColorBrush(settings.BackgroundColor);
                window.Background = newBrush;
                
                _logService?.LogDebug($"背景色設定完了: 設定値={settings.BackgroundColor}, 適用後={window.Background}", "SettingsViewModel");
            }


            
            _logService?.LogDebug("ApplyVisualToActiveWindow完了", "SettingsViewModel");
        }
        catch (Exception ex)
        {
            _logService?.LogError($"視覚設定即時適用エラー: {ex.Message}", "SettingsViewModel", ex);
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
            _logService?.LogInformation("ブラウザ再検出開始", "SettingsViewModel");
            
            // 明示的にブラウザ検出を実行
            var browsers = await _browserService.DetectBrowsersAsync();
            DetectedBrowsers.Clear();
            foreach (var browser in browsers)
            {
                DetectedBrowsers.Add(browser);
            }
            
            _logService?.LogInformation($"ブラウザ再検出完了: {browsers.Count()}個のブラウザを検出", "SettingsViewModel");
            MessageBox.Show($"ブラウザ {browsers.Count()} 個を検出しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logService?.LogError($"ブラウザ再検出エラー: {ex.Message}", "SettingsViewModel", ex);
            MessageBox.Show($"ブラウザの再検出中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
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
            _logService?.LogInformation("ブラウザ追加開始", "SettingsViewModel");
            
            var dialog = new Views.BrowserEditDialog();
            if (dialog.ShowDialog() == true)
            {
                var newBrowser = dialog.Browser;
                newBrowser.DisplayOrder = DetectedBrowsers.Count + 1;

                var result = await _browserService.AddBrowserAsync(newBrowser);
                if (result)
                {
                    await RefreshBrowsersAsync();
                    _logService?.LogInformation("ブラウザ追加完了", "SettingsViewModel");
                    MessageBox.Show("ブラウザを追加しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _logService?.LogWarning("ブラウザ追加失敗", "SettingsViewModel");
                    MessageBox.Show("ブラウザの追加に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"ブラウザ追加エラー: {ex.Message}", "SettingsViewModel", ex);
            MessageBox.Show($"ブラウザの追加中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// URLルールを更新するコマンド
    /// </summary>
    [RelayCommand]
    private async Task RefreshUrlRules()
    {
        await RefreshUrlRulesAsync();
    }

    /// <summary>
    /// URLルールを追加するコマンド
    /// </summary>
    [RelayCommand]
    private async Task AddUrlRule()
    {
        try
        {
            _logService?.LogInformation("URLルール追加開始", "SettingsViewModel");
            
            var dialog = new Views.UrlRuleEditDialog(_browserService, _logService!);
            if (dialog.ShowDialog() == true)
            {
                var newRule = dialog.UrlRule;
                var result = await _urlRuleService.AddRuleAsync(newRule);
                if (result)
                {
                    await RefreshUrlRulesAsync();
                    _logService?.LogInformation("URLルール追加完了", "SettingsViewModel");
                    MessageBox.Show("URLルールを追加しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _logService?.LogWarning("URLルール追加失敗", "SettingsViewModel");
                    MessageBox.Show("URLルールの追加に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"URLルール追加エラー: {ex.Message}", "SettingsViewModel", ex);
            MessageBox.Show($"URLルールの追加中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
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
            _logService?.LogInformation($"URLルール編集開始: {rule.Pattern}", "SettingsViewModel");
            
            var dialog = new Views.UrlRuleEditDialog(rule, _browserService, _logService!);
            if (dialog.ShowDialog() == true)
            {
                var updatedRule = dialog.UrlRule;
                var result = await _urlRuleService.UpdateRuleAsync(updatedRule);
                if (result)
                {
                    await RefreshUrlRulesAsync();
                    _logService?.LogInformation("URLルール編集完了", "SettingsViewModel");
                    MessageBox.Show("URLルールを更新しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _logService?.LogWarning("URLルール編集失敗", "SettingsViewModel");
                    MessageBox.Show("URLルールの更新に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"URLルール編集エラー: {ex.Message}", "SettingsViewModel", ex);
            MessageBox.Show($"URLルールの編集中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
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
            _logService?.LogInformation($"URLルール削除開始: {rule.Pattern}", "SettingsViewModel");
            
            var result = MessageBox.Show(
                $"URLルール「{rule.Pattern}」を削除しますか？", 
                "削除確認", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                var deleteResult = await _urlRuleService.DeleteRuleAsync(rule.Id);
                if (deleteResult)
                {
                    await RefreshUrlRulesAsync();
                    _logService?.LogInformation("URLルール削除完了", "SettingsViewModel");
                    MessageBox.Show("URLルールを削除しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _logService?.LogWarning("URLルール削除失敗", "SettingsViewModel");
                    MessageBox.Show("URLルールの削除に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"URLルール削除エラー: {ex.Message}", "SettingsViewModel", ex);
            MessageBox.Show($"URLルールの削除中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
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
            TestResult = "テストURLを入力してください";
            TestResultColor = Brushes.Red;
            return;
        }

        try
        {
            var matchingBrowser = await _urlRuleService.FindMatchingBrowserAsync(TestUrl, DetectedBrowsers);
            if (matchingBrowser != null)
            {
                TestResult = $"マッチ: {matchingBrowser.Name}";
                TestResultColor = Brushes.Green;
            }
            else
            {
                TestResult = "マッチするルールなし";
                TestResultColor = Brushes.Orange;
            }
        }
        catch (Exception ex)
        {
            TestResult = $"エラー: {ex.Message}";
            TestResultColor = Brushes.Red;
        }
    }



    /// <summary>
    /// 背景色を選択するコマンド
    /// </summary>
    [RelayCommand]
    private void SelectBackgroundColor()
    {
        var colorDialog = new System.Windows.Forms.ColorDialog();
        if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
            VisualSettings.BackgroundColor = color;
        }
    }

    /// <summary>
    /// グラデーション開始色を選択するコマンド
    /// </summary>
    [RelayCommand]
    private void SelectGradientStartColor()
    {
        var colorDialog = new System.Windows.Forms.ColorDialog();
        if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
            VisualSettings.GradientStartColor = color;
        }
    }

    /// <summary>
    /// グラデーション終了色を選択するコマンド
    /// </summary>
    [RelayCommand]
    private void SelectGradientEndColor()
    {
        var colorDialog = new System.Windows.Forms.ColorDialog();
        if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
            VisualSettings.GradientEndColor = color;
        }
    }



    /// <summary>
    /// フォーカス色を選択するコマンド
    /// </summary>
    [RelayCommand]
    private void SelectFocusColor()
    {
        var colorDialog = new System.Windows.Forms.ColorDialog();
        if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
            FocusColor = color;
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
            var result = await _settingsService.ResetSettingsAsync();
            if (result)
            {
                // 設定を再読み込み
                AppSettings = await _settingsService.LoadAppSettingsAsync();
                VisualSettings = await _settingsService.LoadVisualSettingsAsync();
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
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "設定ファイルを選択"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var result = await _settingsService.ImportSettingsAsync(openFileDialog.FileName);
                if (result)
                {
                    // 設定を再読み込み
                    AppSettings = await _settingsService.LoadAppSettingsAsync();
                    VisualSettings = await _settingsService.LoadVisualSettingsAsync();
                    InitializeLanguages();
                }
            }
        }
        catch (Exception)
        {
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
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "設定ファイルを保存",
                FileName = $"browserselector_settings_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var exportData = new
                {
                    AppSettings = AppSettings,
                    VisualSettings = VisualSettings,
                    ExportDate = DateTime.Now
                };

                var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(saveFileDialog.FileName, json);
            }
        }
        catch (Exception)
        {
        }
    }

    /// <summary>
    /// 設定を保存するコマンド
    /// </summary>
    [RelayCommand]
    private async Task SaveSettings()
    {
        _logService?.LogDebug("SaveSettingsコマンド実行開始", "SettingsViewModel");
        await SaveSettingsInternal();
    }

    private async Task SaveSettingsInternal()
    {
        try
        {
            _logService?.LogDebug("SaveSettings開始", "SettingsViewModel");
            _logService?.LogDebug($"保存対象VisualSettings: BackgroundColor={VisualSettings.BackgroundColor}", "SettingsViewModel");
            
            // アプリケーション設定を保存
            var appSettingsResult = await _settingsService.SaveAppSettingsAsync(AppSettings);
            _logService?.LogDebug($"AppSettings保存結果: {appSettingsResult}", "SettingsViewModel");
            
            // 視覚設定を保存
            var visualSettingsResult = await _settingsService.SaveVisualSettingsAsync(VisualSettings);
            _logService?.LogDebug($"VisualSettings保存結果: {visualSettingsResult}", "SettingsViewModel");
            
            // ログ設定を保存
            var logSettingsResult = await _settingsService.SaveLogSettingsAsync(LogSettings);
            _logService?.LogDebug($"LogSettings保存結果: {logSettingsResult}", "SettingsViewModel");

            if (appSettingsResult && visualSettingsResult && logSettingsResult)
            {
                _logService?.LogDebug("設定保存成功、メイン画面への反映開始", "SettingsViewModel");
                
                // 設定変更通知を送信
                SettingsChanged?.Invoke(this, new SettingsChangedEventArgs("VisualSettings", null, VisualSettings));
                
                // メイン画面へ反映
                ApplyVisualToActiveWindow(VisualSettings);
                
                _logService?.LogDebug("メイン画面への反映完了", "SettingsViewModel");

                // 成功時はウィンドウを閉じる
                if (Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this) is Window window)
                {
                    _logService?.LogDebug($"設定ウィンドウを閉じる: {window.GetType().Name}", "SettingsViewModel");
                    window.DialogResult = true;
                    window.Close();
                }
            }
            else
            {
                _logService?.LogWarning($"設定保存に失敗: AppSettings={appSettingsResult}, VisualSettings={visualSettingsResult}", "SettingsViewModel");
            }
            
            _logService?.LogDebug("SaveSettings完了", "SettingsViewModel");
        }
        catch (Exception ex)
        {
            _logService?.LogError($"設定保存エラー: {ex.Message}", "SettingsViewModel", ex);
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
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "ログ出力フォルダを選択してください",
                SelectedPath = LogSettings.LogOutputFolder
            };

            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LogSettings.LogOutputFolder = folderDialog.SelectedPath;
                CurrentLogFilePath = _logService.GetLogFilePath();
                _logService.UpdateSettings(LogSettings);
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
            var logContent = _logService.GetLogContent();
            var logWindow = new Views.LogViewerWindow(logContent);
            logWindow.ShowDialog();
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
            var result = MessageBox.Show("ログファイルをクリアしますか？", "確認", 
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _logService.ClearLogs();
                CurrentLogFilePath = _logService.GetLogFilePath();
                MessageBox.Show("ログファイルをクリアしました。", "完了", 
                              MessageBoxButton.OK, MessageBoxImage.Information);
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
            var result = MessageBox.Show("古いログファイルを削除しますか？", "確認", 
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _logService.CleanupOldLogs();
                MessageBox.Show("古いログファイルを削除しました。", "完了", 
                              MessageBoxButton.OK, MessageBoxImage.Information);
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
            var targetBrowser = browser ?? SelectedBrowser;
            if (targetBrowser == null)
            {
                MessageBox.Show("編集するブラウザを選択してください。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _logService?.LogInformation($"ブラウザ編集開始: {targetBrowser.Name}", "SettingsViewModel");
            
            // システムで検出されたブラウザは、順序・アイコン・パラメーターのみ編集可能
            if (targetBrowser.Type != BrowserType.Custom)
            {
                // システムブラウザの場合は、編集可能な項目を制限
                var limitedBrowser = new Browser
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
                
                var systemBrowserDialog = new Views.BrowserEditDialog(limitedBrowser, true); // システムブラウザフラグ
                if (systemBrowserDialog.ShowDialog() == true)
                {
                    var updatedBrowser = systemBrowserDialog.Browser;
                    
                    // システムブラウザの場合は、編集可能な項目のみ更新
                    targetBrowser.DisplayOrder = updatedBrowser.DisplayOrder;
                    targetBrowser.IconPath = updatedBrowser.IconPath;
                    targetBrowser.Arguments = updatedBrowser.Arguments;
                    targetBrowser.IsEnabled = updatedBrowser.IsEnabled;
                    
                    var result = await _browserService.UpdateBrowserAsync(targetBrowser);
                    if (result)
                    {
                        await RefreshBrowsersAsync();
                        _logService?.LogInformation($"システムブラウザ編集完了: {updatedBrowser.Name}", "SettingsViewModel");
                        MessageBox.Show("ブラウザ設定を更新しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        _logService?.LogWarning("システムブラウザ更新失敗", "SettingsViewModel");
                        MessageBox.Show("ブラウザ設定の更新に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                return;
            }

            var dialog = new Views.BrowserEditDialog(targetBrowser);
            if (dialog.ShowDialog() == true)
            {
                var updatedBrowser = dialog.Browser;
                
                var result = await _browserService.UpdateBrowserAsync(updatedBrowser);
                if (result)
                {
                    await RefreshBrowsersAsync();
                    _logService?.LogInformation($"ブラウザ編集完了: {updatedBrowser.Name}", "SettingsViewModel");
                    MessageBox.Show("ブラウザを更新しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _logService?.LogWarning("ブラウザ更新失敗", "SettingsViewModel");
                    MessageBox.Show("ブラウザの更新に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"ブラウザ編集エラー: {ex.Message}", "SettingsViewModel", ex);
            MessageBox.Show($"ブラウザの編集中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
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
            var targetBrowser = browser ?? SelectedBrowser;
            if (targetBrowser == null)
            {
                MessageBox.Show("削除するブラウザを選択してください。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _logService?.LogInformation($"ブラウザ削除開始: {targetBrowser.Name}", "SettingsViewModel");
            
            // システム検出ブラウザも削除可能にする
            // if (targetBrowser.Type != BrowserType.Custom)
            // {
            //     MessageBox.Show("システムで検出されたブラウザは削除できません。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            //     return;
            // }

            var result = MessageBox.Show($"ブラウザ「{targetBrowser.Name}」を削除しますか？", "確認", 
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                var deleteResult = await _browserService.RemoveBrowserAsync(targetBrowser.Id);
                if (deleteResult)
                {
                    await RefreshBrowsersAsync();
                    if (SelectedBrowser == targetBrowser)
                    {
                        SelectedBrowser = null;
                    }
                    _logService?.LogInformation($"ブラウザ削除完了: {targetBrowser.Name}", "SettingsViewModel");
                    MessageBox.Show("ブラウザを削除しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _logService?.LogWarning("ブラウザ削除失敗", "SettingsViewModel");
                    MessageBox.Show("ブラウザの削除に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"ブラウザ削除エラー: {ex.Message}", "SettingsViewModel", ex);
            MessageBox.Show($"ブラウザの削除中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
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
            var currentIndex = DetectedBrowsers.IndexOf(browser);
            if (currentIndex > 0)
            {
                var previousBrowser = DetectedBrowsers[currentIndex - 1];
                
                // DisplayOrderを交換
                var tempOrder = browser.DisplayOrder;
                browser.DisplayOrder = previousBrowser.DisplayOrder;
                previousBrowser.DisplayOrder = tempOrder;
                
                // コレクション内の順序を更新
                DetectedBrowsers.Move(currentIndex, currentIndex - 1);
                
                // サービスに保存
                await _browserService.UpdateBrowserAsync(browser);
                await _browserService.UpdateBrowserAsync(previousBrowser);
                
                _logService?.LogInformation($"ブラウザ順序変更: {browser.Name} を上に移動", "SettingsViewModel");
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"ブラウザ順序変更エラー: {ex.Message}", "SettingsViewModel", ex);
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
            var currentIndex = DetectedBrowsers.IndexOf(browser);
            if (currentIndex < DetectedBrowsers.Count - 1)
            {
                var nextBrowser = DetectedBrowsers[currentIndex + 1];
                
                // DisplayOrderを交換
                var tempOrder = browser.DisplayOrder;
                browser.DisplayOrder = nextBrowser.DisplayOrder;
                nextBrowser.DisplayOrder = tempOrder;
                
                // コレクション内の順序を更新
                DetectedBrowsers.Move(currentIndex, currentIndex + 1);
                
                // サービスに保存
                await _browserService.UpdateBrowserAsync(browser);
                await _browserService.UpdateBrowserAsync(nextBrowser);
                
                _logService?.LogInformation($"ブラウザ順序変更: {browser.Name} を下に移動", "SettingsViewModel");
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"ブラウザ順序変更エラー: {ex.Message}", "SettingsViewModel", ex);
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
