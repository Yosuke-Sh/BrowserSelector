using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Presentation.Helpers;
using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace BrowserSelector.Presentation.Views;

/// <summary>
/// BrowserEditDialog.xaml の相互作用ロジック.
/// </summary>
public partial class BrowserEditDialog : Window
{
    private readonly bool _isNewBrowser;
    private readonly ILogService? _logService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserEditDialog"/> class.
    /// </summary>
    /// <param name="browser">browser.</param>
    /// <param name="logService">logService.</param>
    public BrowserEditDialog(Browser? browser = null, ILogService? logService = null)
    {
        _logService = logService;
        InitializeComponent();

        if (browser == null)
        {
            // 新規作成
            _isNewBrowser = true;
            Browser = new Browser
            {
                Id = Guid.NewGuid(),
                Name = "新しいブラウザ",
                ExecutablePath = string.Empty,
                IconPath = string.Empty,
                Arguments = string.Empty,
                Type = BrowserType.Custom,
                IsEnabled = true,
                DisplayOrder = 0
            };
        }
        else
        {
            // 編集
            _isNewBrowser = false;
            Browser = browser.Clone(); // 複製して編集
        }

        DataContext = Browser;
        Title = _isNewBrowser ? LocalizedLogHelper.GetString("Dialog.BrowserEdit.AddTitle") :
                LocalizedLogHelper.GetString("Dialog.BrowserEdit.EditTitle");
    }

    /// <summary>
    /// Gets browser.
    /// </summary>
    public Browser Browser { get; private set; }

    /// <summary>
    /// BrowseExecutable_Click.
    /// </summary>
    /// <param name="sender">sender.</param>
    /// <param name="e">e.</param>
    private void BrowseExecutable_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new()
        {
            Title = "ブラウザの実行ファイルを選択",
            Filter = "実行ファイル (*.exe)|*.exe|すべてのファイル (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
        };

        if (openFileDialog.ShowDialog() == true)
        {
            Browser.ExecutablePath = openFileDialog.FileName;

            // 実行ファイルが選択された場合、自動的にアイコンパスを設定
            // 既存のアイコンが設定されていない場合のみ自動設定
            if (string.IsNullOrEmpty(Browser.IconPath))
            {
                Browser.IconPath = openFileDialog.FileName;
                _logService?.LogInformation($"アイコンを実行ファイルから自動設定: {openFileDialog.FileName}", "BrowserEditDialog");
            }

            // 名前がデフォルトの場合、ファイル名から自動設定
            if (Browser.Name == "新しいブラウザ" || string.IsNullOrEmpty(Browser.Name))
            {
                Browser.Name = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                _logService?.LogInformation($"ブラウザ名を実行ファイルから自動設定: {Browser.Name}", "BrowserEditDialog");
            }
        }
    }

    /// <summary>
    /// BrowseIcon_Click.
    /// </summary>
    /// <param name="sender">sender.</param>
    /// <param name="e">e.</param>
    private void BrowseIcon_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new()
        {
            Title = "アイコンファイルを選択",
            Filter = "アイコンファイル (*.ico;*.exe)|*.ico;*.exe|画像ファイル (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|すべてのファイル (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
        };

        if (openFileDialog.ShowDialog() == true)
        {
            Browser.IconPath = openFileDialog.FileName;
        }
    }

    private void SelectIcon_Click(object sender, RoutedEventArgs e)
    {
        IconSelectionDialog dialog = new(_logService);

        // 実行ファイルが設定されている場合、そのアイコンを優先表示
        if (!string.IsNullOrEmpty(Browser.ExecutablePath) && File.Exists(Browser.ExecutablePath))
        {
            dialog.LoadExecutableIcon(Browser.ExecutablePath);
        }
        // 現在のアイコンパスが設定されている場合、そのファイルを表示
        else if (!string.IsNullOrEmpty(Browser.IconPath) && File.Exists(Browser.IconPath))
        {
            dialog.LoadExecutableIcon(Browser.IconPath);
        }

        if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.SelectedIconPath))
        {
            Browser.IconPath = dialog.SelectedIconPath;
            Browser.IconIndex = dialog.SelectedIconIndex;
            _logService?.LogInformation($"アイコンが選択されました: {dialog.SelectedIconPath}, インデックス: {dialog.SelectedIconIndex}", "BrowserEditDialog");
        }
    }

    private void MoveUp_Click(object sender, RoutedEventArgs e)
    {
        if (Browser.DisplayOrder > 0)
        {
            Browser.DisplayOrder--;
        }
    }

    private void MoveDown_Click(object sender, RoutedEventArgs e)
    {
        Browser.DisplayOrder++;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        // バリデーション
        if (string.IsNullOrWhiteSpace(Browser.Name))
        {
            _ = LocalizedMessageBox.ShowError("Dialog.BrowserEdit.EnterBrowserName", "MessageBox.Error");
            return;
        }

        if (string.IsNullOrWhiteSpace(Browser.ExecutablePath))
        {
            _ = LocalizedMessageBox.ShowError("Dialog.BrowserEdit.SelectExecutableFile", "MessageBox.Error");
            return;
        }

        if (!File.Exists(Browser.ExecutablePath))
        {
            _ = LocalizedMessageBox.ShowError("Dialog.BrowserEdit.ExecutableFileNotFound", "MessageBox.Error");
            return;
        }

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
