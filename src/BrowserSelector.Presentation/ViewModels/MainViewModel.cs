using System.Collections.ObjectModel;
using System.Windows.Input;
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
            StatusMessage = _localizationService.GetString("LoadingBrowsers");

            var browsers = await _browserService.GetAllBrowsersAsync();
            
            Browsers.Clear();
            foreach (var browser in browsers.Where(b => b.IsEnabled))
            {
                Browsers.Add(browser);
            }

            StatusMessage = _localizationService.GetString("BrowsersLoaded", Browsers.Count);
        }
        catch (Exception ex)
        {
            StatusMessage = _localizationService.GetString("ErrorLoadingBrowsers", ex.Message);
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
            StatusMessage = _localizationService.GetString("LaunchingBrowser", browser.Name);

            var success = await _browserService.LaunchBrowserAsync(browser, Url);
            
            if (success)
            {
                await _browserService.UpdateUsageAsync(browser);
                browser.IncrementUseCount();
                StatusMessage = _localizationService.GetString("BrowserLaunched", browser.Name);
            }
            else
            {
                StatusMessage = _localizationService.GetString("ErrorLaunchingBrowser", browser.Name);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = _localizationService.GetString("ErrorLaunchingBrowser", ex.Message);
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
        IsSettingsVisible = true;
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
    /// URLが変更された時の処理
    /// </summary>
    partial void OnUrlChanged(string value)
    {
        LaunchBrowserCommand.NotifyCanExecuteChanged();
    }
}
