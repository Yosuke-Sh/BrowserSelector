using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BrowserSelector.Core.Models;

namespace BrowserSelector.Presentation.Controls;

/// <summary>
/// 透明化機能を持つカスタムウィンドウ
/// </summary>
public class TransparentWindow : Window
{
    public static readonly DependencyProperty TransparencyColorProperty =
        DependencyProperty.Register(nameof(TransparencyColor), typeof(Color), typeof(TransparentWindow),
            new PropertyMetadata(Colors.Black, OnTransparencyColorChanged));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(double), typeof(TransparentWindow),
            new PropertyMetadata(0.0, OnCornerRadiusChanged));

    public static readonly DependencyProperty BackgroundGradientProperty =
        DependencyProperty.Register(nameof(BackgroundGradient), typeof(LinearGradientBrush), typeof(TransparentWindow),
            new PropertyMetadata(null, OnBackgroundGradientChanged));

    public static readonly DependencyProperty ShowTitleBarProperty =
        DependencyProperty.Register(nameof(ShowTitleBar), typeof(bool), typeof(TransparentWindow),
            new PropertyMetadata(true, OnShowTitleBarChanged));

    /// <summary>
    /// 透明化色
    /// </summary>
    public Color TransparencyColor
    {
        get => (Color)GetValue(TransparencyColorProperty);
        set => SetValue(TransparencyColorProperty, value);
    }

    /// <summary>
    /// 角の丸み（半径）
    /// </summary>
    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// 背景グラデーション
    /// </summary>
    public LinearGradientBrush BackgroundGradient
    {
        get => (LinearGradientBrush)GetValue(BackgroundGradientProperty);
        set => SetValue(BackgroundGradientProperty, value);
    }

    /// <summary>
    /// タイトルバーを表示するかどうか
    /// </summary>
    public bool ShowTitleBar
    {
        get => (bool)GetValue(ShowTitleBarProperty);
        set => SetValue(ShowTitleBarProperty, value);
    }

    static TransparentWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TransparentWindow), 
            new FrameworkPropertyMetadata(typeof(TransparentWindow)));
    }

    public TransparentWindow()
    {
        // ウィンドウの基本設定
        AllowsTransparency = true;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.CanResize;
        
        // ドラッグ可能にする
        MouseLeftButtonDown += (s, e) => DragMove();
    }

    /// <summary>
    /// 視覚設定を適用
    /// </summary>
    public void ApplyVisualSettings(VisualSettings settings)
    {
        if (settings == null) return;

        // 透明度を設定
        Opacity = Math.Max(0.01, Math.Min(1.0, settings.Opacity));

        // 透明化色を設定
        TransparencyColor = settings.TransparencyColor;

        // 角の丸みを設定
        CornerRadius = Math.Max(0, Math.Min(50, settings.CornerRadius));

        // 背景色を設定
        if (settings.UseCustomBackgroundColor)
        {
            Background = new SolidColorBrush(settings.BackgroundColor);
        }
        else
        {
            Background = null;
        }

        // 背景グラデーションを設定
        if (settings.UseBackgroundGradient)
        {
            BackgroundGradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(settings.GradientStartColor, 0),
                    new GradientStop(settings.GradientEndColor, 1)
                }
            };
        }

        // タイトルバーの表示/非表示
        ShowTitleBar = settings.ShowTitleBar;

        // ウィンドウの角を丸くする
        UpdateWindowShape();
    }

    private static void OnTransparencyColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TransparentWindow window)
        {
            window.UpdateTransparency();
        }
    }

    private static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TransparentWindow window)
        {
            window.UpdateWindowShape();
        }
    }

    private static void OnBackgroundGradientChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TransparentWindow window)
        {
            window.UpdateBackground();
        }
    }

    private static void OnShowTitleBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TransparentWindow window)
        {
            window.UpdateTitleBar();
        }
    }

    private void UpdateTransparency()
    {
        // 透明化処理の実装
        if (TransparencyColor != Colors.Transparent)
        {
            // 透明化色を適用
            var brush = new SolidColorBrush(TransparencyColor);
            brush.Opacity = 0.1; // 低い透明度で透明化効果
            Background = brush;
        }
    }

    private void UpdateWindowShape()
    {
        if (CornerRadius > 0)
        {
            // 角を丸くする処理
            var geometry = new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight), CornerRadius, CornerRadius);
            Clip = geometry;
        }
        else
        {
            Clip = null;
        }
    }

    private void UpdateBackground()
    {
        if (BackgroundGradient != null)
        {
            Background = BackgroundGradient;
        }
        else if (Background is SolidColorBrush solidBrush)
        {
            // 既存の背景色を保持
            Background = solidBrush;
        }
    }

    private void UpdateTitleBar()
    {
        // タイトルバーの表示/非表示を制御
        if (ShowTitleBar)
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
        }
        else
        {
            WindowStyle = WindowStyle.None;
        }
    }
}
