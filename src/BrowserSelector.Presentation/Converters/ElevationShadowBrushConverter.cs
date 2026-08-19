// <copyright file="ElevationShadowBrushConverter.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BrowserSelector.Presentation.Converters;

/// <summary>
/// ブラウザタイルの背景色から、立体表現（<see cref="Core.Enums.TileElevationStyle.Shadow"/>）の
/// 影色を生成するコンバーター。RGBを暗くし半透明化することで背景色に応じた影を表現する。
/// <see cref="Core.Models.VisualSettings.BrowserButtonBackgroundColor"/>の既定値は
/// <see cref="Colors.Transparent"/>のため、その場合は既存のグレー系デザイントークン
/// （Brush.ElevationShadow相当の#33000000）へフォールバックする（さもないと既定設定で
/// 影が見えなくなり、機能が壊れているように見えるため）.
/// </summary>
public class ElevationShadowBrushConverter : IValueConverter
{
    private const double DarkenFactor = 0.55;
    private const byte ShadowAlpha = 0x66;
    private static readonly Color FallbackShadowColor = Color.FromArgb(0x33, 0x00, 0x00, 0x00);

    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Color color || color == Colors.Transparent || color.A == 0)
        {
            return CreateFrozenBrush(FallbackShadowColor);
        }

        byte r = (byte)(color.R * DarkenFactor);
        byte g = (byte)(color.G * DarkenFactor);
        byte b = (byte)(color.B * DarkenFactor);
        return CreateFrozenBrush(Color.FromArgb(ShadowAlpha, r, g, b));
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static SolidColorBrush CreateFrozenBrush(Color color)
    {
        SolidColorBrush brush = new(color);
        brush.Freeze();
        return brush;
    }
}
