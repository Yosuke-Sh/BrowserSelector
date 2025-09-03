using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using BrowserSelector.Core.Enums;

namespace BrowserSelector.Core.Models;

/// <summary>
/// 視覚的な設定を表すモデル
/// </summary>
public partial class VisualSettings : ObservableObject
{
    [ObservableProperty]
    private Color _backgroundColor = Colors.White;

    [ObservableProperty]
    private bool _useBackgroundGradient = false;

    [ObservableProperty]
    private Color _gradientStartColor = Colors.Transparent;

    [ObservableProperty]
    private Color _gradientEndColor = Colors.Transparent;

    [ObservableProperty]
    private GradientDirection _gradientDirection = GradientDirection.Vertical;

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
