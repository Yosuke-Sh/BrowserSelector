using System.Windows;
using BrowserSelector.Presentation.Controls;
using BrowserSelector.Presentation.ViewModels;

namespace BrowserSelector.Presentation.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : TransparentWindow
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        // 視覚設定を適用
        ApplyVisualSettings();
    }
    
    /// <summary>
    /// 視覚設定を適用
    /// </summary>
    private void ApplyVisualSettings()
    {
        try
        {
            // TODO: 設定サービスから視覚設定を読み込んで適用
            // 現在はデフォルト設定を使用
            var visualSettings = new Core.Models.VisualSettings();
            ApplyVisualSettings(visualSettings);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"視覚設定適用エラー: {ex.Message}");
        }
    }
}