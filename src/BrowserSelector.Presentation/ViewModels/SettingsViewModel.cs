using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;

namespace BrowserSelector.Presentation.ViewModels;

/// <summary>
/// 設定画面のViewModel
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IBrowserService _browserService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private AppSettings _appSettings = new();

    [ObservableProperty]
    private VisualSettings _visualSettings = new();

    [ObservableProperty]
    private ObservableCollection<Browser> _detectedBrowsers = new();

    [ObservableProperty]
    private ObservableCollection<LanguageInfo> _availableLanguages = new();

    [ObservableProperty]
    private LanguageInfo? _selectedLanguage;

    [ObservableProperty]
    private ObservableCollection<string> _logLevels = new()
    {
        "Trace", "Debug", "Information", "Warning", "Error", "Critical"
    };

    [ObservableProperty]
    private string _selectedLogLevel = "Information";

    [ObservableProperty]
    private bool _showFocusIndicator = true;

    [ObservableProperty]
    private Color _focusColor = Colors.Blue;

    [ObservableProperty]
    private double _focusThickness = 2.0;

    [ObservableProperty]
    private bool _enableKeyboardNavigation = true;

    [ObservableProperty]
    private bool _enableShortcuts = true;

    [ObservableProperty]
    private bool _enableScreenReaderSupport = true;

    [ObservableProperty]
    private bool _provideDetailedDescriptions = true;

    public SettingsViewModel(
        ISettingsService settingsService,
        IBrowserService browserService,
        ILocalizationService localizationService)
    {
        _settingsService = settingsService;
        _browserService = browserService;
        _localizationService = localizationService;

        InitializeAsync();
    }

    /// <summary>
    /// 初期化処理
    /// </summary>
    private async void InitializeAsync()
    {
        try
        {
            // 設定を読み込み
            AppSettings = await _settingsService.LoadAppSettingsAsync();
            VisualSettings = await _settingsService.LoadVisualSettingsAsync();

            // 言語リストを初期化
            InitializeLanguages();

            // ブラウザリストを更新
            await RefreshBrowsersAsync();

            // プロパティ変更イベントを監視
            PropertyChanged += OnPropertyChanged;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"設定画面の初期化エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// 言語リストを初期化
    /// </summary>
    private void InitializeLanguages()
    {
        AvailableLanguages.Clear();
        AvailableLanguages.Add(new LanguageInfo("ja-JP", "日本語"));
        AvailableLanguages.Add(new LanguageInfo("en-US", "English"));

        // 現在の言語を選択
        SelectedLanguage = AvailableLanguages.FirstOrDefault(l => l.CultureCode == AppSettings.Language) 
                          ?? AvailableLanguages.First();
    }

    /// <summary>
    /// ブラウザリストを更新
    /// </summary>
    private async Task RefreshBrowsersAsync()
    {
        try
        {
            var browsers = await _browserService.GetAllBrowsersAsync();
            DetectedBrowsers.Clear();
            foreach (var browser in browsers)
            {
                DetectedBrowsers.Add(browser);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ブラウザリスト更新エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// プロパティ変更時の処理
    /// </summary>
    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // リアルタイムプレビューの更新
        if (e.PropertyName == nameof(VisualSettings) || 
            e.PropertyName == "VisualSettings.Opacity" ||
            e.PropertyName == "VisualSettings.TransparencyColor" ||
            e.PropertyName == "VisualSettings.CornerRadius" ||
            e.PropertyName == "VisualSettings.BackgroundColor" ||
            e.PropertyName == "VisualSettings.UseBackgroundGradient")
        {
            await UpdateVisualSettingsAsync();
        }

        // 言語変更時の処理
        if (e.PropertyName == nameof(SelectedLanguage) && SelectedLanguage != null)
        {
            AppSettings.Language = SelectedLanguage.CultureCode;
            _localizationService.SetLanguage(new System.Globalization.CultureInfo(SelectedLanguage.CultureCode));
        }
    }

    /// <summary>
    /// 視覚設定を更新
    /// </summary>
    private async Task UpdateVisualSettingsAsync()
    {
        try
        {
            await _settingsService.SaveVisualSettingsAsync(VisualSettings);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"視覚設定更新エラー: {ex.Message}");
        }
    }

    #region Commands

    /// <summary>
    /// ブラウザを再検出するコマンド
    /// </summary>
    [RelayCommand]
    private async Task RefreshBrowsers()
    {
        await RefreshBrowsersAsync();
    }

    /// <summary>
    /// ブラウザを追加するコマンド
    /// </summary>
    [RelayCommand]
    private Task AddBrowser()
    {
        // TODO: ブラウザ追加ダイアログを実装
        System.Diagnostics.Debug.WriteLine("ブラウザ追加機能は未実装です");
        return Task.CompletedTask;
    }

    /// <summary>
    /// 設定をリセットするコマンド
    /// </summary>
    [RelayCommand]
    private async Task ResetSettings()
    {
        try
        {
            var result = await _settingsService.ResetSettingsAsync();
            if (result)
            {
                // 設定を再読み込み
                AppSettings = await _settingsService.LoadAppSettingsAsync();
                VisualSettings = await _settingsService.LoadVisualSettingsAsync();
                InitializeLanguages();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"設定リセットエラー: {ex.Message}");
        }
    }

    /// <summary>
    /// 設定をインポートするコマンド
    /// </summary>
    [RelayCommand]
    private async Task ImportSettings()
    {
        try
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "設定ファイルを選択"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var result = await _settingsService.ImportSettingsAsync(openFileDialog.FileName);
                if (result)
                {
                    // 設定を再読み込み
                    AppSettings = await _settingsService.LoadAppSettingsAsync();
                    VisualSettings = await _settingsService.LoadVisualSettingsAsync();
                    InitializeLanguages();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"設定インポートエラー: {ex.Message}");
        }
    }

    /// <summary>
    /// 設定をエクスポートするコマンド
    /// </summary>
    [RelayCommand]
    private async Task ExportSettings()
    {
        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "設定ファイルを保存",
                FileName = $"browserselector_settings_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var exportData = new
                {
                    AppSettings = AppSettings,
                    VisualSettings = VisualSettings,
                    ExportDate = DateTime.Now
                };

                var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(saveFileDialog.FileName, json);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"設定エクスポートエラー: {ex.Message}");
        }
    }

    /// <summary>
    /// 設定を保存するコマンド
    /// </summary>
    [RelayCommand]
    private async Task SaveSettings()
    {
        try
        {
            // アプリケーション設定を保存
            var appSettingsResult = await _settingsService.SaveAppSettingsAsync(AppSettings);
            
            // 視覚設定を保存
            var visualSettingsResult = await _settingsService.SaveVisualSettingsAsync(VisualSettings);

            if (appSettingsResult && visualSettingsResult)
            {
                // 成功時はウィンドウを閉じる
                if (Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this) is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"設定保存エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// キャンセルコマンド
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        // ウィンドウを閉じる
        if (Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this) is Window window)
        {
            window.DialogResult = false;
            window.Close();
        }
    }

    #endregion
}

/// <summary>
/// 言語情報を表すクラス
/// </summary>
public class LanguageInfo
{
    public string CultureCode { get; set; }
    public string DisplayName { get; set; }

    public LanguageInfo(string cultureCode, string displayName)
    {
        CultureCode = cultureCode;
        DisplayName = displayName;
    }
}
