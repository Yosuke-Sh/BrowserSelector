using BrowserSelector.Core.Enums;
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Presentation.Helpers;
using BrowserSelector.Presentation.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace BrowserSelector.Presentation.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
public partial class MainWindow : Window
{
    private readonly ILogService _logService;
    private readonly IThemeService? _themeService;
    private readonly ISettingsService? _settingsService;
    private readonly CountdownController _countdownController = new();
    private DispatcherTimer? _countdownTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    /// <param name="viewModel">viewModel.</param>
    /// <param name="logService">logService.</param>
    /// <param name="themeService">themeService（省略可。DWMバックドロップのダーク/ライト判定に使用）.</param>
    /// <param name="settingsService">settingsService（省略可。ガラス効果・アニメーション設定の取得に使用）.</param>
    public MainWindow(MainViewModel viewModel, ILogService logService, IThemeService? themeService = null, ISettingsService? settingsService = null)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        _logService = logService;
        _themeService = themeService;
        _settingsService = settingsService;
        InitializeComponent();

        // DataContextの設定を即座に実行
        DataContext = viewModel;

        // 初期サイズ設定を適用（InitializeComponentの後）
        ApplyInitialSizeSettings(viewModel);

        // 初期背景設定を適用
        ApplyInitialBackgroundSettings(viewModel);

        // カウントダウン自動起動の初期化（Phase D）
        InitializeCountdown();

        // ウィンドウをアクティブにする
        _ = Activate();
        _ = Focus();

