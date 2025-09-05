using BrowserSelector.Presentation.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

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

        // 初期サイズ設定を適用（InitializeComponentの後）
        ApplyInitialSizeSettings(viewModel);

        // ウィンドウをアクティブにする
        this.Activate();
        this.Focus();

        // ウィンドウのLoadedイベントでサイズ設定を再適用
        this.Loaded += MainWindow_Loaded;

        // ウィンドウを確実に表示（App.xaml.csでShow()が呼ばれるため削除）
        // this.Show();
        this.BringIntoView();
    }

    /// <summary>
    /// 初期サイズ設定を適用
    /// </summary>
    private void ApplyInitialSizeSettings(MainViewModel viewModel)
    {
        try
        {
            var visualSettings = viewModel.VisualSettings;
            if (visualSettings != null)
            {
                // 最小・最大サイズの制限
                var width = Math.Max(400, Math.Min(2000, visualSettings.InitialWindowWidth));
                var height = Math.Max(300, Math.Min(1500, visualSettings.InitialWindowHeight));

                // デバッグ情報を出力
                System.Diagnostics.Debug.WriteLine($"初期サイズ設定適用: Width={width}, Height={height}");
                System.Diagnostics.Debug.WriteLine($"VisualSettings: InitialWindowWidth={visualSettings.InitialWindowWidth}, InitialWindowHeight={visualSettings.InitialWindowHeight}");

                // ウィンドウの位置を中央に設定
                this.WindowState = WindowState.Normal;
                this.Left = (SystemParameters.PrimaryScreenWidth - width) / 2;
                this.Top = (SystemParameters.PrimaryScreenHeight - height) / 2;

                // サイズを設定（位置設定の後）
                this.Width = width;
                this.Height = height;

                // 設定を強制適用
                this.UpdateLayout();

                System.Diagnostics.Debug.WriteLine($"ウィンドウサイズ設定完了: ActualWidth={this.ActualWidth}, ActualHeight={this.ActualHeight}");
            }
            else
            {
                // デフォルトサイズ
                System.Diagnostics.Debug.WriteLine("VisualSettingsがnullのため、デフォルトサイズを使用");
                this.WindowState = WindowState.Normal;
                this.Left = 100;
                this.Top = 100;
                this.Width = 800;
                this.Height = 600;
            }
        }
        catch (Exception ex)
        {
            // エラー時はデフォルトサイズを使用
            System.Diagnostics.Debug.WriteLine($"初期サイズ設定適用エラー: {ex.Message}");
            this.WindowState = WindowState.Normal;
            this.Left = 100;
            this.Top = 100;
            this.Width = 800;
            this.Height = 600;
        }
    }

    /// <summary>
    /// ウィンドウのLoadedイベントハンドラー
    /// </summary>
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // ウィンドウが完全に読み込まれた後にサイズ設定を再適用
            if (DataContext is MainViewModel viewModel)
            {
                System.Diagnostics.Debug.WriteLine("MainWindow_Loaded: サイズ設定を再適用");
                ApplyInitialSizeSettings(viewModel);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MainWindow_Loaded エラー: {ex.Message}");
        }
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
