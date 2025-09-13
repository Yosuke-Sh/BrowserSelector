// <copyright file="BackgroundBrushConverter.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Core.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BrowserSelector.Presentation.Converters;

/// <summary>
/// 背景ブラシ変換コンバーター.
/// グラデーション設定に応じて適切なブラシを返します.
/// </summary>
public class BackgroundBrushConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not VisualSettings visualSettings)
        {
            System.Diagnostics.Debug.WriteLine($"BackgroundBrushConverter: VisualSettingsがnullまたは無効です。デフォルトの白いブラシを返します。");
            return new SolidColorBrush(Colors.White);
        }

        System.Diagnostics.Debug.WriteLine($"BackgroundBrushConverter: UseGradient={visualSettings.UseBackgroundGradient}, BackgroundColor={visualSettings.BackgroundColor}, GradientStartColor={visualSettings.GradientStartColor}, GradientEndColor={visualSettings.GradientEndColor}");

        if (visualSettings.UseBackgroundGradient)
        {
            // グラデーション方向に応じてStartPointとEndPointを設定
            Point startPoint, endPoint;
            switch (visualSettings.GradientDirection)
            {
                case BrowserSelector.Core.Enums.GradientDirection.Horizontal:
                    startPoint = new Point(0, 0);
                    endPoint = new Point(1, 0);
                    break;
                case BrowserSelector.Core.Enums.GradientDirection.Diagonal:
                    startPoint = new Point(0, 0);
                    endPoint = new Point(1, 1);
                    break;
                default: // Vertical
                    startPoint = new Point(0, 0);
                    endPoint = new Point(0, 1);
                    break;
            }

            var gradientBrush = new LinearGradientBrush
            {
                StartPoint = startPoint,
                EndPoint = endPoint,
                GradientStops =
                [
                    new GradientStop(visualSettings.GradientStartColor, 0),
                    new GradientStop(visualSettings.GradientEndColor, 1)
                ]
            };

            System.Diagnostics.Debug.WriteLine($"BackgroundBrushConverter: LinearGradientBrushを作成しました。StartPoint={startPoint}, EndPoint={endPoint}");
            return gradientBrush;
        }
        else
        {
            var solidBrush = new SolidColorBrush(visualSettings.BackgroundColor);
            System.Diagnostics.Debug.WriteLine($"BackgroundBrushConverter: SolidColorBrushを作成しました。Color={visualSettings.BackgroundColor}");
            return solidBrush;
        }
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
