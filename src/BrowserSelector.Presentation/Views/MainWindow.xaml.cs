using System.Windows;
using BrowserSelector.Presentation.ViewModels;

namespace BrowserSelector.Presentation.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        // 視覚設定を適用
        ApplyVisualSettings();
        
        // ウィンドウをアクティブにする
        this.Activate();
        this.Focus();
        
        // ウィンドウの位置とサイズを明示的に設定
        this.WindowState = WindowState.Normal;
        this.Left = 100;
        this.Top = 100;
        this.Width = 800;
        this.Height = 600;
        
        // デバッグ情報を出力
        System.Diagnostics.Debug.WriteLine($"ウィンドウ位置: Left={this.Left}, Top={this.Top}, Width={this.Width}, Height={this.Height}");
        System.Diagnostics.Debug.WriteLine($"ウィンドウ状態: WindowState={this.WindowState}, Visibility={this.Visibility}");
        
        // ウィンドウを確実に表示
        this.Show();
        this.BringIntoView();
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
            // ApplyVisualSettings(visualSettings); // 一時的にコメントアウト
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"視覚設定適用エラー: {ex.Message}");
        }
    }
}
