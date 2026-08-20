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
/// <see cref="SettingsViewModel"/> のOS既定ブラウザ判定・設定導線を検証する.
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
    public async Task InitializeAsync_WithDefaultBrowserServiceReturningTrue_SetsIsDefaultBrowserTrue()
    {
        _ = _mockDefaultBrowserService.Setup(x => x.IsDefaultBrowser()).Returns(true);
        SettingsViewModel viewModel = CreateViewModel(_mockDefaultBrowserService.Object);

        await viewModel.InitializeAsync();

        viewModel.IsDefaultBrowser.Should().BeTrue();
    }

    [Fact]
    public async Task InitializeAsync_WithNoDefaultBrowserService_SetsIsDefaultBrowserFalseWithoutThrowing()
    {
        SettingsViewModel viewModel = CreateViewModel(defaultBrowserService: null);

        Func<Task> act = () => viewModel.InitializeAsync();

        await act.Should().NotThrowAsync();
        viewModel.IsDefaultBrowser.Should().BeFalse();
    }

    [Fact]
    public async Task InitializeAsync_WithDefaultBrowserServiceReturningTrue_LeavesDefaultBrowserNameNull()
    {
        _ = _mockDefaultBrowserService.Setup(x => x.IsDefaultBrowser()).Returns(true);
        SettingsViewModel viewModel = CreateViewModel(_mockDefaultBrowserService.Object);

        await viewModel.InitializeAsync();

        viewModel.DefaultBrowserName.Should().BeNull();
        viewModel.IsDefaultBrowserUnknown.Should().BeFalse();
    }

    [Fact]
    public async Task InitializeAsync_WithOtherBrowserAsDefault_SetsDefaultBrowserName()
    {
        _ = _mockDefaultBrowserService.Setup(x => x.IsDefaultBrowser()).Returns(false);
        _ = _mockDefaultBrowserService.Setup(x => x.GetDefaultBrowserDisplayName()).Returns("Microsoft Edge");
        SettingsViewModel viewModel = CreateViewModel(_mockDefaultBrowserService.Object);

        await viewModel.InitializeAsync();

        viewModel.IsDefaultBrowser.Should().BeFalse();
        viewModel.DefaultBrowserName.Should().Be("Microsoft Edge");
        viewModel.IsDefaultBrowserUnknown.Should().BeFalse();
    }

    [Fact]
    public async Task InitializeAsync_WithUnresolvableDefaultBrowser_SetsIsDefaultBrowserUnknownTrue()
    {
        _ = _mockDefaultBrowserService.Setup(x => x.IsDefaultBrowser()).Returns(false);
        _ = _mockDefaultBrowserService.Setup(x => x.GetDefaultBrowserDisplayName()).Returns((string?)null);
        SettingsViewModel viewModel = CreateViewModel(_mockDefaultBrowserService.Object);

        await viewModel.InitializeAsync();

        viewModel.IsDefaultBrowser.Should().BeFalse();
        viewModel.DefaultBrowserName.Should().BeNull();
        viewModel.IsDefaultBrowserUnknown.Should().BeTrue();
    }

    [Fact]
    public void OpenDefaultAppsSettingsCommand_InvokesUnderlyingService()
    {
        SettingsViewModel viewModel = CreateViewModel(_mockDefaultBrowserService.Object);

        viewModel.OpenDefaultAppsSettingsCommand.Execute(null);

        _mockDefaultBrowserService.Verify(x => x.OpenDefaultAppsSettings(), Times.Once);
    }

    [Fact]
    public void OpenDefaultAppsSettingsCommand_WithNoService_DoesNotThrow()
    {
        SettingsViewModel viewModel = CreateViewModel(defaultBrowserService: null);

        Action act = () => viewModel.OpenDefaultAppsSettingsCommand.Execute(null);

        act.Should().NotThrow();
    }
}
