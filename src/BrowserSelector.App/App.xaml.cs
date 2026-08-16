using BrowserSelector.App.CommandLine;
using BrowserSelector.App.DependencyInjection;
using BrowserSelector.App.SystemIntegration;
using BrowserSelector.Core.Enums;
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.SystemIntegration;
using BrowserSelector.Presentation.Converters;
using BrowserSelector.Presentation.Extensions;
using BrowserSelector.Presentation.Helpers;
using BrowserSelector.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;

namespace BrowserSelector.App;

/// <summary>
/// Interaction logic for App.xaml.
/// </summary>
// CA1515: WPFのXAMLコンパイラが生成する部分クラス(App.g.cs)は常にpublicのため、
// アクセシビリティを一致させる必要があり internal 化できない（正当な設計上の制約）。
// CA1001: IDisposableフィールド(_singleInstanceManager/_host)はOnExit()で確実にDisposeしている。
// Applicationは基底側の設計上IDisposableを実装しないため、型自体をIDisposable化はしない。
#pragma warning disable CA1515, CA1001
public partial class App : System.Windows.Application
#pragma warning restore CA1515, CA1001
{
    private readonly CancellationTokenSource _updateCheckCts = new();
    private IHost? _host;
    private ILogService? _logService;
    private SingleInstanceManager? _singleInstanceManager;
    private MainViewModel? _mainViewModel;
    private TrayIconManager? _trayIconManager;
    private CommandLineOptions? _commandLineOptions;

    /// <inheritdoc/>
    protected override void OnStartup(StartupEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        try
        {
            // コマンドライン引数のパース（Phase D）: -d/--delay, -b/--browser, --silent, --auto-launch, -h/--help, -v/--version
            _commandLineOptions = CommandLineParser.Parse(e.Args);
            if (_commandLineOptions.ShowHelp)
            {
                _ = System.Windows.MessageBox.Show(CommandLineParser.HelpText, "BrowserSelector", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }

            if (_commandLineOptions.ShowVersion)
            {
                _ = System.Windows.MessageBox.Show($"BrowserSelector v{Core.AppInfo.CurrentVersion}", "BrowserSelector", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }

            // テストモードの確認
            bool isTestMode = e.Args.Contains("--test-mode") || _commandLineOptions.TestMode ||
                             Environment.GetEnvironmentVariable("BROWSERSELECTOR_TEST_MODE") == "true";

            // 単一インスタンス判定: 先行インスタンスが存在する場合はURLを転送して即終了
            _singleInstanceManager = new SingleInstanceManager();
            if (!_singleInstanceManager.TryAcquire())
            {
                Uri? forwardedUrl = e.Args.Length > 0 && Uri.TryCreate(e.Args[0], UriKind.Absolute, out Uri? parsedUri)
                    ? parsedUri
                    : null;
                _ = SingleInstanceManager.TrySendToExistingInstanceAsync(forwardedUrl).GetAwaiter().GetResult();
                _singleInstanceManager.Dispose();
                _singleInstanceManager = null;
                Shutdown();
                return;
            }

            _singleInstanceManager.UrlReceived += OnUrlReceivedFromNewInstance;

            _logService?.LogTrace($"アプリケーション起動処理開始: コマンドライン引数={string.Join(" ", e.Args)}, テストモード={isTestMode}", "App");
            // ホストの構築
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // BrowserSelectorサービスの追加
                    _ = services.AddBrowserSelectorServices();
                })
                .Build();

            // ホストの開始
            _host.StartAsync().GetAwaiter().GetResult();

            // ログサービスの取得
            _logService = _host.Services.GetRequiredService<ILogService>();
            _logService?.LogDetailed(LogLevel.Information, "アプリケーション起動開始", "App",
                                  "STARTUP", "Application", "System", "App", "Initialize", "Started");

            // ローカライゼーションサービスの設定
            ILocalizationService localizationService = _host.Services.GetRequiredService<ILocalizationService>();
            LocalizationExtension.SetLocalizationService(localizationService);
            LocalizedMessageBox.SetLocalizationService(localizationService);
            LocalizedLogHelper.SetLocalizationService(localizationService);
            LocalizedFormatHelper.SetLocalizationService(localizationService);

            // アイコンキャッシュサービスの設定
            IconPathConverter.SetIconCacheService(_host.Services.GetRequiredService<IIconCacheService>());

            // 共通コントロールスタイルを読み込み（トークン参照のため、テーマ辞書より先に追加）
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("/BrowserSelector.Presentation;component/Resources/Themes/Controls.xaml", UriKind.Relative),
            });

            // 設定された言語・テーマを適用
            try
            {
                ISettingsService settingsService = _host.Services.GetRequiredService<ISettingsService>();
                Core.Models.AppSettings appSettings = settingsService.LoadAppSettingsAsync().GetAwaiter().GetResult();
                System.Globalization.CultureInfo culture = new(appSettings.Language);
                localizationService.SetLanguage(culture).GetAwaiter().GetResult();

                IThemeService themeService = _host.Services.GetRequiredService<IThemeService>();
                themeService.ApplyTheme(appSettings.ThemeMode);
            }
            // CA1031: アプリ起動/終了時の最上位フォールバック処理。DIコンテナ初期化やホスト起動、UI適用など例外種別が広範なため、アプリのクラッシュを防ぐ意図的な汎用catch。
            #pragma warning disable CA1031
            catch (Exception ex)
            {
                // 設定読み込みに失敗した場合はデフォルト言語・テーマを使用
                _logService?.LogWarning($"設定読み込みに失敗、デフォルト言語・テーマを使用: {ex.Message}", "App");
                System.Globalization.CultureInfo defaultCulture = new("ja-JP");
                localizationService.SetLanguage(defaultCulture).GetAwaiter().GetResult();
                _host.Services.GetRequiredService<IThemeService>().ApplyTheme(BrowserSelector.Core.Enums.ThemeMode.System);
            }
            #pragma warning restore CA1031

            // 起動引数からURLを取得（Phase D: CommandLineParserがオプションを除去済みのURLを返す。
            // 未指定時は従来どおり先頭の非オプション引数へフォールバック）
            string? initialUrl = _commandLineOptions.Url;
            if (string.IsNullOrEmpty(initialUrl) && e.Args.Length > 0 && !e.Args[0].StartsWith('-'))
            {
                initialUrl = e.Args[0];
            }

            if (!string.IsNullOrEmpty(initialUrl))
            {
                _logService?.LogDetailed(LogLevel.Debug, $"起動引数でURLを受信: {initialUrl}", "App",
                                      "ARGS", initialUrl, "System", "Args", "Parse", "Success");
            }

            // メインウィンドウの作成と表示
            MainViewModel mainViewModel;
            try
            {
                _logService?.LogDetailed(LogLevel.Information, "MainViewModel作成開始", "App",
                                      "MVVM_CREATE", "ViewModel", "System", "MainViewModel", "Create", "Started");

                _logService?.LogDetailed(LogLevel.Debug, "DIコンテナからMainViewModelを取得開始", "App",
                                      "MVVM_CREATE", "ViewModel", "System", "MainViewModel", "Resolve", "Started");

                mainViewModel = _host.Services.GetRequiredService<MainViewModel>();
                _mainViewModel = mainViewModel;

                // Phase H-9: 更新適用後のシャットダウン要求をUIスレッドへディスパッチする。
                // MainViewModelから直接Application.Current.Shutdown()を呼ばせないのはテスト容易性のため。
                mainViewModel.ShutdownRequested += (_, _) => Dispatcher.Invoke(Shutdown);

                _logService?.LogDetailed(LogLevel.Debug, "DIコンテナからMainViewModel取得完了", "App",
                                      "MVVM_CREATE", "ViewModel", "System", "MainViewModel", "Resolve", "Success");

                _logService?.LogDetailed(LogLevel.Information, "MainViewModel作成完了", "App",
                                      "MVVM_CREATE", "ViewModel", "System", "MainViewModel", "Create", "Success");
            }
            catch (Exception ex)
            {
                _logService?.LogDetailed(LogLevel.Error, $"MainViewModel作成エラー: {ex.Message}", "App",
                                      "MVVM_CREATE", "ViewModel", "System", "MainViewModel", "Create", "Failed", ex);
                throw;
            }

            // 起動引数でURLが指定されている場合は設定
            if (!string.IsNullOrEmpty(initialUrl))
            {
                if (Uri.TryCreate(initialUrl, UriKind.Absolute, out var uri))
                {
                    mainViewModel.SetInitialUrl(uri);
                }
                else
                {
                    mainViewModel.SetInitialUrl(new Uri(initialUrl));
                }
            }

            _logService?.LogInformation("テストモード: MainWindowを作成", "App");
            // 背景・サイズの初期設定はMainWindowのコンストラクタ（ApplyInitialBackgroundSettings/ApplyInitialSizeSettings）に一本化。
            // 従来ここでApp.xaml.cs側でも背景色を直接適用していたため、初期化経路が重複していた。
            // IThemeService/ISettingsServiceはPhase C-1のDWMバックドロップ適用（ダーク/ライト判定・ガラス効果設定）に使用。
            IThemeService themeServiceForWindow = _host.Services.GetRequiredService<IThemeService>();
            ISettingsService settingsServiceForWindow = _host.Services.GetRequiredService<ISettingsService>();
            Presentation.Views.MainWindow mainWindow = new(mainViewModel, _logService!, themeServiceForWindow, settingsServiceForWindow);
            _logService?.LogInformation("MainWindow作成完了", "App");

            // Phase D: --delay/--browser/--silent/--auto-launchオプションの適用
            ApplyCommandLineOptions(_commandLineOptions, mainWindow, mainViewModel);

            // Phase D: トレイ常駐設定に応じてトレイアイコンを準備し、✕での終了をトレイ格納に差し替える
            SetupTrayIcon(mainWindow, settingsServiceForWindow, localizationService);

            MainWindow = mainWindow;

            if (_commandLineOptions.Silent)
            {
                // --silent: UIを表示せず既定ブラウザへ直接遷移する
                _logService?.LogInformation("--silentオプションによりUIを表示せず既定ブラウザへ遷移します", "App");
                _ = mainViewModel.LaunchDefaultBrowserAsync();
            }
            else
            {
                _logService?.LogInformation("MainWindow表示開始", "App");
                mainWindow.Show(); // 起動時の背景色設定のため必要
                _logService?.LogInformation("MainWindow表示完了", "App");

                // Phase H-10: 起動を一切ブロックしない。ブラウザ検出・ウィンドウ表示・DWM適用が
                // 落ち着いてから走らせる（v0.2.0の起動速度対策を打ち消さないこと）。
                // --silentはUIが無く通知できないため対象外。
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), _updateCheckCts.Token).ConfigureAwait(false);
                        await TryCheckForUpdatesAsync(_updateCheckCts.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        // アプリ終了によるキャンセルは正常系のため何もしない
                    }
                });
            }

            base.OnStartup(e);
            _logService?.LogTrace($"アプリケーション起動処理完了: MainWindow表示済み, 初期URL={initialUrl ?? "なし"}", "App");
            _logService?.LogInformation("OnStartup完了", "App");
        }
        catch (Exception ex)
        {
            // テストモードの確認
            bool isTestMode = e.Args.Contains("--test-mode") ||
                             Environment.GetEnvironmentVariable("BROWSERSELECTOR_TEST_MODE") == "true";

            if (_logService != null)
            {
                _logService?.LogCritical($"アプリケーション起動で致命的エラーが発生: {ex.Message}", "App", ex);
            }
            else
            {
                // ログサービスが利用できない場合はログフォルダに出力
                string logFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BrowserSelector", "Logs");
                Directory.CreateDirectory(logFolder);
                string logPath = Path.Combine(logFolder, $"BrowserSelector_Startup_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [FATAL] [STARTUP_ERROR] [App] 起動エラー: {ex}\n");
                File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [FATAL] [STARTUP_ERROR] [App] スタックトレース: {ex.StackTrace}\n");
            }

            // テストモードの場合は例外を再スロー（メッセージボックスを表示しない）
            if (isTestMode)
            {
                throw new InvalidOperationException($"アプリケーションの起動に失敗しました: {ex.Message}", ex);
            }

            // 通常モードではメッセージボックスを表示
            _ = System.Windows.MessageBox.Show($"アプリケーションの起動に失敗しました: {ex.Message}",
                          "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    /// <inheritdoc/>
    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            _logService?.LogInformation("アプリケーション終了開始", "App");

            _updateCheckCts.Cancel();
            _updateCheckCts.Dispose();

            if (_singleInstanceManager != null)
            {
                _singleInstanceManager.UrlReceived -= OnUrlReceivedFromNewInstance;
                _singleInstanceManager.Dispose();
                _singleInstanceManager = null;
            }

            _trayIconManager?.Dispose();
            _trayIconManager = null;

            if (_host != null)
            {
                _host.StopAsync().GetAwaiter().GetResult();
                _host.Dispose();
            }

            _logService?.LogInformation("アプリケーション終了完了", "App");
        }
        // CA1031: アプリ起動/終了時の最上位フォールバック処理。DIコンテナ初期化やホスト起動、UI適用など例外種別が広範なため、アプリのクラッシュを防ぐ意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"アプリケーション終了エラー: {ex.Message}", "App", ex);
        }
        #pragma warning restore CA1031

        base.OnExit(e);
    }

    /// <summary>
    /// 後続インスタンスから転送されたURLを受信した際の処理.
    /// パイプ受信スレッド上で発火するためUIスレッドへディスパッチする.
    /// </summary>
    private void OnUrlReceivedFromNewInstance(object? sender, UrlReceivedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            _logService?.LogInformation($"後続インスタンスからURLを受信: {e.Url}", "App");

            if (MainWindow != null)
            {
                if (MainWindow.WindowState == WindowState.Minimized)
                {
                    MainWindow.WindowState = WindowState.Normal;
                }

                MainWindow.Show();
                _ = MainWindow.Activate();
            }

            if (!string.IsNullOrWhiteSpace(e.Url) && Uri.TryCreate(e.Url, UriKind.Absolute, out Uri? uri))
            {
                _mainViewModel?.SetInitialUrl(uri);
            }
        });
    }

    /// <summary>
    /// コマンドライン引数（Phase D: <c>-d/--delay</c>, <c>-b/--browser</c>, <c>--auto-launch</c>）を
    /// カウントダウンコントローラー・選択ブラウザへ適用する。<c>--silent</c>は呼び出し元で個別に扱う.
    /// </summary>
    private void ApplyCommandLineOptions(CommandLineOptions options, Presentation.Views.MainWindow mainWindow, MainViewModel mainViewModel)
    {
        if (options.Delay.HasValue)
        {
            mainWindow.Countdown.Start(options.Delay.Value);
            _logService?.LogInformation($"CLIオプションによりカウントダウン遅延を上書き: {options.Delay.Value}秒", "App");
        }

        if (options.BrowserId.HasValue)
        {
            Browser? requestedBrowser = mainViewModel.Browsers.FirstOrDefault(b => b.Id == options.BrowserId.Value);
            if (requestedBrowser != null)
            {
                mainViewModel.SelectedBrowser = requestedBrowser;
                _logService?.LogInformation($"CLIオプションによりブラウザを指定: {requestedBrowser.Name}", "App");
            }
            else
            {
                _logService?.LogWarning($"CLIオプションで指定されたブラウザGUIDが見つかりません: {options.BrowserId.Value}", "App");
            }
        }
    }

    /// <summary>
    /// トレイ常駐（Phase D: <see cref="AppSettings.AlwaysResidentInTray"/>）を準備する。
    /// 有効な場合、✕ボタン/システムメニューでの終了をトレイ格納に差し替え、
    /// トレイ格納中・復帰時にカウントダウンを確実に停止・リセットする（BrowserChooser3 v0.1.5 既知バグ対策）.
    /// </summary>
    private void SetupTrayIcon(Presentation.Views.MainWindow mainWindow, ISettingsService settingsService, ILocalizationService localizationService)
    {
        try
        {
            AppSettings appSettings = settingsService.LoadAppSettingsAsync().GetAwaiter().GetResult();
            if (!appSettings.AlwaysResidentInTray)
            {
                return;
            }

            IBrowserService browserService = _host!.Services.GetRequiredService<IBrowserService>();
            _trayIconManager = new TrayIconManager(mainWindow, browserService, localizationService);
            _trayIconManager.MinimizedToTray += (_, _) => mainWindow.Countdown.SuspendForTray();
            _trayIconManager.RestoredFromTray += (_, _) => mainWindow.Countdown.ResumeFromTray();

            mainWindow.Closing += (_, closingArgs) =>
            {
                if (_trayIconManager != null && !_trayIconManager.IsMinimizedToTray)
                {
                    closingArgs.Cancel = true;
                    _trayIconManager.MinimizeToTray();
                }
            };
        }
        // CA1031: トレイアイコン初期化失敗時はトレイ常駐無しで継続させるための意図的な汎用catch。
#pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"トレイアイコン初期化エラー: {ex.Message}", "App", ex);
        }
