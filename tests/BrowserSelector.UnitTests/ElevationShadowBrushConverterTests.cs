// <copyright file="ElevationShadowBrushConverterTests.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using System.Globalization;
using System.Windows.Media;
using BrowserSelector.Presentation.Converters;
using FluentAssertions;

namespace BrowserSelector.UnitTests;

/// <summary>
/// <see cref="ElevationShadowBrushConverter"/> の色生成・フォールバック挙動を検証する.
/// </summary>
public class ElevationShadowBrushConverterTests
{
    [Fact]
    public void Convert_WithOpaqueColor_ReturnsDarkenedSemiTransparentBrush()
    {
        ElevationShadowBrushConverter converter = new();

        object result = converter.Convert(Colors.Blue, typeof(Brush), null!, CultureInfo.InvariantCulture);

        SolidColorBrush brush = result.Should().BeOfType<SolidColorBrush>().Subject;
        brush.Color.R.Should().Be(0);
        brush.Color.G.Should().Be(0);
        brush.Color.B.Should().BeLessThan(255); // 暗くなっている
        brush.Color.A.Should().Be(0x66);
    }

    [Fact]
    public void Convert_WithTransparentColor_FallsBackToGreyShadow()
    {
        // BrowserButtonBackgroundColorの既定値はColors.Transparentのため、
        // 透明時に影が消えて機能が壊れて見えないようフォールバックする。
        ElevationShadowBrushConverter converter = new();

        object result = converter.Convert(Colors.Transparent, typeof(Brush), null!, CultureInfo.InvariantCulture);

        SolidColorBrush brush = result.Should().BeOfType<SolidColorBrush>().Subject;
        brush.Color.Should().Be(Color.FromArgb(0x33, 0x00, 0x00, 0x00));
    }

    [Fact]
    public void Convert_WithNonColorValue_FallsBackToGreyShadowWithoutThrowing()
    {
        ElevationShadowBrushConverter converter = new();

        object result = converter.Convert("not a color", typeof(Brush), null!, CultureInfo.InvariantCulture);

        result.Should().BeOfType<SolidColorBrush>();
    }

    [Fact]
    public void Convert_ReturnsFrozenBrush()
    {
        ElevationShadowBrushConverter converter = new();

        object result = converter.Convert(Colors.Red, typeof(Brush), null!, CultureInfo.InvariantCulture);

        SolidColorBrush brush = result.Should().BeOfType<SolidColorBrush>().Subject;
        brush.IsFrozen.Should().BeTrue();
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        ElevationShadowBrushConverter converter = new();

        Action act = () => converter.ConvertBack(Colors.Red, typeof(Color), null!, CultureInfo.InvariantCulture);

        act.Should().Throw<NotImplementedException>();
    }
}
