// <copyright file="SettingsViewModelWindowSizeTests.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Presentation.ViewModels;
using FluentAssertions;
using Moq;

namespace BrowserSelector.UnitTests;

/// <summary>
/// <see cref="SettingsViewModel"/> の「現在のサイズを取得」コマンドを検証する.
/// </summary>
public class SettingsViewModelWindowSizeTests
{
    private readonly SettingsViewModel _viewModel;

    public SettingsViewModelWindowSizeTests()
    {
        Mock<ISettingsService> mockSettingsService = new();
        Mock<IBrowserService> mockBrowserService = new();
        Mock<ILocalizationService> mockLocalizationService = new();
        Mock<ICustomLanguageService> mockCustomLanguageService = new();
        Mock<IUrlRuleService> mockUrlRuleService = new();
        Mock<ILogService> mockLogService = new();

        _ = mockSettingsService.Setup(x => x.LoadAppSettingsAsync()).ReturnsAsync(new AppSettings());
        _ = mockSettingsService
            .Setup(x => x.LoadVisualSettingsAsync())
            .ReturnsAsync(new VisualSettings { InitialWindowWidth = 800, InitialWindowHeight = 600 });
        _ = mockBrowserService.Setup(x => x.GetAllBrowsersAsync()).ReturnsAsync([]);
        _ = mockUrlRuleService.Setup(x => x.GetAllRulesAsync()).ReturnsAsync([]);
        _ = mockSettingsService.Setup(x => x.LoadLogSettingsAsync()).ReturnsAsync(new LogSettings());
        _ = mockCustomLanguageService.Setup(x => x.GetAvailableLanguagesAsync()).ReturnsAsync([]);

        _viewModel = new SettingsViewModel(
            mockSettingsService.Object,
            mockBrowserService.Object,
            mockLocalizationService.Object,
            mockCustomLanguageService.Object,
            mockUrlRuleService.Object,
            mockLogService.Object);
    }

    [Fact]
    public async Task CaptureCurrentWindowSizeCommand_WhenNoApplicationContext_DoesNotThrowAndLeavesSizeUnchanged()
    {
        // テスト環境ではApplication.Currentがnullのため、コマンドは何もせず安全に戻る必要がある。
        await _viewModel.InitializeAsync();
        double originalWidth = _viewModel.VisualSettings.InitialWindowWidth;
        double originalHeight = _viewModel.VisualSettings.InitialWindowHeight;

        Action act = () => _viewModel.CaptureCurrentWindowSizeCommand.Execute(null);

        act.Should().NotThrow();
        _viewModel.VisualSettings.InitialWindowWidth.Should().Be(originalWidth);
        _viewModel.VisualSettings.InitialWindowHeight.Should().Be(originalHeight);
    }
}
