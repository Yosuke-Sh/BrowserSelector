// <copyright file="MonitorHelperTests.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Presentation.Helpers;
using FluentAssertions;

namespace BrowserSelector.UnitTests;

/// <summary>
/// <see cref="MonitorHelper.CalculateCenteredPosition"/> の座標計算を検証する。
/// P/Invokeに依存する実際のモニター取得はここでは検証せず、作業領域座標が
/// 与えられた前提での純粋な算術のみを対象とする.
/// </summary>
public class MonitorHelperTests
{
    [Fact]
    public void CalculateCenteredPosition_PrimaryMonitorAtOrigin_CentersWindow()
    {
        (double left, double top) = MonitorHelper.CalculateCenteredPosition(
            workLeft: 0, workTop: 0, workWidth: 1920, workHeight: 1080,
            windowWidth: 800, windowHeight: 600);

        left.Should().Be(560); // (1920-800)/2
        top.Should().Be(240); // (1080-600)/2
    }

    [Fact]
    public void CalculateCenteredPosition_SecondaryMonitorAtNegativeCoordinates_CentersWithinThatMonitor()
    {
        // プライマリの左側に配置されたセカンダリモニター（負座標）を想定。
        (double left, double top) = MonitorHelper.CalculateCenteredPosition(
            workLeft: -1920, workTop: 0, workWidth: 1920, workHeight: 1080,
            windowWidth: 800, windowHeight: 600);

        left.Should().Be(-1360); // -1920 + (1920-800)/2
        top.Should().Be(240);
    }

    [Fact]
    public void CalculateCenteredPosition_WindowLargerThanWorkArea_ClampsToWorkAreaOrigin()
    {
        (double left, double top) = MonitorHelper.CalculateCenteredPosition(
            workLeft: 0, workTop: 0, workWidth: 1024, workHeight: 768,
            windowWidth: 2000, windowHeight: 1500);

        left.Should().Be(0);
        top.Should().Be(0);
    }

    [Fact]
    public void CalculateCenteredPosition_ResultNeverExceedsWorkAreaBounds()
    {
        (double left, double top) = MonitorHelper.CalculateCenteredPosition(
            workLeft: 100, workTop: 50, workWidth: 1280, workHeight: 720,
            windowWidth: 400, windowHeight: 300);

        left.Should().BeGreaterThanOrEqualTo(100);
        top.Should().BeGreaterThanOrEqualTo(50);
        (left + 400).Should().BeLessThanOrEqualTo(100 + 1280);
        (top + 300).Should().BeLessThanOrEqualTo(50 + 720);
    }
}
