// <copyright file="WindowSizeHelperTests.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Presentation.Helpers;
using FluentAssertions;

namespace BrowserSelector.UnitTests;

/// <summary>
/// <see cref="WindowSizeHelper"/> のクランプ・フォールバック挙動を検証する.
/// <c>SizeToContent</c>撤去に伴い、設定値が唯一の正となったためこのロジックが
/// ウィンドウサイズの実効値を決める重要な役割を持つ.
/// </summary>
public class WindowSizeHelperTests
{
    [Theory]
    [InlineData(800, 600, 800, 600)]
    [InlineData(100, 100, WindowSizeHelper.MinWindowWidth, WindowSizeHelper.MinWindowHeight)]
    [InlineData(5000, 5000, WindowSizeHelper.MaxWindowWidth, WindowSizeHelper.MaxWindowHeight)]
    [InlineData(0, 0, WindowSizeHelper.MinWindowWidth, WindowSizeHelper.MinWindowHeight)]
    [InlineData(-10, -10, WindowSizeHelper.MinWindowWidth, WindowSizeHelper.MinWindowHeight)]
    public void ResolveSize_ClampsToValidRange(double width, double height, double expectedWidth, double expectedHeight)
    {
        (double actualWidth, double actualHeight) = WindowSizeHelper.ResolveSize(width, height);

        actualWidth.Should().Be(expectedWidth);
        actualHeight.Should().Be(expectedHeight);
    }

    [Fact]
    public void ResolveSize_WithNaN_FallsBackToMinimum()
    {
        (double width, double height) = WindowSizeHelper.ResolveSize(double.NaN, double.NaN);

        width.Should().Be(WindowSizeHelper.MinWindowWidth);
        height.Should().Be(WindowSizeHelper.MinWindowHeight);
    }

    [Fact]
    public void ResolveSize_WithInfinity_FallsBackToMinimum()
    {
        (double width, double height) = WindowSizeHelper.ResolveSize(double.PositiveInfinity, double.PositiveInfinity);

        width.Should().Be(WindowSizeHelper.MinWindowWidth);
        height.Should().Be(WindowSizeHelper.MinWindowHeight);
    }

    [Fact]
    public void ApplyConfiguredSize_NullWindow_Throws()
    {
        Action act = () => WindowSizeHelper.ApplyConfiguredSize(null!, 800, 600);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(800, 600, 800, 600, false)]
    [InlineData(800.2, 600.2, 800, 600, false)]
    [InlineData(800.6, 600, 800, 600, true)]
    [InlineData(900, 600, 800, 600, true)]
    [InlineData(800, 700, 800, 600, true)]
    public void NeedsResize_ComparesCurrentAgainstConfiguredSizeWithTolerance(
        double currentWidth, double currentHeight, double configuredWidth, double configuredHeight, bool expected)
    {
        bool actual = WindowSizeHelper.NeedsResize(currentWidth, currentHeight, configuredWidth, configuredHeight);

        actual.Should().Be(expected);
    }

    [Fact]
    public void NeedsResize_WhenConfiguredValueIsInvalid_ComparesAgainstResolvedFallback()
    {
        bool actual = WindowSizeHelper.NeedsResize(
            WindowSizeHelper.MinWindowWidth, WindowSizeHelper.MinWindowHeight, 0, 0);

        actual.Should().BeFalse();
    }
}
