using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using BrowserSelector.Core.Models;

namespace BrowserSelector.Presentation.Views;

/// <summary>
/// BrowserEditDialog.xaml の相互作用ロジック
/// </summary>
public partial class BrowserEditDialog : Window
{
    public Browser Browser { get; private set; }
    private readonly bool _isNewBrowser;
    private readonly bool _isSystemBrowser;

    public BrowserEditDialog(Browser? browser = null, bool isSystemBrowser = false)
    {
        InitializeComponent();
        
        if (browser == null)
        {
            // 新規作成
            _isNewBrowser = true;
            _isSystemBrowser = false;
            Browser = new Browser
            {
                Id = Guid.NewGuid(),
                Name = "新しいブラウザ",
                ExecutablePath = "",
                IconPath = "",
                Arguments = "",
                Type = BrowserType.Custom,
                IsEnabled = true,
                DisplayOrder = 0
            };
        }
        else
        {
            // 編集
            _isNewBrowser = false;
            _isSystemBrowser = isSystemBrowser;
            Browser = browser.Clone(); // 複製して編集
        }

        DataContext = Browser;
        Title = _isNewBrowser ? "ブラウザ追加" : (_isSystemBrowser ? "システムブラウザ設定" : "ブラウザ編集");
        
        // システムブラウザの場合は、編集不可能な項目を無効化
        if (_isSystemBrowser)
        {
            // 名前と実行ファイルパスは編集不可
            NameTextBox.IsEnabled = false;
            ExecutablePathTextBox.IsEnabled = false;
            BrowseExecutableButton.IsEnabled = false;
            
            // 説明を追加
            SystemBrowserInfoText.Visibility = Visibility.Visible;
        }
    }

    private void BrowseExecutable_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
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
                System.Diagnostics.Debug.WriteLine($"アイコンを実行ファイルから自動設定: {openFileDialog.FileName}");
            }
            
            // 名前がデフォルトの場合、ファイル名から自動設定
            if (Browser.Name == "新しいブラウザ" || string.IsNullOrEmpty(Browser.Name))
            {
                Browser.Name = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                System.Diagnostics.Debug.WriteLine($"ブラウザ名を実行ファイルから自動設定: {Browser.Name}");
            }
        }
    }

    private void BrowseIcon_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
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
        var dialog = new IconSelectionDialog();
        
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
            System.Diagnostics.Debug.WriteLine($"アイコンが選択されました: {dialog.SelectedIconPath}, インデックス: {dialog.SelectedIconIndex}");
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
            MessageBox.Show("ブラウザ名を入力してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(Browser.ExecutablePath))
        {
            MessageBox.Show("実行ファイルを選択してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (!File.Exists(Browser.ExecutablePath))
        {
            MessageBox.Show("選択された実行ファイルが存在しません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
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