#pragma warning restore CA1031
    }

    /// <summary>
    /// 起動5秒後にバックグラウンドで自動アップデート確認を行う（Phase H-10）.
    /// <c>--silent</c>指定時は呼び出し元（<see cref="OnStartup(StartupEventArgs)"/>）で既に除外済み.
    /// </summary>
    private async Task TryCheckForUpdatesAsync(CancellationToken cancellationToken)
    {
        if (_host == null || _mainViewModel == null)
        {
            return;
        }

        try
        {
            IUpdateService updateService = _host.Services.GetRequiredService<IUpdateService>();
            ISettingsService settingsService = _host.Services.GetRequiredService<ISettingsService>();

            AppSettings appSettings = await settingsService.LoadAppSettingsAsync().ConfigureAwait(false);
            if (!appSettings.CheckForUpdates)
            {
                return;
            }

            bool isPendingLaunchCheck = appSettings.UpdatePendingOnNextLaunch;
            if (!isPendingLaunchCheck)
            {
                DateTimeOffset? lastCheck = appSettings.LastUpdateCheckUtc;
                if (lastCheck.HasValue && lastCheck.Value.AddHours(appSettings.UpdateCheckInterval) > DateTimeOffset.UtcNow)
                {
                    return;
                }
            }

            UpdateInfo? updateInfo = await updateService.CheckForUpdatesAsync(cancellationToken).ConfigureAwait(false);

            appSettings.LastUpdateCheckUtc = DateTimeOffset.UtcNow;
            if (isPendingLaunchCheck)
            {
                appSettings.UpdatePendingOnNextLaunch = false;
            }

            _ = await settingsService.SaveAppSettingsAsync(appSettings).ConfigureAwait(false);

            if (updateInfo == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(appSettings.SkippedUpdateVersion) &&
                string.Equals(appSettings.SkippedUpdateVersion, updateInfo.TagName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (updateInfo.IsPrerelease && !appSettings.IncludePrereleases)
            {
                return;
            }

            Dispatcher.Invoke(() => _mainViewModel.ShowUpdateNotification(updateInfo));
        }
        catch (OperationCanceledException)
        {
            // アプリ終了によるキャンセルは正常系のため何もしない
        }
        // CA1031: バックグラウンド自動確認処理の最上位フォールバック。ネットワーク・ファイルI/O等
        // 例外種別が多岐にわたり、UIに影響を与えず静かに失敗させるための意図的な汎用catch。
#pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogWarning($"自動アップデート確認でエラーが発生しました: {ex.Message}", "App");
        }
#pragma warning restore CA1031
    }
}

