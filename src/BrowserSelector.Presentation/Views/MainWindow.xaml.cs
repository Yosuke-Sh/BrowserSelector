using System.Windows;
using System.Windows.Controls;
using BrowserSelector.Presentation.ViewModels;
using BrowserSelector.Core.Models;
using System.ComponentModel;

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
        
        // ウィンドウをアクティブにする
        this.Activate();
        this.Focus();
        
        // ウィンドウの位置とサイズを明示的に設定
        this.WindowState = WindowState.Normal;
        this.Left = 100;
        this.Top = 100;
        this.Width = 800;
        this.Height = 600;
        
        // ウィンドウを確実に表示（App.xaml.csでShow()が呼ばれるため削除）
        // this.Show();
        this.BringIntoView();
    }



    /// <summary>
    /// DataContext変更時の処理
    /// </summary>
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        
        // DataContext変更の監視を開始
        if (DataContext is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged += OnDataContextPropertyChanged;
            System.Diagnostics.Debug.WriteLine("MainWindow: DataContext変更監視を開始しました");
        }
    }

    /// <summary>
    /// DataContextのプロパティ変更時の処理
    /// </summary>
    private void OnDataContextPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"MainWindow: DataContextプロパティ変更検知: {e.PropertyName}");
        
        if (e.PropertyName == nameof(MainViewModel.VisualSettings))
        {
            System.Diagnostics.Debug.WriteLine("MainWindow: VisualSettingsプロパティ変更を検知しました。UI更新を通知します。");
            // UI更新を強制
            this.InvalidateVisual();
        }
    }
    
    /// <summary>
    /// ブラウザボタンのクリックイベントハンドラー（デバッグ用）
    /// </summary>
    private void BrowserButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            // データコンテキストの確認
            if (DataContext is MainViewModel viewModel)
            {
                // ViewModelの状態確認
            }
            
            // ボタンのバインディング情報を確認
            var command = button.Command;
            var commandParameter = button.CommandParameter;
        }
    }
}
