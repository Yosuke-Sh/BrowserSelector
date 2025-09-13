using BrowserSelector.Core.Services;
using BrowserSelector.Presentation.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace BrowserSelector.Presentation.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
public partial class MainWindow : Window
{
    private readonly ILogService _logService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    /// <param name="viewModel">viewModel.</param>
    /// <param name="logService">logService.</param>
    public MainWindow(MainViewModel viewModel, ILogService logService)
    {
        _logService = logService;
        InitializeComponent();

        // DataContextの設定を即座に実行
        DataContext = viewModel;

        // 初期サイズ設定を適用（InitializeComponentの後）
        ApplyInitialSizeSettings(viewModel);

        // 初期背景設定を適用
        ApplyInitialBackgroundSettings(viewModel);

        // ウィンドウをアクティブにする
        _ = Activate();
        _ = Focus();

        // ウィンドウのLoadedイベントでサイズ設定を再適用
        Loaded += MainWindow_Loaded;

        // ウィンドウを確実に表示（App.xaml.csでShow()が呼ばれるため削除）
        // this.Show();
        BringIntoView();
    }

    /// <summary>
    /// DataContext変更時の処理.
    /// </summary>
    /// <param name="e">e.</param>
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // DataContext変更の監視を開始
        if (DataContext is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged += OnDataContextPropertyChanged;
            _logService?.LogDebug("MainWindow: DataContext変更監視を開始しました", "MainWindow");
        }
    }

    /// <summary>
    /// 初期背景設定を適用.
    /// </summary>
    private void ApplyInitialBackgroundSettings(MainViewModel viewModel)
    {
        try
        {
            Core.Models.VisualSettings visualSettings = viewModel.VisualSettings;
            if (visualSettings != null)
            {
                _logService?.LogDebug($"初期背景設定適用開始: UseGradient={visualSettings.UseBackgroundGradient}", "MainWindow");

                if (visualSettings.UseBackgroundGradient)
                {
                    // グラデーション方向に応じてStartPointとEndPointを設定
                    System.Windows.Point startPoint, endPoint;
                    switch (visualSettings.GradientDirection)
                    {
                        case BrowserSelector.Core.Enums.GradientDirection.Horizontal:
                            startPoint = new System.Windows.Point(0, 0);
                            endPoint = new System.Windows.Point(1, 0);
                            break;
                        case BrowserSelector.Core.Enums.GradientDirection.Diagonal:
                            startPoint = new System.Windows.Point(0, 0);
                            endPoint = new System.Windows.Point(1, 1);
                            break;
                        default: // Vertical
                            startPoint = new System.Windows.Point(0, 0);
                            endPoint = new System.Windows.Point(0, 1);
                            break;
                    }

                    Background = new System.Windows.Media.LinearGradientBrush
                    {
                        StartPoint = startPoint,
                        EndPoint = endPoint,
                        GradientStops =
                        [
                            new System.Windows.Media.GradientStop(visualSettings.GradientStartColor, 0),
                            new System.Windows.Media.GradientStop(visualSettings.GradientEndColor, 1)
                        ]
                    };
                    _logService?.LogDebug($"初期背景グラデーション設定完了: 方向={visualSettings.GradientDirection}", "MainWindow");
                }
                else
                {
                    Background = new System.Windows.Media.SolidColorBrush(visualSettings.BackgroundColor);
                    _logService?.LogDebug($"初期背景色設定完了: {visualSettings.BackgroundColor}", "MainWindow");
                }
            }
            else
            {
                _logService?.LogWarning("VisualSettingsがnullのため、デフォルト背景を使用", "MainWindow");
                Background = System.Windows.SystemColors.WindowBrush;
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"初期背景設定適用エラー: {ex.Message}", "MainWindow", ex);
            Background = System.Windows.SystemColors.WindowBrush;
        }
    }

    /// <summary>
    /// 初期サイズ設定を適用.
    /// </summary>
    private void ApplyInitialSizeSettings(MainViewModel viewModel)
    {
        try
        {
            Core.Models.VisualSettings visualSettings = viewModel.VisualSettings;
            if (visualSettings != null)
            {
                // 最小・最大サイズの制限
                double width = Math.Max(400, Math.Min(2000, visualSettings.InitialWindowWidth));
                double height = Math.Max(300, Math.Min(1500, visualSettings.InitialWindowHeight));

                // ログ出力
                _logService?.LogDebug($"初期サイズ設定適用: Width={width}, Height={height}", "MainWindow");
                _logService?.LogDebug($"VisualSettings: InitialWindowWidth={visualSettings.InitialWindowWidth}, InitialWindowHeight={visualSettings.InitialWindowHeight}", "MainWindow");

                // ウィンドウの位置を中央に設定
                WindowState = WindowState.Normal;
                Left = (SystemParameters.PrimaryScreenWidth - width) / 2;
                Top = (SystemParameters.PrimaryScreenHeight - height) / 2;

                // サイズを設定（位置設定の後）
                Width = width;
                Height = height;

                // 設定を強制適用
                UpdateLayout();

                _logService?.LogDebug($"ウィンドウサイズ設定完了: ActualWidth={ActualWidth}, ActualHeight={ActualHeight}", "MainWindow");
            }
            else
            {
                // デフォルトサイズ
                _logService?.LogWarning("VisualSettingsがnullのため、デフォルトサイズを使用", "MainWindow");
                WindowState = WindowState.Normal;
                Left = 100;
                Top = 100;
                Width = 800;
                Height = 600;
            }
        }
        catch (Exception ex)
        {
            // エラー時はデフォルトサイズを使用
            _logService?.LogError($"初期サイズ設定適用エラー: {ex.Message}", "MainWindow", ex);
            WindowState = WindowState.Normal;
            Left = 100;
            Top = 100;
            Width = 800;
            Height = 600;
        }
    }

    /// <summary>
    /// ウィンドウのLoadedイベントハンドラー.
    /// </summary>
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // ウィンドウが完全に読み込まれた後にサイズ設定を再適用
            if (DataContext is MainViewModel viewModel)
            {
                _logService?.LogDebug("MainWindow_Loaded: サイズ設定を再適用", "MainWindow");
                ApplyInitialSizeSettings(viewModel);
            }
        }
        catch (Exception ex)
        {
            _logService?.LogError($"MainWindow_Loaded エラー: {ex.Message}", "MainWindow", ex);
        }
    }

    /// <summary>
    /// DataContextのプロパティ変更時の処理.
    /// </summary>
    private void OnDataContextPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _logService?.LogDebug($"MainWindow: DataContextプロパティ変更検知: {e.PropertyName}", "MainWindow");

        if (e.PropertyName == nameof(MainViewModel.VisualSettings))
        {
            _logService?.LogDebug("MainWindow: VisualSettingsプロパティ変更を検知しました。UI更新を通知します。", "MainWindow");
            // UI更新を強制（UIスレッドで実行）
            Dispatcher.Invoke(() => InvalidateVisual());
        }
    }

    /// <summary>
    /// ブラウザボタンのクリックイベントハンドラー（デバッグ用）.
    /// </summary>
    private void BrowserButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            // データコンテキストの確認
            if (DataContext is MainViewModel)
            {
                // ViewModelの状態確認
            }

            // ボタンのバインディング情報を確認
            _ = button.Command;
            _ = button.CommandParameter;
        }
    }
}
