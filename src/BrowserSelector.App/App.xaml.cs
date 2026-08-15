using BrowserSelector.App.DependencyInjection;
using BrowserSelector.Core.Enums;
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
public partial class App : Application
#pragma warning restore CA1515, CA1001
{
    private IHost? _host;
    private ILogService? _logService;
    private SingleInstanceManager? _singleInstanceManager;
    private MainViewModel? _mainViewModel;

    /// <inheritdoc/>
    protected override void OnStartup(StartupEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        try
        {
            // テストモードの確認
            bool isTestMode = e.Args.Contains("--test-mode") ||
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

            // 起動引数からURLを取得
            string? initialUrl = null;
            if (e.Args.Length > 0)
            {
                initialUrl = e.Args[0];
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
            Presentation.Views.MainWindow mainWindow = new(mainViewModel, _logService!);
            _logService?.LogInformation("MainWindow作成完了", "App");

            MainWindow = mainWindow;
            _logService?.LogInformation("MainWindow表示開始", "App");
            mainWindow.Show(); // 起動時の背景色設定のため必要
            _logService?.LogInformation("MainWindow表示完了", "App");

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
            _ = MessageBox.Show($"アプリケーションの起動に失敗しました: {ex.Message}",
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

            if (_singleInstanceManager != null)
            {
                _singleInstanceManager.UrlReceived -= OnUrlReceivedFromNewInstance;
                _singleInstanceManager.Dispose();
                _singleInstanceManager = null;
            }

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
}

