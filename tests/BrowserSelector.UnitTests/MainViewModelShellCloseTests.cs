// <copyright file="MainViewModelShellCloseTests.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Presentation.ViewModels;
using FluentAssertions;
using Moq;

namespace BrowserSelector.UnitTests;

/// <summary>
/// <see cref="MainViewModel"/> のブラウザ起動後アプリ終了処理が、<see cref="IShellCloseService"/> が
/// 注入されている場合にそちらへ委譲されることを検証する.
/// トレイ常駐中に <c>Application.Shutdown()</c> を直接呼ぶと <c>Closing</c> のキャンセルが無視され
/// 常駐インスタンスが終了してしまう不具合があったため、常駐設定の振り分けは
/// <see cref="IShellCloseService.RequestClose"/> 側に一本化する.
/// </summary>
public class MainViewModelShellCloseTests
{
    private readonly Mock<IBrowserService> _mockBrowserService;
    private readonly Mock<ISettingsService> _mockSettingsService;
    private readonly Mock<ILocalizationService> _mockLocalizationService;
    private readonly Mock<ICustomLanguageService> _mockCustomLanguageService;
    private readonly Mock<IUrlRuleService> _mockUrlRuleService;
    private readonly Mock<ILogService> _mockLogService;
    private readonly Mock<IShellCloseService> _mockShellCloseService;
    private readonly Browser _browser;

    public MainViewModelShellCloseTests()
    {
        _browser = new Browser
        {
            Name = "Test Browser",
            ExecutablePath = @"C:\Windows\System32\cmd.exe",
            Arguments = "/c exit"
        };

        _mockBrowserService = new Mock<IBrowserService>();
        _ = _mockBrowserService.Setup(x => x.GetAllBrowsersAsync()).ReturnsAsync([_browser]);
        _ = _mockBrowserService.Setup(x => x.DetectBrowsersAsync()).ReturnsAsync([]);
        _ = _mockBrowserService
            .Setup(x => x.LaunchBrowserAsync(It.IsAny<Browser>(), It.IsAny<Uri>()))
            .ReturnsAsync(true);
        _ = _mockBrowserService.Setup(x => x.UpdateUsageAsync(It.IsAny<Browser>())).Returns(Task.CompletedTask);

        _mockSettingsService = new Mock<ISettingsService>();
        AppSettings appSettings = new() { CloseAfterUrlRuleMatch = true };
        _ = _mockSettingsService.Setup(x => x.LoadAppSettingsAsync()).ReturnsAsync(appSettings);
        _ = _mockSettingsService.Setup(x => x.LoadVisualSettingsAsync()).ReturnsAsync(new VisualSettings());

        _mockLocalizationService = new Mock<ILocalizationService>();
        _mockCustomLanguageService = new Mock<ICustomLanguageService>();
        _mockUrlRuleService = new Mock<IUrlRuleService>();
        _mockLogService = new Mock<ILogService>();
        _mockShellCloseService = new Mock<IShellCloseService>();
    }

    private MainViewModel CreateViewModel(IShellCloseService? shellCloseService)
    {
        return new MainViewModel(
            _mockBrowserService.Object,
            _mockSettingsService.Object,
            _mockLocalizationService.Object,
            _mockCustomLanguageService.Object,
            _mockUrlRuleService.Object,
            _mockLogService.Object,
            externalLinkService: null,
            updateService: null,
            shellCloseService: shellCloseService);
    }

    [Fact]
    public async Task LaunchBrowser_WithShellCloseServiceInjected_CallsRequestCloseNotApplicationShutdown()
    {
        MainViewModel viewModel = CreateViewModel(_mockShellCloseService.Object);
        viewModel.Url = "https://example.com/";

        await viewModel.LaunchBrowserCommand.ExecuteAsync(_browser);

        _mockShellCloseService.Verify(x => x.RequestClose(), Times.Once);
    }
}
