// <copyright file="ActiveWindowLocatorTests.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Presentation.Helpers;
using FluentAssertions;

namespace BrowserSelector.UnitTests;

/// <summary>
/// <see cref="ActiveWindowLocator"/> の振る舞いを検証する。
/// <see cref="System.Windows.Application.Current"/>が存在しないユニットテスト環境での
/// 安全なフォールバック（null返却、例外を投げない）を主眼とする.
/// </summary>
public class ActiveWindowLocatorTests
{
    [Fact]
    public void GetActiveWindow_WhenNoApplicationCurrent_ReturnsNullWithoutThrowing()
    {
        // ユニットテスト環境ではApplication.Currentが存在しないため、
        // MessageBoxのOwnerに設定できるウィンドウが無いことを表すnullを返す必要がある。
        Action act = () => ActiveWindowLocator.GetActiveWindow();

        act.Should().NotThrow();
        ActiveWindowLocator.GetActiveWindow().Should().BeNull();
    }
}
