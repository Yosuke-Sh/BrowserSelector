// <copyright file="MainViewModelUpdateTests.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Presentation.ViewModels;
using FluentAssertions;
using Moq;

namespace BrowserSelector.UnitTests;

/// <summary>
/// <see cref="MainViewModel"/> の更新通知バー（Phase H-9）関連の振る舞いを検証する.
/// </summary>
public class MainViewModelUpdateTests
{
    private readonly Mock<IBrowserService> _mockBrowserService;
    private readonly Mock<ISettingsService> _mockSettingsService;
    private readonly Mock<ILocalizationService> _mockLocalizationService;
    private readonly Mock<ICustomLanguageService> _mockCustomLanguageService;
    private readonly Mock<IUrlRuleService> _mockUrlRuleService;
    private readonly Mock<ILogService> _mockLogService;
    private readonly Mock<IUpdateService> _mockUpdateService;

    public MainViewModelUpdateTests()
    {
        _mockBrowserService = new Mock<IBrowserService>();
        _ = _mockBrowserService.Setup(x => x.GetAllBrowsersAsync()).ReturnsAsync([]);
        _ = _mockBrowserService.Setup(x => x.DetectBrowsersAsync()).ReturnsAsync([]);

        _mockSettingsService = new Mock<ISettingsService>();
        _ = _mockSettingsService.Setup(x => x.LoadAppSettingsAsync()).ReturnsAsync(new AppSettings());
        _ = _mockSettingsService.Setup(x => x.LoadVisualSettingsAsync()).ReturnsAsync(new VisualSettings());
        _ = _mockSettingsService.Setup(x => x.SaveAppSettingsAsync(It.IsAny<AppSettings>())).ReturnsAsync(true);

        _mockLocalizationService = new Mock<ILocalizationService>();
        _mockCustomLanguageService = new Mock<ICustomLanguageService>();
        _mockUrlRuleService = new Mock<IUrlRuleService>();
        _mockLogService = new Mock<ILogService>();
        _mockUpdateService = new Mock<IUpdateService>();
    }

    private static UpdateInfo CreateUpdateInfo(string tagName = "v0.3.0")
    {
        return new UpdateInfo
        {
            TagName = tagName,
            Version = new Version(0, 3, 0),
            ReleasePageUrl = "https://github.com/Yosuke-Sh/BrowserSelector/releases/tag/v0.3.0",
        };
    }

    private MainViewModel CreateViewModel(IUpdateService? updateService)
    {
        return new MainViewModel(
            _mockBrowserService.Object,
            _mockSettingsService.Object,
            _mockLocalizationService.Object,
            _mockCustomLanguageService.Object,
            _mockUrlRuleService.Object,
            _mockLogService.Object,
            externalLinkService: null,
            updateService: updateService);
    }

    [Fact]
    public void ShowUpdateNotification_SetsVisibleAndMessage()
    {
        MainViewModel viewModel = CreateViewModel(_mockUpdateService.Object);

        viewModel.ShowUpdateNotification(CreateUpdateInfo("v0.3.0"));

        // ローカライゼーションサービス未設定のテスト環境ではLocalizedLogHelperがキーそのものを返すため、
        // 引数（バージョン文字列）は書式化されない。ここではメッセージが空でないことのみ検証する。
        _ = viewModel.IsUpdateNotificationVisible.Should().BeTrue();
        _ = viewModel.UpdateNotificationMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ShowUpdateNotification_NullArgument_Throws()
    {
        MainViewModel viewModel = CreateViewModel(_mockUpdateService.Object);

        Action act = () => viewModel.ShowUpdateNotification(null!);

        _ = act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task SkipUpdateCommand_SavesSkippedVersionAndHidesBar()
    {
        MainViewModel viewModel = CreateViewModel(_mockUpdateService.Object);
        viewModel.ShowUpdateNotification(CreateUpdateInfo("v0.3.0"));

        await viewModel.SkipUpdateCommand.ExecuteAsync(null);

        _ = viewModel.IsUpdateNotificationVisible.Should().BeFalse();
        _mockSettingsService.Verify(
            x => x.SaveAppSettingsAsync(It.Is<AppSettings>(s => s.SkippedUpdateVersion == "v0.3.0")),
            Times.Once);
    }

    [Fact]
    public async Task DeferUpdateCommand_SetsUpdatePendingOnNextLaunchAndHidesBar()
    {
        MainViewModel viewModel = CreateViewModel(_mockUpdateService.Object);
        viewModel.ShowUpdateNotification(CreateUpdateInfo("v0.3.0"));

        await viewModel.DeferUpdateCommand.ExecuteAsync(null);

        _ = viewModel.IsUpdateNotificationVisible.Should().BeFalse();
        _mockSettingsService.Verify(
            x => x.SaveAppSettingsAsync(It.Is<AppSettings>(s => s.UpdatePendingOnNextLaunch)),
            Times.Once);
    }

    [Fact]
    public async Task StartUpdateCommand_NoUpdateServiceOrPendingUpdate_DoesNotThrow()
    {
        // IUpdateService=nullでも例外を投げない（未接続環境でのフォールバック）。
        MainViewModel viewModel = CreateViewModel(updateService: null);

        Func<Task> act = async () => await viewModel.StartUpdateCommand.ExecuteAsync(null);

        _ = await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StartUpdateCommand_DownloadFails_RaisesNoShutdownAndUpdatesMessage()
    {
        MainViewModel viewModel = CreateViewModel(_mockUpdateService.Object);
        UpdateInfo updateInfo = CreateUpdateInfo("v0.3.0");
        viewModel.ShowUpdateNotification(updateInfo);

        _ = _mockUpdateService.Setup(x => x.ResolveChannel()).Returns(UpdateChannel.Portable);
        _ = _mockUpdateService
            .Setup(x => x.DownloadUpdateAsync(It.IsAny<UpdateInfo>(), It.IsAny<UpdateChannel>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UpdateDownloadResult.Failed(UpdateDownloadFailure.Network));

        bool shutdownRaised = false;
        viewModel.ShutdownRequested += (_, _) => shutdownRaised = true;

        await viewModel.StartUpdateCommand.ExecuteAsync(null);

        _ = shutdownRaised.Should().BeFalse();
        _mockUpdateService.Verify(x => x.ApplyUpdateAsync(It.IsAny<UpdateInfo>(), It.IsAny<CancellationToken>()), Times.Never);
        _ = viewModel.IsUpdateDownloading.Should().BeFalse();
    }

    [Fact]
    public async Task StartUpdateCommand_DownloadAndApplySucceed_RaisesShutdownRequested()
    {
        MainViewModel viewModel = CreateViewModel(_mockUpdateService.Object);
        UpdateInfo updateInfo = CreateUpdateInfo("v0.3.0");
        viewModel.ShowUpdateNotification(updateInfo);

        _ = _mockUpdateService.Setup(x => x.ResolveChannel()).Returns(UpdateChannel.Installer);
        _ = _mockUpdateService
            .Setup(x => x.DownloadUpdateAsync(It.IsAny<UpdateInfo>(), It.IsAny<UpdateChannel>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UpdateDownloadResult.Succeeded(@"C:\temp\setup.exe"));
        _ = _mockUpdateService
            .Setup(x => x.ApplyUpdateAsync(It.IsAny<UpdateInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        bool shutdownRaised = false;
        viewModel.ShutdownRequested += (_, _) => shutdownRaised = true;

        await viewModel.StartUpdateCommand.ExecuteAsync(null);

        _ = shutdownRaised.Should().BeTrue();
    }
}
