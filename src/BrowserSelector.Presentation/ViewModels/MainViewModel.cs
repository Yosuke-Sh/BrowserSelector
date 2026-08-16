using BrowserSelector.Core.Enums;
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Presentation.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace BrowserSelector.Presentation.ViewModels;

/// <summary>
/// メインウィンドウのViewModel.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private const int CloseAfterLaunchSaveDebounceMs = 800;

    private readonly IBrowserService _browserService;
    private readonly ISettingsService _settingsService;
    private readonly ILocalizationService _localizationService;
    private readonly ICustomLanguageService _customLanguageService;
    private readonly IUrlRuleService _urlRuleService;
    private readonly ILogService? _logService;
    private readonly IExternalLinkService? _externalLinkService;
    private readonly IUpdateService? _updateService;

    [ObservableProperty]
    private ObservableCollection<Browser> _browsers = [];

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
    private string _titleMessage = string.Empty;

    [ObservableProperty]
    private VisualSettings _visualSettings = new();

    [ObservableProperty]
    private bool _closeAfterLaunch;

    /// <summary>
    /// Gets or sets a value indicating whether カスタムタイトルバーを表示するか（<see cref="AppSettings.ShowTitleBar"/>）.
    /// falseの場合、タイトルバー内の設定・最小化・閉じるボタンが操作不能になるため、
    /// 代わりにハンバーガーメニュー（<see cref="HamburgerMenuCommand"/>）を表示する.
    /// </summary>
    [ObservableProperty]
    private bool _showTitleBar = true;

    /// <summary>
    /// Gets or sets カウントダウン自動起動の残り秒数（Phase D）。0または非表示状態のときは非表示にする.
    /// </summary>
    [ObservableProperty]
    private int _countdownRemainingSeconds;

    /// <summary>
    /// Gets or sets a value indicating whether カウントダウン自動起動が有効な状態かどうか（Phase D）.
    /// UIの残り秒数ラベルの表示切替に使用する.
    /// </summary>
    [ObservableProperty]
    private bool _isCountdownActive;

    /// <summary>
    /// Ctrl+クリックによる「その起動に限り自動クローズを抑制」フラグ（Phase C-4）.
    /// <see cref="LaunchBrowserCommand"/> 実行前に <see cref="MainWindow"/> 側で一時的に立てる.
    /// </summary>
    private bool _suppressAutoCloseOnce;

    private System.Threading.Timer? _closeAfterLaunchSaveTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    /// <param name="browserService">browserService.</param>
    /// <param name="settingsService">settingsService.</param>
    /// <param name="localizationService">localizationService.</param>
    /// <param name="customLanguageService">customLanguageService.</param>
    /// <param name="urlRuleService">urlRuleService.</param>
    /// <param name="logService">logService.</param>
    /// <param name="externalLinkService">
    /// 設定画面のAboutセクション（Phase E-2）へ引き継ぐための外部リンクサービス。省略可（テスト互換のため）.
    /// </param>
    /// <param name="updateService">
    /// 更新通知バー（Phase H-9）用のアップデートサービス。省略可（テスト互換のため）.
    /// 未指定の場合、更新確認・通知バーは一切動作しない.
    /// </param>
    public MainViewModel(
        IBrowserService browserService,
        ISettingsService settingsService,
        ILocalizationService localizationService,
        ICustomLanguageService customLanguageService,
        IUrlRuleService urlRuleService,
        ILogService logService,
        IExternalLinkService? externalLinkService = null,
        IUpdateService? updateService = null)
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

        _customLanguageService = customLanguageService;
        _logService?.LogDetailed(LogLevel.Debug, "ICustomLanguageService設定完了", "MainViewModel",
                                "MVVM_INIT", "ViewModel", "System", "MainViewModel", "Constructor", "Service_CustomLanguage");

        _urlRuleService = urlRuleService;
        _logService?.LogDetailed(LogLevel.Debug, "IUrlRuleService設定完了", "MainViewModel",
                                "MVVM_INIT", "ViewModel", "System", "MainViewModel", "Constructor", "Service_UrlRule");

        _externalLinkService = externalLinkService;
        _updateService = updateService;

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

            // 初期化時にVisualSettingsを読み込み
            _logService?.LogDetailed(LogLevel.Debug, "VisualSettings読み込み開始", "MainViewModel",
                                    "MVVM_INIT", "ViewModel", "System", "MainViewModel", "Initialize", "LoadVisualSettings_Start");
            try
            {
                VisualSettings = _settingsService.LoadVisualSettingsAsync().GetAwaiter().GetResult();
                _logService?.LogDebug($"VisualSettings読み込み完了: Width={VisualSettings.InitialWindowWidth}, Height={VisualSettings.InitialWindowHeight}, UseGradient={VisualSettings.UseBackgroundGradient}, BackgroundColor={VisualSettings.BackgroundColor}, GradientStartColor={VisualSettings.GradientStartColor}, GradientEndColor={VisualSettings.GradientEndColor}", "MainViewModel");
            }
            // CA1031: RelayCommand/イベントハンドラーの最上位try-catch。サービス呼び出しやUI操作など例外種別が多岐にわたり、UIスレッドをクラッシュさせないための意図的な汎用catch。
            #pragma warning disable CA1031
            catch (Exception ex)
            {
                _logService?.LogError($"VisualSettings読み込みエラー: {ex.Message}", "MainViewModel", ex);
                // デフォルト設定を使用
                VisualSettings = new VisualSettings();
                _logService?.LogDebug($"デフォルトVisualSettings使用: UseGradient={VisualSettings.UseBackgroundGradient}, BackgroundColor={VisualSettings.BackgroundColor}", "MainViewModel");
            }
            #pragma warning restore CA1031
            _logService?.LogDetailed(LogLevel.Debug, "VisualSettings読み込み完了", "MainViewModel",
                                    "MVVM_INIT", "ViewModel", "System", "MainViewModel", "Initialize", "LoadVisualSettings_Success");

            // 初期化時に「起動後に閉じる」設定を読み込み（Phase C-5）
            try
            {
                AppSettings initialAppSettings = _settingsService.LoadAppSettingsAsync().GetAwaiter().GetResult();
                _closeAfterLaunch = initialAppSettings.CloseAfterUrlRuleMatch;
                ShowTitleBar = initialAppSettings.ShowTitleBar;
            }
            // CA1031: RelayCommand/イベントハンドラーの最上位try-catch。サービス呼び出しやUI操作など例外種別が多岐にわたり、UIスレッドをクラッシュさせないための意図的な汎用catch。
            #pragma warning disable CA1031
            catch (Exception ex)
            {
                _logService?.LogError($"AppSettings読み込みエラー（CloseAfterLaunch既定値を使用）: {ex.Message}", "MainViewModel", ex);
            }
            #pragma warning restore CA1031

            // 初期化時にブラウザ一覧を読み込み（データが存在しない場合のみ検出）
            _logService?.LogDetailed(LogLevel.Debug, "ブラウザ一覧読み込み開始", "MainViewModel",
                                    "MVVM_INIT", "ViewModel", "System", "MainViewModel", "Initialize", "LoadBrowsers_Start");
            _logService?.LogDebug("BrowserLoad.Init Triggered", "MainViewModel");
            _ = LoadBrowsersAsync();
            _logService?.LogDetailed(LogLevel.Debug, "ブラウザ一覧読み込み完了", "MainViewModel",
                                    "MVVM_INIT", "ViewModel", "System", "MainViewModel", "Initialize", "LoadBrowsers_Success");
            _logService?.LogDebug("BrowserLoad.Init Enqueued", "MainViewModel");

            // 初期タイトルメッセージを設定
            TitleMessage = LocalizedLogHelper.GetString("MainWindow.TitleMessage");
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
    /// Gets ブラウザを起動.
    /// </summary>
    public IAsyncRelayCommand<Browser> LaunchBrowserCommand { get; }

    /// <summary>
    /// Gets ブラウザ一覧を読み込み.
    /// </summary>
    public IAsyncRelayCommand LoadBrowsersCommand { get; }

    /// <summary>
    /// Gets 設定を開く.
    /// </summary>
    public IRelayCommand OpenSettingsCommand { get; }

    /// <summary>
    /// Gets 設定を閉じる.
    /// </summary>
    public IRelayCommand CloseSettingsCommand { get; }

    /// <summary>
    /// Gets uRLをクリア.
    /// </summary>
    public IRelayCommand ClearUrlCommand { get; }

    /// <summary>
    /// 設定変更通知を受け取る.
    /// </summary>
    /// <param name="sender">sender.</param>
    /// <param name="e">e.</param>
    public void OnSettingsChanged(object? sender, SettingsChangedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        if (e.SettingType == "VisualSettings" && e.NewValue is VisualSettings newVisualSettings)
        {
            // VisualSettingsを更新
            VisualSettings = newVisualSettings;

            // ウィンドウサイズの即座変更
            if (Application.Current.MainWindow is Views.MainWindow mainWindow)
            {
                ApplyWindowSizeChanges(mainWindow, newVisualSettings);

                // 背景色・グラデーションの即座変更
                ApplyBackgroundChanges(newVisualSettings);
            }
        }
    }

    /// <summary>
    /// ブラウザ変更通知を受け取る.
    /// </summary>
    /// <param name="sender">sender.</param>
    /// <param name="e">e.</param>
    public void OnBrowserChanged(object? sender, BrowserChangedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        _logService?.LogDebug($"ブラウザ変更通知を受信: {e.Browser.Name}, 変更タイプ: {e.ChangeType}", "MainViewModel");

        // ブラウザ一覧を再読み込み
        _ = LoadBrowsersAsync();
    }

    /// <summary>
    /// 起動引数で指定されたURLを設定.
    /// </summary>
    /// <param name="url">設定するURL.</param>
    public void SetInitialUrl(Uri url)
    {
        if (url != null)
        {
            SetInitialUrl(url.ToString());
        }
    }

    /// <summary>
    /// 起動引数で指定されたURLを設定.
    /// </summary>
    /// <param name="url">設定するURL.</param>
    public async void SetInitialUrl(string url)
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            Url = url;
            _logService?.LogInformation($"初期URLを設定: {url}", "MainViewModel");

            // URLルールに基づいてブラウザを自動選択
            await ApplyUrlRulesAsync(url).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 次回1回のブラウザ起動に限り、「起動後に閉じる」設定を無視してアプリを終了しないようにする（Ctrl+クリック、Phase C-4）.
    /// </summary>
    public void SuppressAutoCloseOnce()
    {
        _suppressAutoCloseOnce = true;
    }

    /// <summary>
    /// 既定ブラウザへ起動する（Phase D: カウントダウン自動起動・<c>--silent</c>/<c>--auto-launch</c>CLIオプションから使用）.
    /// URLが未設定の場合は何もしない。既定ブラウザが見つからない場合は先頭のブラウザへフォールバックする.
    /// </summary>
    /// <returns>representing the asynchronous operation.</returns>
    public async Task LaunchDefaultBrowserAsync()
    {
        if (string.IsNullOrWhiteSpace(Url))
        {
            return;
        }

        Browser? target = Browsers.FirstOrDefault(b => b.IsDefault) ?? Browsers.FirstOrDefault();
        if (target != null)
        {
            await LaunchBrowserAsync(target).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 「起動後に閉じる」設定変更時、800msデバウンスしてAppSettingsへ保存する（Phase C-5）.
    /// </summary>
    partial void OnCloseAfterLaunchChanged(bool value)
    {
        _closeAfterLaunchSaveTimer?.Dispose();
        _closeAfterLaunchSaveTimer = new System.Threading.Timer(
            _ => SaveCloseAfterLaunchSetting(value),
            null,
            CloseAfterLaunchSaveDebounceMs,
            System.Threading.Timeout.Infinite);
    }

    private void SaveCloseAfterLaunchSetting(bool value)
    {
        try
        {
            AppSettings appSettings = _settingsService.LoadAppSettingsAsync().GetAwaiter().GetResult();
            appSettings.CloseAfterUrlRuleMatch = value;
            _ = _settingsService.SaveAppSettingsAsync(appSettings).GetAwaiter().GetResult();
            _logService?.LogDebug($"CloseAfterLaunch設定を保存しました: {value}", "MainViewModel");
        }
        // CA1031: デバウンスタイマーのコールバック（バックグラウンドスレッド）。UIスレッド外で例外が伝播すると
        // プロセス終了につながるため、保存失敗をログに残しつつ握り潰す意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"CloseAfterLaunch設定の保存エラー: {ex.Message}", "MainViewModel", ex);
        }
        #pragma warning restore CA1031
    }

    partial void OnVisualSettingsChanged(VisualSettings value)
    {
        // VisualSettingsの変更を監視してPropertyChangedイベントを購読
        if (value != null)
        {
            value.PropertyChanged += OnVisualSettingsPropertyChanged;
        }
    }

    /// <summary>
    /// VisualSettingsのプロパティ変更を監視.
    /// </summary>
    private void OnVisualSettingsPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // ブラウザボタン設定の変更を即座に反映
        if (e.PropertyName?.StartsWith("BrowserButton", StringComparison.Ordinal) == true || e.PropertyName == "ShowBrowserName" || e.PropertyName == "BrowserIconSize")
        {
            // UIの更新を強制
            OnPropertyChanged(nameof(VisualSettings));
        }
    }

    /// <summary>
    /// ウィンドウサイズの変更を適用.
    /// </summary>
    private void ApplyWindowSizeChanges(Views.MainWindow mainWindow, VisualSettings visualSettings)
    {
        try
        {
            // 最小・最大サイズの制限
            double width = Math.Max(400, Math.Min(2000, visualSettings.InitialWindowWidth));
            double height = Math.Max(300, Math.Min(1500, visualSettings.InitialWindowHeight));

            // ウィンドウサイズを変更
            mainWindow.Width = width;
            mainWindow.Height = height;

            // ウィンドウ位置を中央に調整
            mainWindow.Left = (SystemParameters.PrimaryScreenWidth - width) / 2;
            mainWindow.Top = (SystemParameters.PrimaryScreenHeight - height) / 2;

            _logService?.LogInformation($"ウィンドウサイズを即座に変更: {width}x{height}", "MainViewModel");
        }
        // CA1031: RelayCommand/イベントハンドラーの最上位try-catch。サービス呼び出しやUI操作など例外種別が多岐にわたり、UIスレッドをクラッシュさせないための意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"ウィンドウサイズ変更エラー: {ex.Message}", "MainViewModel", ex);
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// 背景色・グラデーションの即座変更を適用.
    /// </summary>
    /// <param name="visualSettings">視覚設定.</param>
    private void ApplyBackgroundChanges(VisualSettings visualSettings)
    {
        try
        {
            _logService?.LogDebug("ApplyBackgroundChanges開始", "MainViewModel");
            _logService?.LogDebug($"設定値: UseBackgroundGradient={visualSettings.UseBackgroundGradient}, BackgroundColor={visualSettings.BackgroundColor}", "MainViewModel");

            // XAMLでBackgroundBrushConverterを使用して背景を管理するため、
            // ここではVisualSettingsの変更を通知するのみ
            _logService?.LogDebug("VisualSettingsの変更を通知してXAMLで背景を更新", "MainViewModel");

            // VisualSettingsプロパティの変更を通知（XAMLのバインディングが更新される）
            OnPropertyChanged(nameof(VisualSettings));

            _logService?.LogDebug("ApplyBackgroundChanges完了", "MainViewModel");
        }
        // CA1031: RelayCommand/イベントハンドラーの最上位try-catch。サービス呼び出しやUI操作など例外種別が多岐にわたり、UIスレッドをクラッシュさせないための意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"背景設定適用エラー: {ex.Message}", "MainViewModel", ex);
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// ブラウザ一覧を読み込み.
    /// </summary>
    private async Task LoadBrowsersAsync()
    {
        try
        {
            // 既存のブラウザデータがある場合は読み込みのみ
            IEnumerable<Browser> existingBrowsers = await _browserService.GetAllBrowsersAsync().ConfigureAwait(false);
            if (existingBrowsers.Any())
            {
                // UIスレッドでコレクションを更新
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    Browsers.Clear();
                    foreach (Browser? browser in existingBrowsers.Where(b => b.IsEnabled))
                    {
                        Browsers.Add(browser);
                    }
                    StatusMessage = LocalizedLogHelper.GetString("MainWindow.BrowsersLoaded", Browsers.Count);
                });
                return;
            }

            // ブラウザデータが存在しない場合のみ検出を実行
            IsLoading = true;
            StatusMessage = LocalizedLogHelper.GetString("MainWindow.DetectingBrowsers");

            IEnumerable<Browser> browsers = await _browserService.DetectBrowsersAsync().ConfigureAwait(false);

            // UIスレッドでコレクションを更新
            Application.Current?.Dispatcher.Invoke(() =>
            {
                Browsers.Clear();
                foreach (Browser? browser in browsers.Where(b => b.IsEnabled))
                {
                    Browsers.Add(browser);
                }
            });

            // ログ出力
            _logService?.LogDebug($"検出されたブラウザ数: {browsers.Count()}", "MainViewModel");
            foreach (Browser browser in browsers)
            {
                _logService?.LogDebug($"ブラウザ: {browser.Name}, ID: {browser.Id}, 有効: {browser.IsEnabled}, パス: {browser.ExecutablePath}, タイプ: {browser.Type}", "MainViewModel");
            }

            StatusMessage = LocalizedLogHelper.GetString("MainWindow.BrowsersDetected", Browsers.Count);
        }
        // CA1031: RelayCommand/イベントハンドラーの最上位try-catch。サービス呼び出しやUI操作など例外種別が多岐にわたり、UIスレッドをクラッシュさせないための意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"ブラウザ読み込みエラー: {ex.Message}", "MainViewModel", ex);
            Application.Current?.Dispatcher.Invoke(() =>
            {
                IsLoading = false;
                StatusMessage = LocalizedLogHelper.GetString("MainWindow.BrowserLoadError", ex.Message);
            });
        }
        #pragma warning restore CA1031
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// ブラウザを起動.
    /// </summary>
    private async Task LaunchBrowserAsync(Browser? browser)
    {
        if (browser == null || string.IsNullOrWhiteSpace(Url))
        {
            return;
        }

        try
        {
            IsLoading = true;

            // ログ出力
            _logService?.LogInformation($"ブラウザ起動試行: {browser.Name}, ID: {browser.Id}, パス: {browser.ExecutablePath}, URL: {Url}", "MainViewModel");

            StatusMessage = $"ブラウザ {browser.Name} を起動中...";

            Uri urlUri = new(Url);
            bool success = await _browserService.LaunchBrowserAsync(browser, urlUri).ConfigureAwait(false);

            if (success)
            {
                await _browserService.UpdateUsageAsync(browser).ConfigureAwait(false);
                browser.IncrementUseCount();
                StatusMessage = $"ブラウザ {browser.Name} を起動しました";
                _logService?.LogInformation($"ブラウザ起動成功: {browser.Name}", "MainViewModel");

                // ブラウザ起動後のアプリ終了設定が有効な場合（Ctrl+クリックによる今回限りの抑制を優先、Phase C-4/C-5）
                bool suppressThisTime = _suppressAutoCloseOnce;
                _suppressAutoCloseOnce = false;
                if (CloseAfterLaunch && !suppressThisTime)
                {
                    _logService?.LogInformation("ブラウザ起動後のアプリ終了", "MainViewModel");
                    // UIスレッドでアプリケーションを終了
                    Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
                }
            }
            else
            {
                StatusMessage = $"ブラウザ {browser.Name} の起動に失敗しました";
                _logService?.LogWarning($"ブラウザ起動失敗: {browser.Name}", "MainViewModel");
            }
        }
        // CA1031: RelayCommand/イベントハンドラーの最上位try-catch。サービス呼び出しやUI操作など例外種別が多岐にわたり、UIスレッドをクラッシュさせないための意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            StatusMessage = $"ブラウザ起動エラー: {ex.Message}";
            _logService?.LogError($"ブラウザ起動例外: {browser?.Name}, エラー: {ex.Message}", "MainViewModel", ex);
        }
        #pragma warning restore CA1031
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// ブラウザ起動が可能かどうかを判定.
    /// </summary>
    private bool CanLaunchBrowser(Browser? browser)
    {
        return browser != null && !string.IsNullOrWhiteSpace(Url) && !IsLoading;
    }

    /// <summary>
    /// 設定を開く.
    /// </summary>
    private void OpenSettings()
    {
        try
        {
            // 設定画面を開く
            SettingsViewModel settingsViewModel = new(_settingsService, _browserService, _localizationService, _customLanguageService, _urlRuleService, _logService ?? throw new InvalidOperationException("LogService is not available"), _externalLinkService, _updateService);
            Views.SettingsWindow settingsWindow = new(settingsViewModel);

            // 設定変更通知のイベントハンドラーを登録
            settingsViewModel.SettingsChanged += OnSettingsChanged;
            settingsViewModel.BrowserChanged += OnBrowserChanged;

            // 設定画面の「更新を適用」からアプリ終了が要求された場合、ダイアログを閉じてから
            // MainViewModel.ShutdownRequestedへ委譲する（ShowDialog()実行中に直接Shutdownを
            // 呼ぶとネストされたDispatcherフレームと整合しないため、フラグで戻り値を待つ）。
            bool updateShutdownRequested = false;
            settingsViewModel.ShutdownRequested += (_, _) =>
            {
                updateShutdownRequested = true;
                settingsWindow.Close();
            };

            bool? result = settingsWindow.ShowDialog();

            // イベントハンドラーを解除
            settingsViewModel.SettingsChanged -= OnSettingsChanged;
            settingsViewModel.BrowserChanged -= OnBrowserChanged;

            // S2583: updateShutdownRequestedはShutdownRequestedイベントハンドラー（クロージャ）内で
            // trueへ書き換えられる可能性があるが、静的解析はShowDialog()呼び出しを挟んだ
            // イベント発火のタイミングを追跡できず「常にfalse」と誤検知するため抑制する。
#pragma warning disable S2583
            if (updateShutdownRequested)
#pragma warning restore S2583
            {
                ShutdownRequested?.Invoke(this, EventArgs.Empty);
                return;
            }

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
        // CA1031: RelayCommand/イベントハンドラーの最上位try-catch。サービス呼び出しやUI操作など例外種別が多岐にわたり、UIスレッドをクラッシュさせないための意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"設定画面を開くエラー: {ex.Message}", "MainViewModel", ex);
            _ = MessageBox.Show($"設定画面を開けませんでした: {ex.Message}", "エラー",
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// 視覚設定を再読み込みしてメイン画面に反映.
    /// </summary>
    private async Task RefreshVisualSettingsAsync()
    {
        try
        {
            _logService?.LogDebug("視覚設定の再読み込み開始", "MainViewModel");

            VisualSettings visualSettings = await _settingsService.LoadVisualSettingsAsync().ConfigureAwait(false);
            _logService?.LogDebug($"視覚設定読み込み完了: BackgroundColor={visualSettings.BackgroundColor}", "MainViewModel");

            // メイン画面に視覚設定を適用（UIスレッドで実行）
            Application.Current?.Dispatcher.Invoke(() =>
            {
                if (Application.Current?.MainWindow is Window mainWindow)
                {
                    _logService?.LogDebug("メイン画面に視覚設定を適用開始", "MainViewModel");

                    // XAMLでBackgroundBrushConverterを使用して背景を管理するため、
                    // ここではVisualSettingsの変更を通知するのみ
                    _logService?.LogDebug("VisualSettingsの変更を通知してXAMLで背景を更新", "MainViewModel");

                    // VisualSettingsを反映
                    VisualSettings = visualSettings;
                    _logService?.LogDebug("メイン画面への視覚設定適用完了", "MainViewModel");
                }
                else
                {
                    _logService?.LogWarning("メイン画面が見つかりません", "MainViewModel");
                }
            });
        }
        // CA1031: RelayCommand/イベントハンドラーの最上位try-catch。サービス呼び出しやUI操作など例外種別が多岐にわたり、UIスレッドをクラッシュさせないための意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"視覚設定の再読み込みエラー: {ex.Message}", "MainViewModel", ex);
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// 設定を閉じる.
    /// </summary>
    private void CloseSettings()
    {
        IsSettingsVisible = false;
    }

    /// <summary>
    /// URLをクリア.
    /// </summary>
    private void ClearUrl()
    {
        Url = string.Empty;
    }

    /// <summary>
    /// URLルールに基づいてブラウザを自動選択.
    /// </summary>
    /// <param name="url">対象URL.</param>
    private async Task ApplyUrlRulesAsync(string url)
    {
        try
        {
            _logService?.LogInformation($"URLルール適用開始: {url}", "MainViewModel");

            Uri urlUri = new(url);
            Browser? matchingBrowser = await _urlRuleService.FindMatchingBrowserAsync(urlUri, Browsers).ConfigureAwait(false);
            if (matchingBrowser != null)
            {
                SelectedBrowser = matchingBrowser;
                _logService?.LogInformation($"URLルール適用完了: {url} -> {matchingBrowser.Name}", "MainViewModel");
                StatusMessage = $"URLルールにより {matchingBrowser.Name} が自動選択されました";

                // 自動起動（LaunchBrowserAsync内でアプリ終了処理も実行される）
                await LaunchBrowserAsync(matchingBrowser).ConfigureAwait(false);
            }
            else
            {
                _logService?.LogInformation($"URLルールにマッチするブラウザなし: {url}", "MainViewModel");
                StatusMessage = "URLルールにマッチするブラウザがありません";
            }
        }
        // CA1031: RelayCommand/イベントハンドラーの最上位try-catch。サービス呼び出しやUI操作など例外種別が多岐にわたり、UIスレッドをクラッシュさせないための意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"URLルール適用エラー: {ex.Message}", "MainViewModel", ex);
            StatusMessage = "URLルールの適用中にエラーが発生しました";
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// URLが変更された時の処理.
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
    /// タイトルメッセージを更新.
    /// </summary>
    private void UpdateTitleMessage()
    {
        TitleMessage = string.IsNullOrWhiteSpace(Url)
            ? LocalizedLogHelper.GetString("MainWindow.TitleMessage")
            : LocalizedLogHelper.GetString("MainWindow.SelectBrowser");
    }
}
