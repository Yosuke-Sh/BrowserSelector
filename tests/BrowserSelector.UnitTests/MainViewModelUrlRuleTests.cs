// <copyright file="MainViewModelUrlRuleTests.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Presentation.ViewModels;
using FluentAssertions;
using Moq;

namespace BrowserSelector.UnitTests;

/// <summary>
/// <see cref="MainViewModel"/> のURLルール自動適用時のブラウザ起動回数を検証する.
/// かつてURLセッターの変更通知（OnUrlChanged）と<c>SetInitialUrl</c>内の明示呼び出しが
/// 二重にURLルール適用処理を発火させ、ブラウザが二重起動する不具合があった.
/// </summary>
public class MainViewModelUrlRuleTests
{
    private const string TestUrl = "https://example.com/";

    private readonly Mock<IBrowserService> _mockBrowserService;
    private readonly Mock<ISettingsService> _mockSettingsService;
    private readonly Mock<ILocalizationService> _mockLocalizationService;
    private readonly Mock<ICustomLanguageService> _mockCustomLanguageService;
    private readonly Mock<IUrlRuleService> _mockUrlRuleService;
    private readonly Mock<ILogService> _mockLogService;
    private readonly Browser _matchingBrowser;

    public MainViewModelUrlRuleTests()
    {
        _matchingBrowser = new Browser
        {
            Name = "Test Browser",
            ExecutablePath = @"C:\Windows\System32\cmd.exe",
            Arguments = "/c exit"
        };

        _mockBrowserService = new Mock<IBrowserService>();
        _ = _mockBrowserService.Setup(x => x.GetAllBrowsersAsync()).ReturnsAsync([_matchingBrowser]);
        _ = _mockBrowserService.Setup(x => x.DetectBrowsersAsync()).ReturnsAsync([]);
        _ = _mockBrowserService
            .Setup(x => x.LaunchBrowserAsync(It.IsAny<Browser>(), It.IsAny<Uri>()))
            .ReturnsAsync(true);
        _ = _mockBrowserService.Setup(x => x.UpdateUsageAsync(It.IsAny<Browser>())).Returns(Task.CompletedTask);

        _mockSettingsService = new Mock<ISettingsService>();
        AppSettings appSettings = new() { CloseAfterUrlRuleMatch = false };
        _ = _mockSettingsService.Setup(x => x.LoadAppSettingsAsync()).ReturnsAsync(appSettings);
        _ = _mockSettingsService.Setup(x => x.LoadVisualSettingsAsync()).ReturnsAsync(new VisualSettings());

        _mockLocalizationService = new Mock<ILocalizationService>();
        _mockCustomLanguageService = new Mock<ICustomLanguageService>();

        _mockUrlRuleService = new Mock<IUrlRuleService>();
        _ = _mockUrlRuleService
            .Setup(x => x.FindMatchingBrowserAsync(It.IsAny<Uri>(), It.IsAny<IEnumerable<Browser>>()))
            .ReturnsAsync(_matchingBrowser);

        _mockLogService = new Mock<ILogService>();
    }

    private MainViewModel CreateViewModel()
    {
        return new MainViewModel(
            _mockBrowserService.Object,
            _mockSettingsService.Object,
            _mockLocalizationService.Object,
            _mockCustomLanguageService.Object,
            _mockUrlRuleService.Object,
            _mockLogService.Object);
    }

    [Fact]
    public async Task SetInitialUrl_WithMatchingRule_LaunchesBrowserExactlyOnce()
    {
        MainViewModel viewModel = CreateViewModel();

        viewModel.SetInitialUrl(TestUrl);

        // OnUrlChangedはfire-and-forgetで起動されるため、完了を待機する。
        await WaitUntilAsync(() => _mockBrowserService.Invocations.Any(
            i => i.Method.Name == nameof(IBrowserService.LaunchBrowserAsync)));

        _mockBrowserService.Verify(
            x => x.LaunchBrowserAsync(It.IsAny<Browser>(), It.IsAny<Uri>()),
            Times.Once);
    }

    [Fact]
    public async Task SetInitialUrl_CalledTwiceWithSameUrl_LaunchesBrowserOnlyOnce()
    {
        MainViewModel viewModel = CreateViewModel();

        viewModel.SetInitialUrl(TestUrl);
        await WaitUntilAsync(() => _mockBrowserService.Invocations.Any(
            i => i.Method.Name == nameof(IBrowserService.LaunchBrowserAsync)));

        // 同一URLの再設定（例: トレイ経由の再配信）では追加の自動起動を行わない。
        viewModel.SetInitialUrl(TestUrl);
        await Task.Delay(100);

        _mockBrowserService.Verify(
            x => x.LaunchBrowserAsync(It.IsAny<Browser>(), It.IsAny<Uri>()),
            Times.Once);
    }

    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 2000)
    {
        DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(10);
        }
    }
}
