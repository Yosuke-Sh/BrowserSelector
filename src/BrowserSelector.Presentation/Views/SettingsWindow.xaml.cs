using BrowserSelector.Core.Enums;
using BrowserSelector.Presentation.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace BrowserSelector.Presentation.Views;

/// <summary>
/// SettingsWindow.xaml の相互作用ロジック
/// </summary>
public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // ウィンドウの所有者を設定
        if (Application.Current.MainWindow != null)
        {
            Owner = Application.Current.MainWindow;
        }
    }

    /// <summary>
    /// グラデーション方向コンボボックスの選択変更イベントハンドラー
    /// </summary>
    private void GradientDirectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && DataContext is SettingsViewModel viewModel)
        {
            int selectedIndex = comboBox.SelectedIndex;
            GradientDirection newDirection = (GradientDirection)selectedIndex;

            // ViewModelのプロパティを更新
            viewModel.VisualSettings.GradientDirection = newDirection;

            // ログ出力
            // グラデーション方向の変更は通常の操作なので、ログレベルを下げる
            // System.Diagnostics.Debug.WriteLine($"グラデーション方向が変更されました: {newDirection} (インデックス: {selectedIndex})");
        }
    }

    /// <summary>
    /// 言語管理ボタンのクリックイベントハンドラー
    /// </summary>
    private void LanguageManagementButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is SettingsViewModel settingsViewModel)
            {
                // 言語管理ダイアログを表示
                LanguageManagementViewModel languageManagementViewModel = new(
                    settingsViewModel.CustomLanguageService,
                    settingsViewModel.LogService);

                LanguageManagementDialog dialog = new(languageManagementViewModel)
                {
                    Owner = this
                };

                _ = dialog.ShowDialog();

                // ダイアログを閉じた後、言語一覧を更新
                settingsViewModel.RefreshLanguages();
            }
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show($"言語管理ダイアログの表示に失敗しました: {ex.Message}", "エラー",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
