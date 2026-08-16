using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.Services;
using FluentAssertions;
using Moq;

namespace BrowserSelector.InfrastructureTests;

/// <summary>
/// <see cref="ExternalLinkService"/> のテストクラス（Phase E-2）.
/// 既定ブラウザがBrowserSelector自身の場合に検出済み一覧の先頭の実ブラウザへフォールバックすることを中心に検証する。
/// この経路が壊れると、About画面のGitHubリンクを押した際にBrowserSelectorが再帰的に自分自身を起動してしまう.
/// </summary>
public sealed class ExternalLinkServiceTests
{
    private const string SelfPath = @"C:\Program Files\BrowserSelector\BrowserSelector.exe";

    [Fact]
    public async Task OpenAsync_WithValidDefaultBrowser_LaunchesDefaultBrowser()
    {
        Mock<IBrowserService> browserServiceMock = new();
        Browser chrome = new() { Name = "Chrome", ExecutablePath = @"C:\chrome.exe", IsEnabled = true };
        browserServiceMock.Setup(s => s.GetDefaultBrowserAsync()).ReturnsAsync(chrome);
        browserServiceMock.Setup(s => s.LaunchBrowserAsync(chrome, It.IsAny<Uri>())).ReturnsAsync(true);

        ExternalLinkService service = new(browserServiceMock.Object, null, () => SelfPath);

        bool result = await service.OpenAsync("https://github.com/Yosuke-Sh/BrowserSelector");

        result.Should().BeTrue();
        browserServiceMock.Verify(s => s.LaunchBrowserAsync(chrome, It.IsAny<Uri>()), Times.Once);
    }

    [Fact]
    public async Task OpenAsync_WhenDefaultBrowserIsSelf_FallsBackToFirstRealBrowser()
    {
        // 既定ブラウザがBrowserSelector自身に設定されているケース（罠のシナリオ）。
        // 自己再帰を避け、検出済み一覧の先頭の実ブラウザへフォールバックしなければならない。
        Mock<IBrowserService> browserServiceMock = new();
        Browser self = new() { Name = "BrowserSelector", ExecutablePath = SelfPath, IsEnabled = true, IsDefault = true };
        Browser firstRealBrowser = new() { Name = "Firefox", ExecutablePath = @"C:\firefox.exe", IsEnabled = true };
        Browser secondRealBrowser = new() { Name = "Edge", ExecutablePath = @"C:\edge.exe", IsEnabled = true };

        browserServiceMock.Setup(s => s.GetDefaultBrowserAsync()).ReturnsAsync(self);
        browserServiceMock.Setup(s => s.GetAllBrowsersAsync())
            .ReturnsAsync(new List<Browser> { self, firstRealBrowser, secondRealBrowser });
        browserServiceMock.Setup(s => s.LaunchBrowserAsync(firstRealBrowser, It.IsAny<Uri>())).ReturnsAsync(true);

        ExternalLinkService service = new(browserServiceMock.Object, null, () => SelfPath);

        bool result = await service.OpenAsync("https://github.com/Yosuke-Sh/BrowserSelector");

        result.Should().BeTrue();
        browserServiceMock.Verify(s => s.LaunchBrowserAsync(firstRealBrowser, It.IsAny<Uri>()), Times.Once);
        browserServiceMock.Verify(s => s.LaunchBrowserAsync(self, It.IsAny<Uri>()), Times.Never);
    }

    [Fact]
    public async Task OpenAsync_WhenDefaultBrowserIsNullAndNoRealBrowsersDetected_ReturnsFalse()
    {
        Mock<IBrowserService> browserServiceMock = new();
        browserServiceMock.Setup(s => s.GetDefaultBrowserAsync()).ReturnsAsync((Browser?)null);
        browserServiceMock.Setup(s => s.GetAllBrowsersAsync()).ReturnsAsync(new List<Browser>());

        ExternalLinkService service = new(browserServiceMock.Object, null, () => SelfPath);

        bool result = await service.OpenAsync("https://github.com/Yosuke-Sh/BrowserSelector");

        result.Should().BeFalse();
        browserServiceMock.Verify(s => s.LaunchBrowserAsync(It.IsAny<Browser>(), It.IsAny<Uri>()), Times.Never);
    }

    [Fact]
    public async Task OpenAsync_WhenAllDetectedBrowsersAreSelf_ReturnsFalse()
    {
        Mock<IBrowserService> browserServiceMock = new();
        Browser self = new() { Name = "BrowserSelector", ExecutablePath = SelfPath, IsEnabled = true, IsDefault = true };
        browserServiceMock.Setup(s => s.GetDefaultBrowserAsync()).ReturnsAsync(self);
        browserServiceMock.Setup(s => s.GetAllBrowsersAsync()).ReturnsAsync(new List<Browser> { self });

        ExternalLinkService service = new(browserServiceMock.Object, null, () => SelfPath);

        bool result = await service.OpenAsync("https://github.com/Yosuke-Sh/BrowserSelector");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task OpenAsync_WithInvalidUrl_ReturnsFalseWithoutCallingBrowserService()
    {
        Mock<IBrowserService> browserServiceMock = new();

        ExternalLinkService service = new(browserServiceMock.Object, null, () => SelfPath);

        bool result = await service.OpenAsync("not a valid url");

        result.Should().BeFalse();
        browserServiceMock.Verify(s => s.GetDefaultBrowserAsync(), Times.Never);
    }

    [Fact]
    public async Task OpenAsync_WhenBrowserServiceThrows_ReturnsFalseInsteadOfThrowing()
    {
        Mock<IBrowserService> browserServiceMock = new();
        browserServiceMock.Setup(s => s.GetDefaultBrowserAsync()).ThrowsAsync(new InvalidOperationException("boom"));

        ExternalLinkService service = new(browserServiceMock.Object, null, () => SelfPath);

        bool result = await service.OpenAsync("https://github.com/Yosuke-Sh/BrowserSelector");

        result.Should().BeFalse();
    }
}
