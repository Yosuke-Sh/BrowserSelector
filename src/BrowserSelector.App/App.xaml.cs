using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BrowserSelector.App.DependencyInjection;
using BrowserSelector.Presentation.ViewModels;
using BrowserSelector.Presentation.Views;

namespace BrowserSelector.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IHost? _host;

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

            // 起動引数からURLを取得
            string? initialUrl = null;
            if (e.Args.Length > 0)
            {
                initialUrl = e.Args[0];
                System.Diagnostics.Debug.WriteLine($"起動引数でURLを受信: {initialUrl}");
            }

            // メインウィンドウの作成と表示
            var mainViewModel = _host.Services.GetRequiredService<MainViewModel>();
            
            // 起動引数でURLが指定されている場合は設定
            if (!string.IsNullOrEmpty(initialUrl))
            {
                mainViewModel.SetInitialUrl(initialUrl);
            }
            
            var mainWindow = new BrowserSelector.Presentation.Views.MainWindow(mainViewModel);
            
            MainWindow = mainWindow;
            MainWindow.Show();

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"アプリケーションの起動に失敗しました: {ex.Message}", 
                          "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
        }
        catch (Exception ex)
        {
            // ログ出力（後で実装）
            System.Diagnostics.Debug.WriteLine($"アプリケーション終了エラー: {ex.Message}");
        }

        base.OnExit(e);
    }
}

