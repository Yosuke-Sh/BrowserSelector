// <copyright file="VisualSettings.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Core.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;

namespace BrowserSelector.Core.Models;

/// <summary>
/// 視覚的な設定を表すモデル.
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

    [ObservableProperty]
    private double _initialWindowWidth = 800.0;

    [ObservableProperty]
    private double _initialWindowHeight = 600.0;

    [ObservableProperty]
    private bool _showLogo = true;

    [ObservableProperty]
    private bool _showUrlInput = true;

    // ブラウザボタン設定
    [ObservableProperty]
    private double _browserButtonWidth = 120.0;

    [ObservableProperty]
    private double _browserButtonHeight = 90.0;

    [ObservableProperty]
    private Color _browserButtonBackgroundColor = Colors.Transparent;

    [ObservableProperty]
    private Color _browserButtonForegroundColor = Colors.Black;

    [ObservableProperty]
    private double _browserButtonOpacity = 1.0;

    [ObservableProperty]
    private double _browserButtonCornerRadius = 8.0;

    [ObservableProperty]
    private bool _showBrowserName = true;

    [ObservableProperty]
    private double _browserIconSize = 32.0;
}
