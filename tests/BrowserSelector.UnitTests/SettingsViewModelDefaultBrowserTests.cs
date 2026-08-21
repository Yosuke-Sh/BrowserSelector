// <copyright file="SettingsViewModelDefaultBrowserTests.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Presentation.ViewModels;
using FluentAssertions;
using Moq;

namespace BrowserSelector.UnitTests;

/// <summary>
/// <see cref="SettingsViewModel"/> のOS既定ブラウザ設定導線を検証する。
/// 以前は既定ブラウザかどうかの判定結果もSettingsViewModelが保持していたが、
/// 判定処理自体が「ボタンを押しても何も表示されない」不具合報告につながり撤去したため、
/// ここでは「既定のアプリ」設定画面を開くボタンの動作のみを検証する.
/// </summary>
public class SettingsViewModelDefaultBrowserTests
{
    private readonly Mock<ISettingsService> _mockSettingsService;
    private readonly Mock<IBrowserService> _mockBrowserService;
    private readonly Mock<ILocalizationService> _mockLocalizationService;
    private readonly Mock<ICustomLanguageService> _mockCustomLanguageService;
    private readonly Mock<IUrlRuleService> _mockUrlRuleService;
    private readonly Mock<ILogService> _mockLogService;
    private readonly Mock<IDefaultBrowserService> _mockDefaultBrowserService;

    public SettingsViewModelDefaultBrowserTests()
    {
        _mockSettingsService = new Mock<ISettingsService>();
        _ = _mockSettingsService.Setup(x => x.LoadAppSettingsAsync()).ReturnsAsync(new AppSettings());
        _ = _mockSettingsService.Setup(x => x.LoadVisualSettingsAsync()).ReturnsAsync(new VisualSettings());
        _ = _mockSettingsService.Setup(x => x.LoadLogSettingsAsync()).ReturnsAsync(new LogSettings());

        _mockBrowserService = new Mock<IBrowserService>();
        _ = _mockBrowserService.Setup(x => x.GetAllBrowsersAsync()).ReturnsAsync([]);

        _mockLocalizationService = new Mock<ILocalizationService>();
        _ = _mockLocalizationService.Setup(x => x.GetString(It.IsAny<string>())).Returns(string.Empty);

        _mockCustomLanguageService = new Mock<ICustomLanguageService>();
        _ = _mockCustomLanguageService.Setup(x => x.GetAvailableLanguagesAsync()).ReturnsAsync([]);

        _mockUrlRuleService = new Mock<IUrlRuleService>();
        _ = _mockUrlRuleService.Setup(x => x.GetAllRulesAsync()).ReturnsAsync([]);

        _mockLogService = new Mock<ILogService>();
        _mockDefaultBrowserService = new Mock<IDefaultBrowserService>();
    }

    private SettingsViewModel CreateViewModel(IDefaultBrowserService? defaultBrowserService)
    {
        return new SettingsViewModel(
            _mockSettingsService.Object,
            _mockBrowserService.Object,
            _mockLocalizationService.Object,
            _mockCustomLanguageService.Object,
            _mockUrlRuleService.Object,
            _mockLogService.Object,
            externalLinkService: null,
            updateService: null,
            defaultBrowserService: defaultBrowserService);
    }

    [Fact]
    public void OpenDefaultAppsSettingsCommand_WhenServiceSucceeds_InvokesUnderlyingServiceOnce()
    {
        _ = _mockDefaultBrowserService.Setup(x => x.OpenDefaultAppsSettings()).Returns(true);
        SettingsViewModel viewModel = CreateViewModel(_mockDefaultBrowserService.Object);

        viewModel.OpenDefaultAppsSettingsCommand.Execute(null);

        _mockDefaultBrowserService.Verify(x => x.OpenDefaultAppsSettings(), Times.Once);
    }

    [Fact]
    public void OpenDefaultAppsSettingsCommand_WhenServiceFails_DoesNotThrow()
    {
        _ = _mockDefaultBrowserService.Setup(x => x.OpenDefaultAppsSettings()).Returns(false);
        SettingsViewModel viewModel = CreateViewModel(_mockDefaultBrowserService.Object);

        Action act = () => viewModel.OpenDefaultAppsSettingsCommand.Execute(null);

        act.Should().NotThrow();
    }

    [Fact]
    public void OpenDefaultAppsSettingsCommand_WithNoService_DoesNotThrow()
    {
        SettingsViewModel viewModel = CreateViewModel(defaultBrowserService: null);

        Action act = () => viewModel.OpenDefaultAppsSettingsCommand.Execute(null);

        act.Should().NotThrow();
    }
}
