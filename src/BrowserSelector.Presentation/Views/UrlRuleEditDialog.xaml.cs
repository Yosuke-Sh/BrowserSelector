using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace BrowserSelector.Presentation.Views;

/// <summary>
/// URLルール編集ダイアログ
/// </summary>
public partial class UrlRuleEditDialog : Window
{
    private readonly IBrowserService _browserService;
    private readonly ILogService _logService;

    public UrlRule UrlRule { get; set; } = new();
    public ObservableCollection<Browser> AvailableBrowsers { get; set; } = new();
    public Browser? SelectedBrowser { get; set; }

    public UrlRuleEditDialog(IBrowserService browserService, ILogService logService)
    {
        InitializeComponent();
        DataContext = this;

        _browserService = browserService;
        _logService = logService;

        LoadBrowsers();
    }

    public UrlRuleEditDialog(UrlRule urlRule, IBrowserService browserService, ILogService logService)
        : this(browserService, logService)
    {
        UrlRule = urlRule;
        SelectedBrowser = AvailableBrowsers.FirstOrDefault(b => b.Name == urlRule.BrowserName);
    }

    private async void LoadBrowsers()
    {
        try
        {
            var browsers = await _browserService.GetAllBrowsersAsync();
            AvailableBrowsers.Clear();
            foreach (var browser in browsers.Where(b => b.IsEnabled))
            {
                AvailableBrowsers.Add(browser);
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"ブラウザ一覧読み込みエラー: {ex.Message}", "UrlRuleEditDialog", ex);
        }
    }


    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // バリデーション
            if (string.IsNullOrWhiteSpace(UrlRule.Pattern))
            {
                MessageBox.Show("パターンを入力してください。", "入力エラー",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedBrowser == null)
            {
                MessageBox.Show("ブラウザを選択してください。", "入力エラー",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // ブラウザ名を設定
            UrlRule.BrowserName = SelectedBrowser.Name;
            UrlRule.UpdatedAt = DateTime.Now;

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            _logService?.LogError($"URLルール保存エラー: {ex.Message}", "UrlRuleEditDialog", ex);
            MessageBox.Show($"保存中にエラーが発生しました: {ex.Message}", "エラー",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
