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

    [ObservableProperty]
    private bool _showFocusIndicator = true;

    [ObservableProperty]
    private Color _focusColor = Colors.Blue;

    [ObservableProperty]
    private double _focusThickness = 2.0;

    [ObservableProperty]
    private bool _enableKeyboardNavigation = true;

    [ObservableProperty]
    private bool _enableShortcuts = true;

    [ObservableProperty]
    private bool _enableScreenReaderSupport = true;

    [ObservableProperty]
    private bool _provideDetailedDescriptions = true;

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
            // 設定を読み込み
            AppSettings = await _settingsService.LoadAppSettingsAsync();
            VisualSettings = await _settingsService.LoadVisualSettingsAsync();

            // 言語リストを初期化
            InitializeLanguages();

            // ブラウザリストを更新
            await RefreshBrowsersAsync();

            // URLルールリストを更新
            await RefreshUrlRulesAsync();
            
            // ログ設定の初期化
            await RefreshLogSettingsAsync();
            
            // ログレベルの初期化
            InitializeLogLevels();

            // プロパティ変更イベントを監視
            PropertyChanged += OnPropertyChanged;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"設定画面の初期化エラー: {ex.Message}");
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ブラウザリスト更新エラー: {ex.Message}");
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"URLルールリスト更新エラー: {ex.Message}");
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
            LogSettings.LogLevel = LogLevel.Information;
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
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ログ設定初期化エラー: {ex.Message}");
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
            e.PropertyName == "VisualSettings.Opacity" ||
            e.PropertyName == "VisualSettings.TransparencyColor" ||
            e.PropertyName == "VisualSettings.CornerRadius" ||
            e.PropertyName == "VisualSettings.BackgroundColor" ||
            e.PropertyName == "VisualSettings.UseBackgroundGradient")
        {
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
            await _settingsService.SaveVisualSettingsAsync(VisualSettings);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"視覚設定更新エラー: {ex.Message}");
        }
    }

    #region Commands

    /// <summary>
    /// ブラウザを再検出するコマンド
    /// </summary>
    [RelayCommand]
    private async Task RefreshBrowsers()
    {
        await RefreshBrowsersAsync();
    }

    /// <summary>
    /// ブラウザを追加するコマンド
    /// </summary>
    [RelayCommand]
    private Task AddBrowser()
    {
        // TODO: ブラウザ追加ダイアログを実装
        System.Diagnostics.Debug.WriteLine("ブラウザ追加機能は未実装です");
        return Task.CompletedTask;
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
    private Task AddUrlRule()
    {
        // TODO: URLルール追加ダイアログを実装
        System.Diagnostics.Debug.WriteLine("URLルール追加機能は未実装です");
        return Task.CompletedTask;
    }

    /// <summary>
    /// URLルールを編集するコマンド
    /// </summary>
    [RelayCommand]
    private Task EditUrlRule(UrlRule rule)
    {
        // TODO: URLルール編集ダイアログを実装
        System.Diagnostics.Debug.WriteLine($"URLルール編集機能は未実装です: {rule.Pattern}");
        return Task.CompletedTask;
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
    /// 透明化色を選択するコマンド
    /// </summary>
    [RelayCommand]
    private void SelectTransparencyColor()
    {
        var colorDialog = new System.Windows.Forms.ColorDialog();
        if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
            VisualSettings.TransparencyColor = color;
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"設定リセットエラー: {ex.Message}");
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"設定インポートエラー: {ex.Message}");
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"設定エクスポートエラー: {ex.Message}");
        }
    }

    /// <summary>
    /// 設定を保存するコマンド
    /// </summary>
    [RelayCommand]
    private async Task SaveSettings()
    {
        try
        {
            // アプリケーション設定を保存
            var appSettingsResult = await _settingsService.SaveAppSettingsAsync(AppSettings);
            
            // 視覚設定を保存
            var visualSettingsResult = await _settingsService.SaveVisualSettingsAsync(VisualSettings);

            if (appSettingsResult && visualSettingsResult)
            {
                // 成功時はウィンドウを閉じる
                if (Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this) is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"設定保存エラー: {ex.Message}");
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ログフォルダ選択エラー: {ex.Message}");
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ログ表示エラー: {ex.Message}");
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ログクリアエラー: {ex.Message}");
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"古いログ削除エラー: {ex.Message}");
        }
    }

    #endregion

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
