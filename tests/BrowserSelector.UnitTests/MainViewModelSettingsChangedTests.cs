// <copyright file="MainViewModelSettingsChangedTests.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Presentation.ViewModels;
using FluentAssertions;
using Moq;

namespace BrowserSelector.UnitTests;

/// <summary>
/// <see cref="MainViewModel.OnSettingsChanged"/> のAppSettings変更通知処理を検証する。
/// 設定画面の保存操作でShowTitleBar・ThemeMode・AlwaysOnTop等の外観設定を再起動なしで
/// 即時反映するようにした際、MainViewModel側の状態同期（CloseAfterLaunch）が
/// 正しく行われることを確認する.
/// </summary>
public class MainViewModelSettingsChangedTests
{
    private readonly Mock<IBrowserService> _mockBrowserService;
    private readonly Mock<ISettingsService> _mockSettingsService;
    private readonly Mock<ILocalizationService> _mockLocalizationService;
    private readonly Mock<ICustomLanguageService> _mockCustomLanguageService;
    private readonly Mock<IUrlRuleService> _mockUrlRuleService;
    private readonly Mock<ILogService> _mockLogService;

    public MainViewModelSettingsChangedTests()
    {
        _mockBrowserService = new Mock<IBrowserService>();
        _ = _mockBrowserService.Setup(x => x.GetAllBrowsersAsync()).ReturnsAsync([]);
        _ = _mockBrowserService.Setup(x => x.DetectBrowsersAsync()).ReturnsAsync([]);

        _mockSettingsService = new Mock<ISettingsService>();
        _ = _mockSettingsService.Setup(x => x.LoadAppSettingsAsync())
            .ReturnsAsync(new AppSettings { CloseAfterUrlRuleMatch = false });
        _ = _mockSettingsService.Setup(x => x.LoadVisualSettingsAsync()).ReturnsAsync(new VisualSettings());

        _mockLocalizationService = new Mock<ILocalizationService>();
        _mockCustomLanguageService = new Mock<ICustomLanguageService>();
        _mockUrlRuleService = new Mock<IUrlRuleService>();
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
    public void OnSettingsChanged_WithAppSettingsUpdate_SyncsCloseAfterLaunch()
    {
        MainViewModel viewModel = CreateViewModel();
        AppSettings updatedSettings = new() { CloseAfterUrlRuleMatch = true };

        viewModel.OnSettingsChanged(viewModel, new SettingsChangedEventArgs("AppSettings", null, updatedSettings));

        viewModel.CloseAfterLaunch.Should().BeTrue();
    }

    [Fact]
    public void OnSettingsChanged_WithAppSettingsUpdate_DoesNotThrowWithoutLiveApplication()
    {
        // ユニットテスト環境ではApplication.Currentが存在しないため、
        // MainWindow.ApplyAppSettingsへの委譲部分は安全にスキップされる必要がある。
        MainViewModel viewModel = CreateViewModel();

        Action act = () => viewModel.OnSettingsChanged(
            viewModel,
            new SettingsChangedEventArgs("AppSettings", null, new AppSettings()));

        act.Should().NotThrow();
    }

    [Fact]
    public void OnSettingsChanged_WithVisualSettingsType_DoesNotAffectCloseAfterLaunch()
    {
        MainViewModel viewModel = CreateViewModel();
        bool initialValue = viewModel.CloseAfterLaunch;

        viewModel.OnSettingsChanged(viewModel, new SettingsChangedEventArgs("VisualSettings", null, new VisualSettings()));

        viewModel.CloseAfterLaunch.Should().Be(initialValue);
    }
}
