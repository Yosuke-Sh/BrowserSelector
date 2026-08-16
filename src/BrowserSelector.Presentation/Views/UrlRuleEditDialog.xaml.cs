using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Presentation.Helpers;
using System.Collections.ObjectModel;
using System.Windows;

namespace BrowserSelector.Presentation.Views;

/// <summary>
/// URLルール編集ダイアログ.
/// </summary>
public partial class UrlRuleEditDialog : Window
{
    private readonly IBrowserService _browserService;
    private readonly ILogService _logService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlRuleEditDialog"/> class.
    /// </summary>
    /// <param name="browserService">browserService.</param>
    /// <param name="logService">logService.</param>
    public UrlRuleEditDialog(IBrowserService browserService, ILogService logService)
    {
        InitializeComponent();
        DataContext = this;

        _browserService = browserService;
        _logService = logService;

        LoadBrowsers();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlRuleEditDialog"/> class.
    /// </summary>
    /// <param name="urlRule">urlRule.</param>
    /// <param name="browserService">browserService.</param>
    /// <param name="logService">logService.</param>
    public UrlRuleEditDialog(UrlRule urlRule, IBrowserService browserService, ILogService logService)
        : this(browserService, logService)
    {
        UrlRule = urlRule;
        SelectedBrowser = AvailableBrowsers.FirstOrDefault(b => b.Name == urlRule.BrowserName);
    }

    /// <summary>
    /// Gets or sets urlRule.
    /// </summary>
    public UrlRule UrlRule { get; set; } = new();

    /// <summary>
    /// Gets availableBrowsers.
    /// </summary>
    public ObservableCollection<Browser> AvailableBrowsers { get; } = new();

    /// <summary>
    /// Gets or sets selectedBrowser.
    /// </summary>
    public Browser? SelectedBrowser { get; set; }

    private async void LoadBrowsers()
    {
        try
        {
            IEnumerable<Browser> browsers = await _browserService.GetAllBrowsersAsync().ConfigureAwait(false);
            AvailableBrowsers.Clear();
            foreach (Browser? browser in browsers.Where(b => b.IsEnabled))
            {
                AvailableBrowsers.Add(browser);
            }
        }
        // CA1031: イベントハンドラーの最上位try-catch。サービス呼び出しやバリデーション処理由来の例外種別が多岐にわたり、UIスレッドをクラッシュさせないための意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"ブラウザ一覧読み込みエラー: {ex.Message}", "UrlRuleEditDialog", ex);
        }
        #pragma warning restore CA1031
    }


    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // バリデーション
            if (string.IsNullOrWhiteSpace(UrlRule.Pattern))
            {
                _ = LocalizedMessageBox.ShowError("Dialog.UrlRuleEdit.EnterPattern", "MessageBox.InputError");
                return;
            }

            if (SelectedBrowser == null)
            {
                _ = LocalizedMessageBox.ShowError("Dialog.UrlRuleEdit.SelectBrowser", "MessageBox.InputError");
                return;
            }

            // ブラウザ名を設定
            UrlRule.BrowserName = SelectedBrowser.Name;
            UrlRule.UpdatedAt = DateTime.Now;

            DialogResult = true;
            Close();
        }
        // CA1031: イベントハンドラーの最上位try-catch。サービス呼び出しやバリデーション処理由来の例外種別が多岐にわたり、UIスレッドをクラッシュさせないための意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"URLルール保存エラー: {ex.Message}", "UrlRuleEditDialog", ex);
            _ = LocalizedMessageBox.ShowError($"Dialog.UrlRuleEdit.SaveError: {ex.Message}", "MessageBox.Error");
        }
        #pragma warning restore CA1031
    }
}
