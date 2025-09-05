using System.Collections.ObjectModel;
using System.Windows.Input;
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Core.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace BrowserSelector.Presentation.ViewModels;

/// <summary>
/// メインウィンドウのViewModel
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IBrowserService _browserService;
    private readonly ISettingsService _settingsService;
    private readonly ILocalizationService _localizationService;
    private readonly IUrlRuleService _urlRuleService;
    private readonly ILogService _logService = null!;

    [ObservableProperty]
    private ObservableCollection<Browser> _browsers = new();

    [ObservableProperty]
    private Browser? _selectedBrowser;

    [ObservableProperty]
    private string _url = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isSettingsVisible;

    [ObservableProperty]
    private string _titleMessage = "URLを設定し、ブラウザを選択してください。";

    [ObservableProperty]
    private VisualSettings _visualSettings = new();

    /// <summary>
    /// 設定変更通知を受け取る
    /// </summary>
    public void OnSettingsChanged(object? sender, SettingsChangedEventArgs e)
    {
        if (e.SettingType == "VisualSettings" && e.NewValue is VisualSettings newVisualSettings)
        {
            // VisualSettingsを更新
            VisualSettings = newVisualSettings;
            
            // ウィンドウサイズの即座変更
            if (Application.Current.MainWindow is Views.MainWindow mainWindow)
            {
                ApplyWindowSizeChanges(mainWindow, newVisualSettings);
            }
        }
    }

    /// <summary>
    /// ウィンドウサイズの変更を適用
    /// </summary>
    private void ApplyWindowSizeChanges(Views.MainWindow mainWindow, VisualSettings visualSettings)
    {
        try
        {
            // 最小・最大サイズの制限
            var width = Math.Max(400, Math.Min(2000, visualSettings.InitialWindowWidth));
            var height = Math.Max(300, Math.Min(1500, visualSettings.InitialWindowHeight));
            
            // ウィンドウサイズを変更
            mainWindow.Width = width;
            mainWindow.Height = height;
            
            // ウィンドウ位置を中央に調整
            mainWindow.Left = (SystemParameters.PrimaryScreenWidth - width) / 2;
            mainWindow.Top = (SystemParameters.PrimaryScreenHeight - height) / 2;
            
            _logService?.LogInformation($"ウィンドウサイズを即座に変更: {width}x{height}", "MainViewModel");
        }
        catch (Exception ex)
        {
            _logService?.LogError($"ウィンドウサイズ変更エラー: {ex.Message}", "MainViewModel", ex);
        }
    }

    public MainViewModel(
        IBrowserService browserService,
        ISettingsService settingsService,
        ILocalizationService localizationService,
        IUrlRuleService urlRuleService,
        ILogService logService)
    {
        // まずログサービスを設定
        _logService = logService;
        _logService?.LogDetailed(LogLevel.Debug, "MainViewModelコンストラクタ開始", "MainViewModel", 
                                "MVVM_INIT", "ViewModel", "System", "MainViewModel", "Constructor", "Started");

        // 依存サービスの設定
        _browserService = browserService;
        _logService?.LogDetailed(LogLevel.Debug, "IBrowserService設定完了", "MainViewModel", 
                                "MVVM_INIT", "ViewModel", "System", "MainViewModel", "Constructor", "Service_Browser");
        
        _settingsService = settingsService;
        _logService?.LogDetailed(LogLevel.Debug, "ISettingsService設定完了", "MainViewModel", 
                                "MVVM_INIT", "ViewModel", "System", "MainViewModel", "Constructor", "Service_Settings");
        
        _localizationService = localizationService;
        _logService?.LogDetailed(LogLevel.Debug, "ILocalizationService設定完了", "MainViewModel", 
                                "MVVM_INIT", "ViewModel", "System", "MainViewModel", "Constructor", "Service_Localization");
        
        _urlRuleService = urlRuleService;
        _logService?.LogDetailed(LogLevel.Debug, "IUrlRuleService設定完了", "MainViewModel", 
                                "MVVM_INIT", "ViewModel", "System", "MainViewModel", "Constructor", "Service_UrlRule");

        // 起動ログ
        try
        {
            _logService?.LogDetailed(LogLevel.Information, "MainViewModel初期化開始", "MainViewModel", 
                                    "MVVM_INIT", "ViewModel", "System", "MainViewModel", "Initialize", "Started");
            
            // コマンドの初期化
            LoadBrowsersCommand = new AsyncRelayCommand(LoadBrowsersAsync);
            _logService?.LogDetailed(LogLevel.Debug, "LoadBrowsersCommand作成完了", "MainViewModel", 
                                    "MVVM_INIT", "ViewModel", "System", "MainViewModel", "Initialize", "Command_LoadBrowsers");
            
            LaunchBrowserCommand = new AsyncRelayCommand<Browser>(LaunchBrowserAsync, CanLaunchBrowser);
            _logService?.LogDetailed(LogLevel.Debug, "LaunchBrowserCommand作成完了", "MainViewModel", 
                                    "MVVM_INIT", "ViewModel", "System", "MainViewModel", "Initialize", "Command_LaunchBrowser");
            
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            CloseSettingsCommand = new RelayCommand(CloseSettings);
            ClearUrlCommand = new RelayCommand(ClearUrl);
            
            _logService?.LogDetailed(LogLevel.Information, "コマンド初期化完了", "MainViewModel", 
                                    "MVVM_INIT", "ViewModel", "System", "MainViewModel", "Initialize", "Commands");

            // 初期化時にブラウザ一覧を読み込み（データが存在しない場合のみ検出）
            _logService?.LogDetailed(LogLevel.Debug, "ブラウザ一覧読み込み開始", "MainViewModel", 
                                    "MVVM_INIT", "ViewModel", "System", "MainViewModel", "Initialize", "LoadBrowsers_Start");
            _logService?.LogDebug("BrowserLoad.Init Triggered", "MainViewModel");
            _ = LoadBrowsersAsync();
            _logService?.LogDetailed(LogLevel.Debug, "ブラウザ一覧読み込み完了", "MainViewModel", 
                                    "MVVM_INIT", "ViewModel", "System", "MainViewModel", "Initialize", "LoadBrowsers_Success");
            _logService?.LogDebug("BrowserLoad.Init Enqueued", "MainViewModel");
            
            // 初期タイトルメッセージを設定
            UpdateTitleMessage();
            
            _logService?.LogDetailed(LogLevel.Information, "MainViewModel初期化完了", "MainViewModel", 
                                    "MVVM_INIT", "ViewModel", "System", "MainViewModel", "Initialize", "Success");
        }
        catch (Exception ex)
        {
            _logService?.LogDetailed(LogLevel.Error, $"MainViewModel初期化エラー: {ex.Message}", "MainViewModel", 
                                    "MVVM_INIT", "ViewModel", "System", "MainViewModel", "Initialize", "Failed", ex);
            throw;
        }
    }

    /// <summary>
    /// ブラウザ一覧を読み込み
    /// </summary>
    public IAsyncRelayCommand LoadBrowsersCommand { get; }

    /// <summary>
    /// ブラウザを起動
    /// </summary>
    public IAsyncRelayCommand<Browser> LaunchBrowserCommand { get; }

    /// <summary>
    /// 設定を開く
    /// </summary>
    public IRelayCommand OpenSettingsCommand { get; }

    /// <summary>
    /// 設定を閉じる
    /// </summary>
    public IRelayCommand CloseSettingsCommand { get; }

    /// <summary>
    /// URLをクリア
    /// </summary>
    public IRelayCommand ClearUrlCommand { get; }

    /// <summary>
    /// ブラウザ一覧を読み込み
    /// </summary>
    private async Task LoadBrowsersAsync()
    {
        try
        {
            // 既存のブラウザデータがある場合は読み込みのみ
            var existingBrowsers = await _browserService.GetAllBrowsersAsync();
            if (existingBrowsers.Any())
            {
                Browsers.Clear();
                foreach (var browser in existingBrowsers.Where(b => b.IsEnabled))
                {
                    Browsers.Add(browser);
                }
                StatusMessage = $"ブラウザ {Browsers.Count} 個を読み込みました";
                return;
            }

            // ブラウザデータが存在しない場合のみ検出を実行
            IsLoading = true;
            StatusMessage = "ブラウザを検出中...";

            var browsers = await _browserService.DetectBrowsersAsync();
            
            Browsers.Clear();
            foreach (var browser in browsers.Where(b => b.IsEnabled))
            {
                Browsers.Add(browser);
            }

            // デバッグ情報を出力
            System.Diagnostics.Debug.WriteLine($"検出されたブラウザ数: {browsers.Count()}");
            foreach (var browser in browsers)
            {
                System.Diagnostics.Debug.WriteLine($"ブラウザ: {browser.Name}, ID: {browser.Id}, 有効: {browser.IsEnabled}, パス: {browser.ExecutablePath}, タイプ: {browser.Type}");
            }

            StatusMessage = $"ブラウザ {Browsers.Count} 個を検出しました";
        }
        catch (Exception ex)
        {
            StatusMessage = $"ブラウザ読み込みエラー: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"ブラウザ読み込みエラー: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// ブラウザを起動
    /// </summary>
    private async Task LaunchBrowserAsync(Browser? browser)
    {
        if (browser == null || string.IsNullOrWhiteSpace(Url))
            return;

        try
        {
            IsLoading = true;
            
            // デバッグ情報を出力
            System.Diagnostics.Debug.WriteLine($"ブラウザ起動試行: {browser.Name}, ID: {browser.Id}, パス: {browser.ExecutablePath}, URL: {Url}");
            
            StatusMessage = $"ブラウザ {browser.Name} を起動中...";

            var success = await _browserService.LaunchBrowserAsync(browser, Url);
            
            if (success)
            {
                await _browserService.UpdateUsageAsync(browser);
                browser.IncrementUseCount();
                StatusMessage = $"ブラウザ {browser.Name} を起動しました";
                System.Diagnostics.Debug.WriteLine($"ブラウザ起動成功: {browser.Name}");
                
                // ブラウザ起動後のアプリ終了設定が有効な場合
                var appSettings = await _settingsService.LoadAppSettingsAsync();
                if (appSettings.CloseAfterUrlRuleMatch)
                {
                    _logService?.LogInformation("ブラウザ起動後のアプリ終了", "MainViewModel");
                    Application.Current.Shutdown();
                }
            }
            else
            {
                StatusMessage = $"ブラウザ {browser.Name} の起動に失敗しました";
                System.Diagnostics.Debug.WriteLine($"ブラウザ起動失敗: {browser.Name}");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"ブラウザ起動エラー: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"ブラウザ起動例外: {browser?.Name}, エラー: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// ブラウザ起動が可能かどうかを判定
    /// </summary>
    private bool CanLaunchBrowser(Browser? browser)
    {
        return browser != null && !string.IsNullOrWhiteSpace(Url) && !IsLoading;
    }

    /// <summary>
    /// 設定を開く
    /// </summary>
    private void OpenSettings()
    {
        try
        {
            // 設定画面を開く
            var settingsViewModel = new SettingsViewModel(_settingsService, _browserService, _localizationService, _urlRuleService, _logService);
            var settingsWindow = new Views.SettingsWindow(settingsViewModel);
            
            // 設定変更通知のイベントハンドラーを登録
            settingsViewModel.SettingsChanged += OnSettingsChanged;
            
            var result = settingsWindow.ShowDialog();
            
            // イベントハンドラーを解除
            settingsViewModel.SettingsChanged -= OnSettingsChanged;
            
            // 設定画面が閉じられた後、設定が保存された場合は再読み込み
            if (result == true)
            {
                _logService?.LogDebug("設定画面で設定が保存されました。メイン画面の設定を再読み込みします。", "MainViewModel");
                _ = LoadBrowsersAsync();
                
                // 視覚設定を再読み込みしてメイン画面に反映
                _ = RefreshVisualSettingsAsync();
            }
            else
            {
                _logService?.LogDebug("設定画面がキャンセルされました。", "MainViewModel");
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"設定画面を開くエラー: {ex.Message}", "MainViewModel", ex);
            MessageBox.Show($"設定画面を開けませんでした: {ex.Message}", "エラー", 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 視覚設定を再読み込みしてメイン画面に反映
    /// </summary>
    private async Task RefreshVisualSettingsAsync()
    {
        try
        {
            _logService?.LogDebug("視覚設定の再読み込み開始", "MainViewModel");
            
            var visualSettings = await _settingsService.LoadVisualSettingsAsync();
            _logService?.LogDebug($"視覚設定読み込み完了: BackgroundColor={visualSettings.BackgroundColor}", "MainViewModel");
            
            // メイン画面に視覚設定を適用
            if (Application.Current?.MainWindow is Window mainWindow)
            {
                _logService?.LogDebug("メイン画面に視覚設定を適用開始", "MainViewModel");
                
                // 背景色またはグラデーションを適用
                if (visualSettings.UseBackgroundGradient)
                {
                    // グラデーション方向に応じてStartPointとEndPointを設定
                    System.Windows.Point startPoint, endPoint;
                    switch (visualSettings.GradientDirection)
                    {
                        case BrowserSelector.Core.Enums.GradientDirection.Horizontal:
                            startPoint = new System.Windows.Point(0, 0);
                            endPoint = new System.Windows.Point(1, 0);
                            break;
                        case BrowserSelector.Core.Enums.GradientDirection.Diagonal:
                            startPoint = new System.Windows.Point(0, 0);
                            endPoint = new System.Windows.Point(1, 1);
                            break;
                        default: // Vertical
                            startPoint = new System.Windows.Point(0, 0);
                            endPoint = new System.Windows.Point(0, 1);
                            break;
                    }
                    
                    mainWindow.Background = new System.Windows.Media.LinearGradientBrush
                    {
                        StartPoint = startPoint,
                        EndPoint = endPoint,
                        GradientStops = new System.Windows.Media.GradientStopCollection
                        {
                            new System.Windows.Media.GradientStop(visualSettings.GradientStartColor, 0),
                            new System.Windows.Media.GradientStop(visualSettings.GradientEndColor, 1)
                        }
                    };
                }
                else
                {
                    mainWindow.Background = new System.Windows.Media.SolidColorBrush(visualSettings.BackgroundColor);
                }
                
                // VisualSettingsを反映
                VisualSettings = visualSettings;
                _logService?.LogDebug("メイン画面への視覚設定適用完了", "MainViewModel");
            }
            else
            {
                _logService?.LogWarning("メイン画面が見つかりません", "MainViewModel");
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"視覚設定の再読み込みエラー: {ex.Message}", "MainViewModel", ex);
        }
    }

    /// <summary>
    /// 設定を閉じる
    /// </summary>
    private void CloseSettings()
    {
        IsSettingsVisible = false;
    }

    /// <summary>
    /// URLをクリア
    /// </summary>
    private void ClearUrl()
    {
        Url = string.Empty;
    }

    /// <summary>
    /// 起動引数で指定されたURLを設定
    /// </summary>
    /// <param name="url">設定するURL</param>
    public async void SetInitialUrl(string url)
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            Url = url;
            _logService?.LogInformation($"初期URLを設定: {url}", "MainViewModel");
            
            // URLルールに基づいてブラウザを自動選択
            await ApplyUrlRulesAsync(url);
        }
    }

    /// <summary>
    /// URLルールに基づいてブラウザを自動選択
    /// </summary>
    /// <param name="url">対象URL</param>
    private async Task ApplyUrlRulesAsync(string url)
    {
        try
        {
            _logService?.LogInformation($"URLルール適用開始: {url}", "MainViewModel");
            
            var matchingBrowser = await _urlRuleService.FindMatchingBrowserAsync(url, Browsers);
            if (matchingBrowser != null)
            {
                SelectedBrowser = matchingBrowser;
                _logService?.LogInformation($"URLルール適用完了: {url} -> {matchingBrowser.Name}", "MainViewModel");
                StatusMessage = $"URLルールにより {matchingBrowser.Name} が自動選択されました";
                
                // 自動起動（LaunchBrowserAsync内でアプリ終了処理も実行される）
                await LaunchBrowserAsync(matchingBrowser);
            }
            else
            {
                _logService?.LogInformation($"URLルールにマッチするブラウザなし: {url}", "MainViewModel");
                StatusMessage = "URLルールにマッチするブラウザがありません";
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"URLルール適用エラー: {ex.Message}", "MainViewModel", ex);
            StatusMessage = "URLルールの適用中にエラーが発生しました";
        }
    }

    /// <summary>
    /// URLが変更された時の処理
    /// </summary>
    partial void OnUrlChanged(string value)
    {
        LaunchBrowserCommand.NotifyCanExecuteChanged();
        
        // タイトルメッセージを更新
        UpdateTitleMessage();
        
        // URLルールに基づいてブラウザを自動選択
        if (!string.IsNullOrWhiteSpace(value))
        {
            _ = ApplyUrlRulesAsync(value);
        }
    }
    
    /// <summary>
    /// タイトルメッセージを更新
    /// </summary>
    private void UpdateTitleMessage()
    {
        if (string.IsNullOrWhiteSpace(Url))
        {
            TitleMessage = "URLを設定し、ブラウザを選択してください。";
        }
        else
        {
            TitleMessage = "ブラウザを選択してください。";
        }
    }
}
