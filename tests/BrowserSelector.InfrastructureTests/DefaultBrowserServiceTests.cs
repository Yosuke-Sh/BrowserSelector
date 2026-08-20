// <copyright file="DefaultBrowserServiceTests.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Infrastructure.SystemIntegration;
using FluentAssertions;

namespace BrowserSelector.InfrastructureTests;

/// <summary>
/// <see cref="DefaultBrowserService"/> のProgId比較ロジックを検証する。
/// 実レジストリには依存せず、<see cref="DefaultBrowserService.IsExpectedProgId"/>の
/// 純粋な比較ロジックのみを対象とする.
/// </summary>
public class DefaultBrowserServiceTests
{
    [Theory]
    [InlineData("BrowserSelector.https", true)]
    [InlineData("BROWSERSELECTOR.HTTPS", true)] // 大文字小文字は区別しない
    [InlineData("ChromeHTML", false)]
    [InlineData("MSEdgeHTM", false)]
    [InlineData(null, false)] // UserChoiceキー未設定
    [InlineData("", false)]
    public void IsExpectedProgId_ReturnsExpectedResult(string? progId, bool expected)
    {
        bool result = DefaultBrowserService.IsExpectedProgId(progId);

        result.Should().Be(expected);
    }

    [Fact]
    public void IsDefaultBrowser_DoesNotThrowInTestEnvironment()
    {
        // 実際のUserChoiceキーへ依存する経路だが、キー不存在時もfalseへ安全にフォールバックし
        // 例外を投げないことを確認する。
        DefaultBrowserService service = new();

        Action act = () => service.IsDefaultBrowser();

        act.Should().NotThrow();
    }

    [Fact]
    public void GetDefaultBrowserDisplayName_DoesNotThrowInTestEnvironment()
    {
        DefaultBrowserService service = new();

        Action act = () => service.GetDefaultBrowserDisplayName();

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("\"C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe\" -- \"%1\"", "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe")]
    [InlineData("\"C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe\" %1", "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe")]
    [InlineData("C:\\Browsers\\browser.exe %1", "C:\\Browsers\\browser.exe")]
    [InlineData("C:\\Browsers\\browser.exe", "C:\\Browsers\\browser.exe")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData("\"unterminated", null)] // 閉じ引用符が無い異常値
    public void ExtractExecutablePath_ReturnsExpectedResult(string? command, string? expected)
    {
        string? result = DefaultBrowserService.ExtractExecutablePath(command);

        result.Should().Be(expected);
    }
}