        // ウィンドウを確実に表示（App.xaml.csでShow()が呼ばれるため削除）
        // this.Show();
        BringIntoView();
    }

    /// <summary>
    /// Gets カウントダウン制御を外部（トレイ常駐等）から操作するためのコントローラー（Phase D）.
    /// </summary>
    public CountdownController Countdown => _countdownController;

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

        // Phase C-1: DWMバックドロップ（Mica/Acrylic）をHWND確定後に適用
        ApplyWindowBackdrop();

        // マルチモニター対応: URLリンクをクリックしたモニター（＝現在カーソルがあるモニター）と
        // 同一モニターに表示する。DPI取得にHWNDが必要なためSourceInitialized後に実行する。
        MonitorHelper.CenterOnCursorMonitor(this);
    }

    private static UniformGrid? FindVisualChildUniformGrid(DependencyObject parent)
    {
        int childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is UniformGrid uniformGrid)
            {
                return uniformGrid;
            }

            UniformGrid? found = FindVisualChildUniformGrid(child);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static Button? FindVisualChildButton(DependencyObject parent)
    {
        int childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is Button button)
            {
                return button;
            }

            Button? found = FindVisualChildButton(child);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    /// <summary>
    /// DWMバックドロップを適用する（Phase C-1）.
    /// ウィンドウ枠のダークモード追従（C-1）とトークンの中身のテーマ（C-0の<see cref="IThemeService"/>）を必ず一致させる。
    /// </summary>
    private void ApplyWindowBackdrop()
    {
        try
        {
            bool isDarkMode = _themeService?.IsDarkThemeActive ?? false;
            bool glassEffectEnabled = true;
            BackdropMode backdropMode = BackdropMode.Mica;
            double cornerRadiusPreference = 1;
            if (_settingsService != null)
            {
                AppSettings appSettings = _settingsService.LoadAppSettingsAsync().GetAwaiter().GetResult();
                glassEffectEnabled = appSettings.EnableGlassEffect;
                backdropMode = appSettings.BackdropMode;
                cornerRadiusPreference = appSettings.WindowCornerRadius;

                // Phase E-1: 外観タブの設定を反映（不透明度・常に最前面・タイトルバー表示切替）
                Opacity = Math.Clamp(appSettings.WindowOpacity, 0.3, 1.0);
                Topmost = appSettings.AlwaysOnTop;
                if (FindName("CustomTitleBarRow") is RowDefinition titleBarRow)
                {
                    titleBarRow.Height = appSettings.ShowTitleBar ? new GridLength(36) : new GridLength(0);
                }

                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.ShowTitleBar = appSettings.ShowTitleBar;
                }
            }

            // 半透明単色/不透明はDWMバックドロップを使わずフォールバック描画に直接倒す
            bool glassRequested = glassEffectEnabled && backdropMode != BackdropMode.SolidTranslucent && backdropMode != BackdropMode.Opaque;
            WindowBackdropHelper.BackdropKind kind = backdropMode switch
            {
                BackdropMode.Acrylic => WindowBackdropHelper.BackdropKind.Acrylic,
                BackdropMode.MicaAlt => WindowBackdropHelper.BackdropKind.MicaAlt,
                _ => WindowBackdropHelper.BackdropKind.Mica,
            };

            bool applied = WindowBackdropHelper.Apply(this, kind, isDarkMode, glassRequested, cornerRadiusPreference);
            _logService?.LogDebug($"DWMバックドロップ適用: Applied={applied}, IsDarkMode={isDarkMode}, GlassEffectEnabled={glassEffectEnabled}, BackdropMode={backdropMode}", "MainWindow");
        }
        // CA1031: ウィンドウ初期化の最上位try-catch。DWM呼び出し・設定読み込みなど例外種別が多岐にわたり、
        // 失敗してもウィンドウ表示自体は継続させるための意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"DWMバックドロップ適用エラー: {ex.Message}", "MainWindow", ex);
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// 初期背景設定を適用.
    /// </summary>
    private void ApplyInitialBackgroundSettings(MainViewModel viewModel)
    {
        try
        {
            Core.Models.VisualSettings visualSettings = viewModel.VisualSettings;
            if (visualSettings == null)
            {
                _logService?.LogWarning("VisualSettingsがnullのため、デフォルト背景を使用", "MainWindow");
                return;
            }

            _logService?.LogDebug($"初期背景設定適用開始: UseGradient={visualSettings.UseBackgroundGradient}", "MainWindow");

            // Phase C-2: Window.Backgroundはガラス効果のためTransparentのまま維持し、
            // フォールバック背景はXAML側のBackgroundBrushConverterバインドに一本化する。
            // （旧実装はここでWindow.Backgroundへ直接色を設定しており、C-1のDWMバックドロップと競合していた）
        }
        // CA1031: ウィンドウ初期化/イベントハンドラーの最上位try-catch。UI操作由来の例外種別が多岐にわたり、フォールバック値を設定してUIスレッドを継続させるための意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"初期背景設定適用エラー: {ex.Message}", "MainWindow", ex);
        }
        #pragma warning restore CA1031
    }

    /// <summary>
    /// 初期サイズ設定を適用する。
    /// <c>SizeToContent</c>は使用せず、<see cref="Core.Models.VisualSettings.InitialWindowWidth"/>/
    /// <see cref="Core.Models.VisualSettings.InitialWindowHeight"/>を唯一の正として常に適用する
    /// （設定値が反映されない・トレイ自動起動時に横幅が異常になる不具合の対策）.
    /// </summary>
    private void ApplyInitialSizeSettings(MainViewModel viewModel)
    {
        try
        {
            Core.Models.VisualSettings? visualSettings = viewModel.VisualSettings;
            if (visualSettings == null)
            {
                return;
            }

            _logService?.LogDebug($"初期サイズ設定適用: InitialWindowWidth={visualSettings.InitialWindowWidth}, InitialWindowHeight={visualSettings.InitialWindowHeight}", "MainWindow");
            WindowSizeHelper.ApplyConfiguredSize(this, visualSettings.InitialWindowWidth, visualSettings.InitialWindowHeight);
        }
        // CA1031: ウィンドウ初期化/イベントハンドラーの最上位try-catch。UI操作由来の例外種別が多岐にわたり、フォールバック値を設定してUIスレッドを継続させるための意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"初期サイズ設定適用エラー: {ex.Message}", "MainWindow", ex);
        }
        #pragma warning restore CA1031
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
    /// ハンバーガーメニューボタンのクリックハンドラー（Phase E-3）.
    /// タイトルバー非表示時の代替導線として、左クリックで<see cref="Button.ContextMenu"/>を開く.
    /// </summary>
    private void HamburgerMenuButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { ContextMenu: not null } button)
        {
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.IsOpen = true;
        }
    }

    /// <summary>
    /// 最小化ボタンのクリックハンドラー（Phase C-2）.
    /// </summary>
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    /// <summary>
    /// 閉じるボタンのクリックハンドラー（Phase C-2）.
    /// <see cref="AppSettings.AlwaysResidentInTray"/>が有効な場合、実際の終了はApp側（<see cref="Window.Closing"/>）で
    /// トレイ格納に差し替えられる（Phase D）。
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// マウス移動を検知してカウントダウンを一時停止する（Phase D）.
    /// </summary>
    private void MainWindow_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        PauseCountdownOnActivity();
    }

    /// <summary>
    /// カウントダウン自動起動を初期化する（Phase D）。
    /// <see cref="AppSettings.DefaultDelay"/>秒後に既定ブラウザへ自動起動する。
    /// マウス移動・キー入力があれば一時停止する.
    /// </summary>
    private void InitializeCountdown()
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        _countdownController.TickOccurred += (_, remaining) =>
        {
            viewModel.CountdownRemainingSeconds = remaining;
            viewModel.IsCountdownActive = remaining > 0;
        };
        _countdownController.Elapsed += async (_, _) =>
        {
            viewModel.IsCountdownActive = false;
            await viewModel.LaunchDefaultBrowserAsync().ConfigureAwait(true);
        };

        // Phase H-10補完: 更新通知バーはIsCountdownActive==falseの間しか見えないため、
        // カウントダウンと同時に検出された更新をユーザーが確認する前にカウントダウンが
        // 既定ブラウザを自動起動してウィンドウが閉じられ、通知が見送られ続ける不具合があった。
        // 通知表示のタイミングでカウントダウンを一時停止し、通知バーを確実に表示・操作可能にする.
        viewModel.UpdateNotificationShown += (_, _) =>
        {
            _countdownController.Pause();
            viewModel.IsCountdownActive = false;
        };

        _countdownTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1),
        };
        _countdownTimer.Tick += (_, _) => _countdownController.Tick();
        _countdownTimer.Start();

        try
        {
            int delaySeconds = _settingsService?.LoadAppSettingsAsync().GetAwaiter().GetResult().DefaultDelay ?? 0;
            _countdownController.Start(delaySeconds);
            viewModel.IsCountdownActive = delaySeconds > 0;
            viewModel.CountdownRemainingSeconds = delaySeconds;
        }
        // CA1031: ウィンドウ初期化の最上位try-catch。設定読み込み失敗時はカウントダウンを開始しないだけで、
        // ウィンドウ表示自体は継続させるための意図的な汎用catch。
