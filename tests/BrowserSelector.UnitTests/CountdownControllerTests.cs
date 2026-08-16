// <copyright file="CountdownControllerTests.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Presentation.Helpers;
using Xunit;

namespace BrowserSelector.UnitTests;

/// <summary>
/// <see cref="CountdownController"/> のテスト（Phase D）.
/// </summary>
public class CountdownControllerTests
{
    [Fact]
    public void Start_WithPositiveDelay_SetsRunningAndRemainingSeconds()
    {
        CountdownController controller = new();

        controller.Start(5);

        Assert.True(controller.IsRunning);
        Assert.Equal(5, controller.RemainingSeconds);
    }

    [Fact]
    public void Start_WithZeroDelay_DoesNotRun()
    {
        CountdownController controller = new();

        controller.Start(0);

        Assert.False(controller.IsRunning);
        Assert.Equal(0, controller.RemainingSeconds);
    }

    [Fact]
    public void Start_WithNegativeDelay_DoesNotRun()
    {
        CountdownController controller = new();

        controller.Start(-1);

        Assert.False(controller.IsRunning);
    }

    [Fact]
    public void Tick_DecrementsRemainingSeconds()
    {
        CountdownController controller = new();
        controller.Start(3);

        controller.Tick();

        Assert.Equal(2, controller.RemainingSeconds);
        Assert.True(controller.IsRunning);
    }

    [Fact]
    public void Tick_ReachingZero_RaisesElapsedAndStops()
    {
        CountdownController controller = new();
        controller.Start(1);
        bool elapsedRaised = false;
        controller.Elapsed += (_, _) => elapsedRaised = true;

        controller.Tick();

        Assert.True(elapsedRaised);
        Assert.False(controller.IsRunning);
        Assert.Equal(0, controller.RemainingSeconds);
    }

    [Fact]
    public void Tick_WhenNotRunning_DoesNothing()
    {
        CountdownController controller = new();

        controller.Tick();

        Assert.Equal(0, controller.RemainingSeconds);
        Assert.False(controller.IsRunning);
    }

    [Fact]
    public void Pause_StopsRunningWithoutResettingRemainingSeconds()
    {
        CountdownController controller = new();
        controller.Start(5);

        controller.Pause();

        Assert.False(controller.IsRunning);
        Assert.Equal(5, controller.RemainingSeconds);
    }

    [Fact]
    public void Resume_AfterPause_ResumesRunning()
    {
        CountdownController controller = new();
        controller.Start(5);
        controller.Pause();

        controller.Resume();

        Assert.True(controller.IsRunning);
    }

    [Fact]
    public void Reset_StopsAndClearsRemainingSeconds()
    {
        CountdownController controller = new();
        controller.Start(5);

        controller.Reset();

        Assert.False(controller.IsRunning);
        Assert.Equal(0, controller.RemainingSeconds);
    }

    [Fact]
    public void SuspendForTray_StopsCountdownAndPreventsTicks()
    {
        CountdownController controller = new();
        controller.Start(5);

        controller.SuspendForTray();
        controller.Tick();

        Assert.True(controller.IsSuspendedByTray);
        Assert.False(controller.IsRunning);
        Assert.Equal(5, controller.RemainingSeconds); // Tickが無視され進行していない
    }

    [Fact]
    public void SuspendForTray_PreventsResume()
    {
        CountdownController controller = new();
        controller.Start(5);
        controller.SuspendForTray();

        controller.Resume();

        Assert.False(controller.IsRunning);
    }

    [Fact]
    public void ResumeFromTray_ClearsSuspensionAndResetsCountdown()
    {
        CountdownController controller = new();
        controller.Start(5);
        controller.SuspendForTray();

        controller.ResumeFromTray();

        Assert.False(controller.IsSuspendedByTray);
        Assert.False(controller.IsRunning);
        Assert.Equal(0, controller.RemainingSeconds); // 既知バグ対策: 自動再開せずリセットのみ
    }

    [Fact]
    public void Tick_RaisesTickOccurredWithRemainingSeconds()
    {
        CountdownController controller = new();
        controller.Start(2);
        List<int> observedValues = [];
        controller.TickOccurred += (_, remaining) => observedValues.Add(remaining);

        controller.Tick();

        Assert.Contains(1, observedValues);
    }

    [Fact]
    public void MouseOrKeyboardActivity_PauseThenResume_DoesNotAutoLaunch()
    {
        // マウス移動・キー入力での一時停止 → 再開のシナリオ。一時停止中はTickが進行しない。
        CountdownController controller = new();
        controller.Start(2);

        controller.Pause();
        controller.Tick(); // 一時停止中なので無視される
        Assert.Equal(2, controller.RemainingSeconds);

        controller.Resume();
        controller.Tick();
        Assert.Equal(1, controller.RemainingSeconds);
    }
}
