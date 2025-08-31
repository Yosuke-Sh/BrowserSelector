using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Automation;

namespace BrowserSelector.Presentation.Controls;

/// <summary>
/// アクセシビリティ対応のボタンコントロール
/// </summary>
public class AccessibleButton : Button
{
    public static readonly DependencyProperty FocusBorderColorProperty =
        DependencyProperty.Register(nameof(FocusBorderColor), typeof(Color), typeof(AccessibleButton),
            new PropertyMetadata(Colors.Blue));

    public static readonly DependencyProperty FocusBorderWidthProperty =
        DependencyProperty.Register(nameof(FocusBorderWidth), typeof(double), typeof(AccessibleButton),
            new PropertyMetadata(2.0));

    public static readonly DependencyProperty FocusBorderThicknessProperty =
        DependencyProperty.Register(nameof(FocusBorderThickness), typeof(Thickness), typeof(AccessibleButton),
            new PropertyMetadata(new Thickness(2)));

    public static readonly DependencyProperty ShowFocusIndicatorProperty =
        DependencyProperty.Register(nameof(ShowFocusIndicator), typeof(bool), typeof(AccessibleButton),
            new PropertyMetadata(true));

    /// <summary>
    /// フォーカス時のボーダー色
    /// </summary>
    public Color FocusBorderColor
    {
        get => (Color)GetValue(FocusBorderColorProperty);
        set => SetValue(FocusBorderColorProperty, value);
    }

    /// <summary>
    /// フォーカス時のボーダー幅
    /// </summary>
    public double FocusBorderWidth
    {
        get => (double)GetValue(FocusBorderWidthProperty);
        set => SetValue(FocusBorderWidthProperty, value);
    }

    /// <summary>
    /// フォーカス時のボーダー太さ
    /// </summary>
    public Thickness FocusBorderThickness
    {
        get => (Thickness)GetValue(FocusBorderThicknessProperty);
        set => SetValue(FocusBorderThicknessProperty, value);
    }

    /// <summary>
    /// フォーカスインジケーターを表示するかどうか
    /// </summary>
    public bool ShowFocusIndicator
    {
        get => (bool)GetValue(ShowFocusIndicatorProperty);
        set => SetValue(ShowFocusIndicatorProperty, value);
    }

    static AccessibleButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AccessibleButton), 
            new FrameworkPropertyMetadata(typeof(AccessibleButton)));
    }

    public AccessibleButton()
    {
        // アクセシビリティプロパティを設定
        SetValue(AutomationProperties.NameProperty, "AccessibleButton");
        SetValue(AutomationProperties.HelpTextProperty, "アクセシブルなボタンです");
        SetValue(AutomationProperties.LabeledByProperty, this);

        // キーボードナビゲーションを有効にする
        Focusable = true;
        TabIndex = 0;

        // キーボードイベントを処理
        KeyDown += OnKeyDown;
        GotFocus += OnGotFocus;
        LostFocus += OnLostFocus;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        
        // フォーカスを設定
        Focus();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        // EnterキーまたはSpaceキーでクリック
        if (e.Key == Key.Enter || e.Key == Key.Space)
        {
            e.Handled = true;
            OnClick();
        }
    }

    private void OnGotFocus(object sender, RoutedEventArgs e)
    {
        if (ShowFocusIndicator)
        {
            // フォーカスインジケーターを表示
            UpdateFocusIndicator(true);
        }
    }

    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (ShowFocusIndicator)
        {
            // フォーカスインジケーターを非表示
            UpdateFocusIndicator(false);
        }
    }

    private void UpdateFocusIndicator(bool show)
    {
        if (show)
        {
            // フォーカス時のスタイルを適用
            BorderBrush = new SolidColorBrush(FocusBorderColor);
            BorderThickness = FocusBorderThickness;
        }
        else
        {
            // 通常時のスタイルに戻す
            BorderBrush = null;
            BorderThickness = new Thickness(0);
        }
    }

    /// <summary>
    /// アクセシビリティ情報を設定
    /// </summary>
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
    /// キーボードショートカットを設定
    /// </summary>
    public void SetKeyboardShortcut(string shortcut)
    {
        if (!string.IsNullOrEmpty(shortcut))
        {
            SetValue(AutomationProperties.AcceleratorKeyProperty, shortcut);
        }
    }
}
