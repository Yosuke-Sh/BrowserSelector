// <copyright file="ShellCloseServiceTests.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.App.SystemIntegration;
using FluentAssertions;

namespace BrowserSelector.AppTests;

/// <summary>
/// <see cref="ShellCloseService"/> のトレイ格納/完全終了の振り分けを検証する.
/// トレイ未接続（<see cref="ShellCloseService.AttachTrayIcon"/> 未呼び出し）の状態は
/// トレイ常駐設定が無効な場合に相当し、この場合は完全終了へフォールバックする必要がある.
/// </summary>
public class ShellCloseServiceTests
{
    [Fact]
    public void CanMinimizeToTray_WhenTrayIconNotAttached_ReturnsFalse()
    {
        // Arrange: トレイ常駐無効時はApp.SetupTrayIconがAttachTrayIconを呼び出さない。
        ShellCloseService sut = new();

        // Assert
        sut.CanMinimizeToTray.Should().BeFalse();
    }

    [Fact]
    public void RequestClose_WhenTrayIconNotAttached_ShutsDownWithoutThrowing()
    {
        WpfTestHelper.InitializeWpfContext();
        try
        {
            ShellCloseService sut = new();

            Action act = sut.RequestClose;

            act.Should().NotThrow();
        }
        finally
        {
            WpfTestHelper.CleanupWpfContext();
        }
    }
}
