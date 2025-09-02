using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BrowserSelector.Core.Models;

/// <summary>
/// 視覚的な設定を表すモデル
/// </summary>
public partial class VisualSettings : ObservableObject
{
    [ObservableProperty]
    private double _opacity = 1.0;

    [ObservableProperty]
    private Color _transparencyColor = Colors.Black;

    [ObservableProperty]
    private bool _useCustomTransparencyColor = false;

    [ObservableProperty]
    private double _cornerRadius = 0;

    [ObservableProperty]
    private bool _showTitleBar = true;

    [ObservableProperty]
    private Color _backgroundColor = Colors.Transparent;

    [ObservableProperty]
    private bool _useCustomBackgroundColor = false;

    [ObservableProperty]
    private bool _enableGradient = false;

    [ObservableProperty]
    private bool _useBackgroundGradient = false;

    [ObservableProperty]
    private Color _gradientStartColor = Colors.Transparent;

    [ObservableProperty]
    private Color _gradientEndColor = Colors.Transparent;

    [ObservableProperty]
    private double _iconScale = 1.0;

    [ObservableProperty]
    private bool _showFocusIndicator = true;

    [ObservableProperty]
    private Color _focusColor = Colors.Blue;

    [ObservableProperty]
    private double _focusThickness = 2.0;

    [ObservableProperty]
    private double _focusWidth = 100.0;
}
