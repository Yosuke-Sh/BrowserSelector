using System.Configuration;
using System.Data;
using System.Windows;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BrowserSelector.App.DependencyInjection;
using BrowserSelector.Presentation.ViewModels;
using BrowserSelector.Presentation.Views;
using BrowserSelector.Core.Services;
using BrowserSelector.Core.Enums;
using BrowserSelector.Presentation.Services;

namespace BrowserSelector.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IHost? _host;
    private ILogService? _logService;

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            // ホストの構築
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // BrowserSelectorサービスの追加
                    services.AddBrowserSelectorServices();
                })
                .Build();

            // ホストの開始
            await _host.StartAsync();

            // ログサービスの取得
            _logService = _host.Services.GetRequiredService<ILogService>();
            _logService.LogDetailed(LogLevel.Information, "アプリケーション起動開始", "App", 
                                  "STARTUP", "Application", "System", "App", "Initialize", "Started");

            // 不足アイコンの作成
            try
            {
                var iconService = new IconResourceService();
                var missingIcons = iconService.GetMissingIcons();
                if (missingIcons.Length > 0)
                {
                    var createdCount = iconService.CreateMissingIcons(missingIcons);
                    _logService.LogInformation($"不足アイコンを {createdCount} 個作成しました: {string.Join(", ", missingIcons)}", "App");
                }
            }
            catch (Exception iconEx)
            {
                _logService.LogError($"アイコン作成エラー: {iconEx.Message}", "App", iconEx);
            }

            // 起動引数からURLを取得
            string? initialUrl = null;
            if (e.Args.Length > 0)
            {
                initialUrl = e.Args[0];
                _logService.LogDetailed(LogLevel.Debug, $"起動引数でURLを受信: {initialUrl}", "App", 
                                      "ARGS", initialUrl, "System", "Args", "Parse", "Success");
            }

            // メインウィンドウの作成と表示
            MainViewModel mainViewModel;
            try
            {
                _logService.LogDetailed(LogLevel.Information, "MainViewModel作成開始", "App", 
                                      "MVVM_CREATE", "ViewModel", "System", "MainViewModel", "Create", "Started");
                
                _logService.LogDetailed(LogLevel.Debug, "DIコンテナからMainViewModelを取得開始", "App", 
                                      "MVVM_CREATE", "ViewModel", "System", "MainViewModel", "Resolve", "Started");
                
                mainViewModel = _host.Services.GetRequiredService<MainViewModel>();
                
                _logService.LogDetailed(LogLevel.Debug, "DIコンテナからMainViewModel取得完了", "App", 
                                      "MVVM_CREATE", "ViewModel", "System", "MainViewModel", "Resolve", "Success");
                
                _logService.LogDetailed(LogLevel.Information, "MainViewModel作成完了", "App", 
                                      "MVVM_CREATE", "ViewModel", "System", "MainViewModel", "Create", "Success");
            }
            catch (Exception ex)
            {
                _logService.LogDetailed(LogLevel.Error, $"MainViewModel作成エラー: {ex.Message}", "App", 
                                      "MVVM_CREATE", "ViewModel", "System", "MainViewModel", "Create", "Failed", ex);
                throw;
            }
            
            // 起動引数でURLが指定されている場合は設定
            if (!string.IsNullOrEmpty(initialUrl))
            {
                mainViewModel.SetInitialUrl(initialUrl);
            }
            
            var mainWindow = new BrowserSelector.Presentation.Views.MainWindow(mainViewModel);
            _logService.LogInformation("MainWindow作成完了", "App");
            
            // 起動時設定読み込み（適用を実行）
            try
            {
                var settingsSvc = _host.Services.GetRequiredService<ISettingsService>();
                var v = await settingsSvc.LoadVisualSettingsAsync();
                _logService.LogDebug($"Startup.VisualSettings.Load.Success BackgroundColor={v.BackgroundColor}, UseBackgroundGradient={v.UseBackgroundGradient}, GradientDirection={v.GradientDirection}", "App");

                // 起動時即座に背景色設定を実行
                _logService.LogDebug("Startup.VisualSettings.Apply.Start Target=MainWindow (Immediate)", "App");
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
                            GradientStops = new System.Windows.Media.GradientStopCollection
                            {
                                new System.Windows.Media.GradientStop(v.GradientStartColor, 0),
                                new System.Windows.Media.GradientStop(v.GradientEndColor, 1)
                            }
                        };
                        _logService.LogDebug($"起動時背景グラデーション設定完了: 方向={v.GradientDirection}, 開始色={v.GradientStartColor}, 終了色={v.GradientEndColor}", "App");
                    }
                    else
                    {
                        var brush = new System.Windows.Media.SolidColorBrush(v.BackgroundColor);
                        mainWindow.Background = brush;
                        _logService.LogDebug($"起動時背景色設定完了: 設定値={v.BackgroundColor}, 適用後={mainWindow.Background}", "App");
                    }

                    // VisualSettingsを設定
                    mainViewModel.VisualSettings = v;
                    _logService.LogDebug("起動時VisualSettings設定完了", "App");

                    _logService.LogDebug("Startup.VisualSettings.Apply.Success Target=MainWindow (Immediate)", "App");
                }
                catch (Exception aex)
                {
                    _logService.LogDebug($"Startup.VisualSettings.Apply.Error {aex.Message}", "App", aex);
                }

                // 追加でLoadedイベントでも設定を適用（二重適用防止のため条件付き）
                mainWindow.Loaded += (_, __) =>
                {
                    _logService.LogDebug("Startup.VisualSettings.Apply.Start Target=MainWindow (Loaded Event)", "App");
                    try
                    {
                        // 既に設定済みの場合はスキップ
                        if (mainWindow.Background is System.Windows.Media.SolidColorBrush currentBrush)
                        {
                            var currentColor = currentBrush.Color;
                            if (currentColor == v.BackgroundColor)
                            {
                                _logService.LogDebug("起動時背景色設定は既に適用済みです", "App");
                                return;
                            }
                        }

                        // 背景色を再適用
                        if (!v.UseBackgroundGradient)
                        {
                            var brush = new System.Windows.Media.SolidColorBrush(v.BackgroundColor);
                            mainWindow.Background = brush;
                            _logService.LogDebug($"Loadedイベントで背景色再適用完了: {v.BackgroundColor}", "App");
                        }

                        _logService.LogDebug("Startup.VisualSettings.Apply.Success Target=MainWindow (Loaded Event)", "App");
                    }
                    catch (Exception aex)
                    {
                        _logService.LogDebug($"Startup.VisualSettings.Apply.Error (Loaded Event) {aex.Message}", "App", aex);
                    }
                };
            }
            catch (Exception vex)
            {
                _logService.LogDebug($"Startup.VisualSettings.Load.Error {vex.Message}", "App", vex);
            }
            
            MainWindow = mainWindow;
            _logService.LogInformation("MainWindow表示開始", "App");
            mainWindow.Show(); // 起動時の背景色設定のため必要
            _logService.LogInformation("MainWindow表示完了", "App");

            base.OnStartup(e);
            _logService.LogInformation("OnStartup完了", "App");
        }
        catch (Exception ex)
        {
            if (_logService != null)
            {
                _logService.LogError($"アプリケーションの起動に失敗しました: {ex.Message}", "App", ex);
            }
            else
            {
                // ログサービスが利用できない場合はファイルに出力
                var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "BrowserSelector_Startup.log");
                File.AppendAllText(logPath, $"起動エラー: {ex}\n");
                File.AppendAllText(logPath, $"スタックトレース: {ex.StackTrace}\n");
            }
            
            MessageBox.Show($"アプリケーションの起動に失敗しました: {ex.Message}", 
                          "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            if (_logService != null)
            {
                _logService.LogInformation("アプリケーション終了開始", "App");
            }
            
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
            
            if (_logService != null)
            {
                _logService.LogInformation("アプリケーション終了完了", "App");
            }
        }
        catch (Exception ex)
        {
            if (_logService != null)
            {
                _logService.LogError($"アプリケーション終了エラー: {ex.Message}", "App", ex);
            }
            else
            {
                // アプリケーション終了エラーは無視
            }
        }

        base.OnExit(e);
    }
}