#pragma warning disable CA1031
        catch (Exception ex)
        {
            _logService?.LogError($"カウントダウン初期化エラー: {ex.Message}", "MainWindow", ex);
        }
#pragma warning restore CA1031
    }

    /// <summary>
    /// マウス移動・キー入力を検知してカウントダウンを一時停止する（Phase D）.
    /// </summary>
    private void PauseCountdownOnActivity()
    {
        if (_countdownController.IsRunning)
        {
            _countdownController.Pause();
        }
    }

    /// <summary>
    /// ブラウザタイルのCtrl+クリック検知（Phase C-4）。
    /// Ctrl+クリックの場合、その起動に限り「起動後に閉じる」を抑制する。
    /// </summary>
    private void BrowserButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && DataContext is MainViewModel viewModel)
        {
            viewModel.SuppressAutoCloseOnce();
        }
    }

    /// <summary>
    /// キーボード操作（Phase C-4）: Esc（閉じる）、Enter/Space（起動）、矢印キー（グリッド移動、端で回り込み）、
    /// 1-9/A-Z（ホットキー起動）を処理する。キー入力はカウントダウンを一時停止させる（Phase D）.
    /// </summary>
    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        PauseCountdownOnActivity();

        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Escape:
                Close();
                e.Handled = true;
                return;

            case Key.Left:
            case Key.Right:
            case Key.Up:
            case Key.Down:
                MoveTileFocus(e.Key);
                e.Handled = true;
                return;

            case Key.Enter:
            case Key.Space:
                if (Keyboard.FocusedElement is Button focusedButton && focusedButton.DataContext is Browser focusedBrowser)
                {
                    LaunchBrowserFromKeyboard(viewModel, focusedBrowser);
                    e.Handled = true;
                }

                return;
        }

        // ホットキー（1-9 / A-Z）。数字キーの "D5" 問題を避けるため専用ヘルパーで正規化する。
        char? hotkey = HotkeyResolver.Resolve(e.Key, Keyboard.Modifiers);
        if (hotkey.HasValue)
        {
            int index = HotkeyResolver.BadgeSequence.ToList().IndexOf(hotkey.Value);
            if (index >= 0 && index < viewModel.Browsers.Count)
            {
                LaunchBrowserFromKeyboard(viewModel, viewModel.Browsers[index]);
                e.Handled = true;
            }
        }
    }

    private void LaunchBrowserFromKeyboard(MainViewModel viewModel, Browser browser)
    {
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            viewModel.SuppressAutoCloseOnce();
        }

        if (viewModel.LaunchBrowserCommand.CanExecute(browser))
        {
            viewModel.LaunchBrowserCommand.Execute(browser);
        }
    }

    /// <summary>
    /// 矢印キーによるタイルグリッド移動。列数計算は <see cref="TileLayoutHelper"/> を
    /// UniformGridのレイアウトと共有し、表示上の列数とキーボード移動の列数を一致させる（Phase C-3/C-4）.
    /// </summary>
    private void MoveTileFocus(Key key)
    {
        if (DataContext is not MainViewModel viewModel || viewModel.Browsers.Count == 0)
        {
            return;
        }

        int columns = ResolveActualColumnCount();
        int currentIndex = ResolveFocusedBrowserIndex(viewModel);
        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        TileNavigationDirection direction = key switch
        {
            Key.Right => TileNavigationDirection.Right,
            Key.Left => TileNavigationDirection.Left,
            Key.Down => TileNavigationDirection.Down,
            Key.Up => TileNavigationDirection.Up,
            _ => TileNavigationDirection.Right,
        };

        int newIndex = TileLayoutHelper.MoveIndex(currentIndex, viewModel.Browsers.Count, columns, direction);
        FocusTileAtIndex(newIndex);
    }

    /// <summary>
    /// ブラウザ一覧のスクロール領域がリサイズされた際、UniformGridの列数を再計算する（Phase C-3）。
    /// これが列数計算の唯一の適用箇所であり、矢印キー移動（<see cref="ResolveActualColumnCount"/>）はここで
    /// 設定された<see cref="UniformGrid.Columns"/>を読み取って一致させる。
    /// </summary>
    private void BrowserButtonsScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateBrowserGridColumns();
    }

    private void UpdateBrowserGridColumns()
    {
        if (FindVisualChildUniformGrid(BrowserItemsControl) is not UniformGrid uniformGrid)
        {
            return;
        }

        double availableWidth = ((FrameworkElement)BrowserItemsControl.Parent).ActualWidth;
        int columns = TileLayoutHelper.CalculateColumns(availableWidth, TileLayoutHelper.DefaultTileWidth, BrowserItemsControl.Items.Count);
        uniformGrid.Columns = columns;
    }

    private int ResolveActualColumnCount()
    {
        if (FindVisualChildUniformGrid(BrowserItemsControl) is UniformGrid uniformGrid && uniformGrid.Columns > 0)
        {
            return uniformGrid.Columns;
        }

        double availableWidth = BrowserItemsControl.ActualWidth > 0 ? BrowserItemsControl.ActualWidth : ActualWidth;
        return TileLayoutHelper.CalculateColumns(availableWidth, TileLayoutHelper.DefaultTileWidth, BrowserItemsControl.Items.Count);
    }

    private int ResolveFocusedBrowserIndex(MainViewModel viewModel)
    {
        if (Keyboard.FocusedElement is Button focusedButton && focusedButton.DataContext is Browser focusedBrowser)
        {
            return viewModel.Browsers.IndexOf(focusedBrowser);
        }

        return -1;
    }

    private void FocusTileAtIndex(int index)
    {
        if (index < 0 || index >= BrowserItemsControl.Items.Count)
        {
            return;
        }

        if (BrowserItemsControl.ItemContainerGenerator.ContainerFromIndex(index) is ContentPresenter presenter)
        {
            presenter.ApplyTemplate();
            Button? button = FindVisualChildButton(presenter);
            _ = (button?.Focus());
        }
    }

}
