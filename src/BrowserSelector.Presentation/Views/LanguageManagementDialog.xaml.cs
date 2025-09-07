using BrowserSelector.Presentation.ViewModels;
using System.Windows;

namespace BrowserSelector.Presentation.Views;

/// <summary>
/// 言語管理ダイアログ
/// </summary>
public partial class LanguageManagementDialog : Window
{
    public LanguageManagementDialog(LanguageManagementViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // 初期化
        Loaded += async (s, e) => await viewModel.InitializeAsync().ConfigureAwait(false);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
