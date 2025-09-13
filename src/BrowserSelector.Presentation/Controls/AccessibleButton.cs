using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BrowserSelector.Presentation.Controls;

/// <summary>
/// アクセシビリティ対応のボタンコントロール.
/// </summary>
public class AccessibleButton : Button
{
    /// <summary>
    /// FocusBorderColorProperty.
    /// </summary>
    public static readonly DependencyProperty FocusBorderColorProperty =
        DependencyProperty.Register(
            nameof(FocusBorderColor),
            typeof(Color),
            typeof(AccessibleButton),
            new PropertyMetadata(Colors.Blue));

    /// <summary>
    /// FocusBorderWidthProperty.
    /// </summary>
    public static readonly DependencyProperty FocusBorderWidthProperty =
        DependencyProperty.Register(
            nameof(FocusBorderWidth),
            typeof(double),
            typeof(AccessibleButton),
            new PropertyMetadata(2.0));

    /// <summary>
    /// FocusBorderThicknessProperty.
    /// </summary>
    public static readonly DependencyProperty FocusBorderThicknessProperty =
        DependencyProperty.Register(nameof(FocusBorderThickness), typeof(Thickness), typeof(AccessibleButton),
            new PropertyMetadata(new Thickness(2)));

    /// <summary>
    /// ShowFocusIndicatorProperty.
    /// </summary>
    public static readonly DependencyProperty ShowFocusIndicatorProperty =
        DependencyProperty.Register(nameof(ShowFocusIndicator), typeof(bool), typeof(AccessibleButton),
            new PropertyMetadata(true));

    /// <summary>
    /// HighContrastModeProperty.
    /// </summary>
    public static readonly DependencyProperty HighContrastModeProperty =
        DependencyProperty.Register(nameof(HighContrastMode), typeof(bool), typeof(AccessibleButton),
            new PropertyMetadata(false, OnHighContrastModeChanged));


    /// <summary>
    /// Initializes static members of the <see cref="AccessibleButton"/> class.
    /// AccessibleButton.
    /// </summary>
    static AccessibleButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AccessibleButton),
            new FrameworkPropertyMetadata(typeof(AccessibleButton)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AccessibleButton"/> class.
    /// </summary>
    public AccessibleButton()
    {
        // アクセシビリティプロパティを設定
        SetValue(AutomationProperties.NameProperty, "AccessibleButton");
        SetValue(AutomationProperties.HelpTextProperty, "アクセシブルなボタンです");
        SetValue(AutomationProperties.LabeledByProperty, this);

        // キーボードナビゲーションを有効にする
        Focusable = true;
        TabIndex = 0;
    }

    /// <summary>
    /// Gets or sets フォーカス時のボーダー色.
    /// </summary>
    public Color FocusBorderColor
    {
        get => (Color)GetValue(FocusBorderColorProperty);
        set => SetValue(FocusBorderColorProperty, value);
    }

    /// <summary>
    /// Gets or sets フォーカス時のボーダー幅.
    /// </summary>
    public double FocusBorderWidth
    {
        get => (double)GetValue(FocusBorderWidthProperty);
        set => SetValue(FocusBorderWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets フォーカス時のボーダー太さ.
    /// </summary>
    public Thickness FocusBorderThickness
    {
        get => (Thickness)GetValue(FocusBorderThicknessProperty);
        set => SetValue(FocusBorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether フォーカスインジケーターを表示するかどうか.
    /// </summary>
    public bool ShowFocusIndicator
    {
        get => (bool)GetValue(ShowFocusIndicatorProperty);
        set => SetValue(ShowFocusIndicatorProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether 高コントラストモード.
    /// </summary>
    public bool HighContrastMode
    {
        get => (bool)GetValue(HighContrastModeProperty);
        set => SetValue(HighContrastModeProperty, value);
    }

    /// <summary>
    /// アクセシビリティ情報を設定.
    /// </summary>
    /// <param name="name">name.</param>
    /// <param name="helpText">helpText.</param>
    /// <param name="description">description.</param>
    public void SetAccessibilityInfo(string name, string helpText, string description = "")
    {
        SetValue(AutomationProperties.NameProperty, name);
        SetValue(AutomationProperties.HelpTextProperty, helpText);

        if (!string.IsNullOrEmpty(description))
        {
            SetValue(AutomationProperties.ItemStatusProperty, description);
        }
    }

    /// <summary>
    /// キーボードショートカットを設定.
    /// </summary>
    /// <param name="shortcut">shortcut.</param>
    public void SetKeyboardShortcut(string shortcut)
    {
        if (!string.IsNullOrEmpty(shortcut))
        {
            SetValue(AutomationProperties.AcceleratorKeyProperty, shortcut);
        }
    }

    /// <summary>
    /// OnMouseLeftButtonDown.
    /// </summary>
    /// <param name="e">MouseButtonEventArgs.</param>
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        // フォーカスを設定
        _ = Focus();
    }

    /// <summary>
    /// OnHighContrastModeChanged.
    /// </summary>
    /// <param name="d">DependencyObject.</param>
    /// <param name="e">DependencyPropertyChangedEventArgs.</param>
    private static void OnHighContrastModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AccessibleButton button)
        {
            button.UpdateHighContrastMode();
        }
    }

    /// <summary>
    /// UpdateFocusIndicator.
    /// </summary>
    /// <param name="show">show.</param>
    private void UpdateFocusIndicator(bool show)
    {
        if (show)
        {
            // フォーカス時のスタイルを適用
            if (HighContrastMode)
            {
                // 高コントラストモードではより目立つ色を使用
                BorderBrush = new SolidColorBrush(Colors.White);
                BorderThickness = new Thickness(3);
                Background = new SolidColorBrush(Colors.Black);
                Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                BorderBrush = new SolidColorBrush(FocusBorderColor);
                BorderThickness = FocusBorderThickness;
            }
        }
        else
        {
            // 通常時のスタイルに戻す
            if (HighContrastMode)
            {
                BorderBrush = new SolidColorBrush(Colors.Black);
                BorderThickness = new Thickness(1);
                Background = new SolidColorBrush(Colors.White);
                Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                BorderBrush = null;
                BorderThickness = new Thickness(0);
            }
        }
    }

    /// <summary>
    /// UpdateHighContrastMode.
    /// </summary>
    private void UpdateHighContrastMode()
    {
        if (HighContrastMode)
        {
            // 高コントラストモードの設定
            // 高コントラスト用の色を設定
            if (IsFocused)
            {
                UpdateFocusIndicator(true);
            }
            else
            {
                UpdateFocusIndicator(false);
            }
        }
        else
        {
            // 通常モードの設定
            if (IsFocused)
            {
                UpdateFocusIndicator(true);
            }
            else
            {
                UpdateFocusIndicator(false);
            }
        }
    }
}
