using BrowserSelector.App.DependencyInjection;
using BrowserSelector.Core.Enums;
using BrowserSelector.Core.Services;
using BrowserSelector.Presentation.Extensions;
using BrowserSelector.Presentation.Helpers;
using BrowserSelector.Presentation.Services;
using BrowserSelector.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;

namespace BrowserSelector.App;

/// <summary>
/// Interaction logic for App.xaml.
/// </summary>
public partial class App : Application
{
    private IHost? _host;
    private ILogService? _logService;

    /// <inheritdoc/>
    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            // テストモードの確認
            bool isTestMode = e.Args.Contains("--test-mode") ||
                             Environment.GetEnvironmentVariable("BROWSERSELECTOR_TEST_MODE") == "true";

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

            // 設定された言語を適用
            try
            {
                ISettingsService settingsService = _host.Services.GetRequiredService<ISettingsService>();
                Core.Models.AppSettings appSettings = settingsService.LoadAppSettingsAsync().GetAwaiter().GetResult();
                System.Globalization.CultureInfo culture = new(appSettings.Language);
                localizationService.SetLanguage(culture).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                // 設定読み込みに失敗した場合はデフォルト言語（日本語）を使用
                _logService?.LogWarning($"設定読み込みに失敗、デフォルト言語を使用: {ex.Message}", "App");
                System.Globalization.CultureInfo defaultCulture = new("ja-JP");
                localizationService.SetLanguage(defaultCulture).GetAwaiter().GetResult();
            }

            // 不足アイコンの作成
            try
            {
                IconResourceService iconService = new();
                string[] missingIcons = iconService.GetMissingIcons();
                if (missingIcons.Length > 0)
                {
                    int createdCount = iconService.CreateMissingIcons(missingIcons);
                    _logService?.LogInformation($"不足アイコンを {createdCount} 個作成しました: {string.Join(", ", missingIcons)}", "App");
                }
            }
            catch (Exception iconEx)
            {
                _logService?.LogError($"アイコン作成エラー: {iconEx.Message}", "App", iconEx);
            }

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
            Presentation.Views.MainWindow mainWindow = new(mainViewModel, _logService!);
            _logService?.LogInformation("MainWindow作成完了", "App");

            // MainViewModelで既にVisualSettingsが読み込まれているので、それを取得
            Core.Models.VisualSettings v = mainViewModel.VisualSettings;
            _logService?.LogDebug($"Startup.VisualSettings.Load.Success BackgroundColor={v.BackgroundColor}, UseBackgroundGradient={v.UseBackgroundGradient}, GradientDirection={v.GradientDirection}", "App");

            // 起動時即座に背景色設定を実行
            _logService?.LogDebug("Startup.VisualSettings.Apply.Start Target=MainWindow (Immediate)", "App");
            try
            {
                // 背景（グラデーション or 単色）
                if (v.UseBackgroundGradient)
                {
                    // グラデーション方向に応じてStartPointとEndPointを設定
                    System.Windows.Point startPoint, endPoint;
                    switch (v.GradientDirection)
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
                        GradientStops =
                        [
                            new System.Windows.Media.GradientStop(v.GradientStartColor, 0),
                            new System.Windows.Media.GradientStop(v.GradientEndColor, 1)
                        ]
                    };
                    _logService?.LogDebug($"起動時背景グラデーション設定完了: 方向={v.GradientDirection}, 開始色={v.GradientStartColor}, 終了色={v.GradientEndColor}", "App");
                }
                else
                {
                    System.Windows.Media.SolidColorBrush brush = new(v.BackgroundColor);
                    mainWindow.Background = brush;
                    _logService?.LogDebug($"起動時背景色設定完了: 設定値={v.BackgroundColor}, 適用後={mainWindow.Background}", "App");
                }

                _logService?.LogDebug("Startup.VisualSettings.Apply.Success Target=MainWindow (Immediate)", "App");
            }
            catch (Exception aex)
            {
                _logService?.LogDebug($"Startup.VisualSettings.Apply.Error {aex.Message}", "App", aex);
            }

            // 追加でLoadedイベントでも設定を適用（二重適用防止のため条件付き）
            mainWindow.Loaded += (_, __) =>
            {
                _logService?.LogDebug("Startup.VisualSettings.Apply.Start Target=MainWindow (Loaded Event)", "App");
                try
                {
                    // 既に設定済みの場合はスキップ
                    if (mainWindow.Background is System.Windows.Media.SolidColorBrush currentBrush)
                    {
                        System.Windows.Media.Color currentColor = currentBrush.Color;
                        if (currentColor == v.BackgroundColor)
                        {
                            _logService?.LogDebug("起動時背景色設定は既に適用済みです", "App");
                            return;
                        }
                    }

                    // 背景色を再適用
                    if (!v.UseBackgroundGradient)
                    {
                        System.Windows.Media.SolidColorBrush brush = new(v.BackgroundColor);
                        mainWindow.Background = brush;
                        _logService?.LogDebug($"Loadedイベントで背景色再適用完了: {v.BackgroundColor}", "App");
                    }

                    _logService?.LogDebug("Startup.VisualSettings.Apply.Success Target=MainWindow (Loaded Event)", "App");
                }
                catch (Exception aex)
                {
                    _logService?.LogDebug($"Startup.VisualSettings.Apply.Error (Loaded Event) {aex.Message}", "App", aex);
                }
            };

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

            if (_host != null)
            {
                _host.StopAsync().GetAwaiter().GetResult();
                _host.Dispose();
            }

            _logService?.LogInformation("アプリケーション終了完了", "App");
        }
        catch (Exception ex)
        {
            _logService?.LogError($"アプリケーション終了エラー: {ex.Message}", "App", ex);
        }

        base.OnExit(e);
    }
}

