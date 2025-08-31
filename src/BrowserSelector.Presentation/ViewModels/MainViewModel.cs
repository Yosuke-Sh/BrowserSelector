using System.Collections.ObjectModel;
using System.Windows.Input;
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
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

    public MainViewModel(
        IBrowserService browserService,
        ISettingsService settingsService,
        ILocalizationService localizationService)
    {
        _browserService = browserService;
        _settingsService = settingsService;
        _localizationService = localizationService;

        // コマンドの初期化
        LoadBrowsersCommand = new AsyncRelayCommand(LoadBrowsersAsync);
        LaunchBrowserCommand = new AsyncRelayCommand<Browser>(LaunchBrowserAsync, CanLaunchBrowser);
        OpenSettingsCommand = new RelayCommand(OpenSettings);
        CloseSettingsCommand = new RelayCommand(CloseSettings);
        ClearUrlCommand = new RelayCommand(ClearUrl);

        // 初期化時にブラウザ一覧を読み込み
        _ = LoadBrowsersAsync();
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
            IsLoading = true;
            StatusMessage = "ブラウザを検出中...";

            var browsers = await _browserService.GetAllBrowsersAsync();
            
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

            StatusMessage = $"ブラウザ {Browsers.Count} 個を読み込みました";
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
            var settingsWindow = new Views.SettingsWindow(
                new SettingsViewModel(_settingsService, _browserService, _localizationService));
            
            settingsWindow.ShowDialog();
            
            // 設定画面が閉じられた後、ブラウザリストを再読み込み
            _ = LoadBrowsersAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"設定画面を開くエラー: {ex.Message}");
            MessageBox.Show($"設定画面を開けませんでした: {ex.Message}", "エラー", 
                          MessageBoxButton.OK, MessageBoxImage.Error);
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
    public void SetInitialUrl(string url)
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            Url = url;
            System.Diagnostics.Debug.WriteLine($"初期URLを設定: {url}");
        }
    }

    /// <summary>
    /// URLが変更された時の処理
    /// </summary>
    partial void OnUrlChanged(string value)
    {
        LaunchBrowserCommand.NotifyCanExecuteChanged();
    }
}
