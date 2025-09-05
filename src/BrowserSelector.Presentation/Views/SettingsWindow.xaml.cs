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
            var selectedIndex = comboBox.SelectedIndex;
            var newDirection = (GradientDirection)selectedIndex;

            // ViewModelのプロパティを更新
            viewModel.VisualSettings.GradientDirection = newDirection;

            // ログ出力（デバッグ用）
            System.Diagnostics.Debug.WriteLine($"グラデーション方向が変更されました: {newDirection} (インデックス: {selectedIndex})");
        }
    }
}
