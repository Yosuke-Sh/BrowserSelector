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
            
            MainWindow = mainWindow;
            _logService.LogInformation("MainWindow表示開始", "App");
            // MainWindow.Show(); // MainWindow.xaml.csで表示するため削除
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
                System.Diagnostics.Debug.WriteLine($"アプリケーション終了エラー: {ex.Message}");
            }
        }

        base.OnExit(e);
    }
}

