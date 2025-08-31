using System.Windows;
using BrowserSelector.Presentation.ViewModels;

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
}
