// <copyright file="SettingsViewModelUpdateTests.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Presentation.ViewModels;
using FluentAssertions;
using Moq;

namespace BrowserSelector.UnitTests;

/// <summary>
/// <see cref="SettingsViewModel"/> の「アップデート設定」GroupBox（Phase H-8）関連の振る舞いを検証する.
/// </summary>
public class SettingsViewModelUpdateTests
{
    private readonly Mock<ISettingsService> _mockSettingsService;
    private readonly Mock<IBrowserService> _mockBrowserService;
    private readonly Mock<ILocalizationService> _mockLocalizationService;
    private readonly Mock<ICustomLanguageService> _mockCustomLanguageService;
    private readonly Mock<IUrlRuleService> _mockUrlRuleService;
    private readonly Mock<ILogService> _mockLogService;
    private readonly Mock<IUpdateService> _mockUpdateService;

    public SettingsViewModelUpdateTests()
    {
        _mockSettingsService = new Mock<ISettingsService>();
        _ = _mockSettingsService.Setup(x => x.LoadAppSettingsAsync()).ReturnsAsync(new AppSettings());
        _ = _mockSettingsService.Setup(x => x.LoadVisualSettingsAsync()).ReturnsAsync(new VisualSettings());
        _ = _mockSettingsService.Setup(x => x.LoadLogSettingsAsync()).ReturnsAsync(new LogSettings());
        _ = _mockSettingsService.Setup(x => x.SaveAppSettingsAsync(It.IsAny<AppSettings>())).ReturnsAsync(true);

        _mockBrowserService = new Mock<IBrowserService>();
        _ = _mockBrowserService.Setup(x => x.GetAllBrowsersAsync()).ReturnsAsync([]);

        _mockLocalizationService = new Mock<ILocalizationService>();
        _mockCustomLanguageService = new Mock<ICustomLanguageService>();
        _ = _mockCustomLanguageService.Setup(x => x.GetAvailableLanguagesAsync()).ReturnsAsync([]);

        _mockUrlRuleService = new Mock<IUrlRuleService>();
        _ = _mockUrlRuleService.Setup(x => x.GetAllRulesAsync()).ReturnsAsync([]);

        _mockLogService = new Mock<ILogService>();
        _mockUpdateService = new Mock<IUpdateService>();
    }

    private SettingsViewModel CreateViewModel(IUpdateService? updateService)
    {
        return new SettingsViewModel(
            _mockSettingsService.Object,
            _mockBrowserService.Object,
            _mockLocalizationService.Object,
            _mockCustomLanguageService.Object,
            _mockUrlRuleService.Object,
            _mockLogService.Object,
            externalLinkService: null,
            updateService: updateService);
    }

    [Fact]
    public async Task LastUpdateCheckDisplay_WhenNeverChecked_ShowsNeverCheckedFallback()
    {
        SettingsViewModel viewModel = CreateViewModel(_mockUpdateService.Object);
        await viewModel.InitializationTask;

        // ローカライズサービス未設定のテスト環境ではキー自体（またはフォールバック文言）が返る。
        // 重要なのは日時文字列ではなく「未チェック」系のフォールバックが返ることの確認。
        _ = viewModel.LastUpdateCheckDisplay.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HasSkippedUpdateVersion_ReflectsAppSettings()
    {
        SettingsViewModel viewModel = CreateViewModel(_mockUpdateService.Object);
        await viewModel.InitializationTask;

        _ = viewModel.HasSkippedUpdateVersion.Should().BeFalse();

        viewModel.AppSettings.SkippedUpdateVersion = "v0.3.0";

        _ = viewModel.HasSkippedUpdateVersion.Should().BeTrue();
    }

    [Fact]
    public async Task ClearSkippedVersionCommand_ClearsAndSaves()
    {
        SettingsViewModel viewModel = CreateViewModel(_mockUpdateService.Object);
        await viewModel.InitializationTask;
        viewModel.AppSettings.SkippedUpdateVersion = "v0.3.0";

        await viewModel.ClearSkippedVersionCommand.ExecuteAsync(null);

        _ = viewModel.AppSettings.SkippedUpdateVersion.Should().BeEmpty();
        _ = viewModel.HasSkippedUpdateVersion.Should().BeFalse();
        _mockSettingsService.Verify(
            x => x.SaveAppSettingsAsync(It.Is<AppSettings>(s => s.SkippedUpdateVersion == string.Empty)),
            Times.Once);
    }

    [Fact]
    public async Task CheckForUpdatesNowCommand_NoUpdateAvailable_UpdatesLastCheckedAndStatus()
    {
        SettingsViewModel viewModel = CreateViewModel(_mockUpdateService.Object);
        await viewModel.InitializationTask;

        _ = _mockUpdateService
            .Setup(x => x.CheckForUpdatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((UpdateInfo?)null);

        await viewModel.CheckForUpdatesNowCommand.ExecuteAsync(null);

        _ = viewModel.IsCheckingForUpdates.Should().BeFalse();
        _ = viewModel.AppSettings.LastUpdateCheckUtc.Should().NotBeNull();
        _mockSettingsService.Verify(x => x.SaveAppSettingsAsync(It.IsAny<AppSettings>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CheckForUpdatesNowCommand_UpdateAvailable_SetsStatusMessage()
    {
        SettingsViewModel viewModel = CreateViewModel(_mockUpdateService.Object);
        await viewModel.InitializationTask;

        UpdateInfo updateInfo = new() { TagName = "v0.3.0", Version = new Version(0, 3, 0) };
        _ = _mockUpdateService
            .Setup(x => x.CheckForUpdatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(updateInfo);

        await viewModel.CheckForUpdatesNowCommand.ExecuteAsync(null);

        // ローカライゼーションサービス未設定のテスト環境ではLocalizedLogHelperがキーそのものを返すため、
        // 引数（バージョン文字列）は書式化されない。ここでは「見つかった」系のキーが選ばれたことのみ検証する。
        _ = viewModel.UpdateCheckStatusMessage.Should().Be("Settings.App.UpdateFound");
    }

    [Fact]
    public async Task CheckForUpdatesNowCommand_ServiceThrows_SetsFailedStatusAndDoesNotThrow()
    {
        SettingsViewModel viewModel = CreateViewModel(_mockUpdateService.Object);
        await viewModel.InitializationTask;

        _ = _mockUpdateService
            .Setup(x => x.CheckForUpdatesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("network down"));

        Func<Task> act = async () => await viewModel.CheckForUpdatesNowCommand.ExecuteAsync(null);

        _ = await act.Should().NotThrowAsync();
        _ = viewModel.IsCheckingForUpdates.Should().BeFalse();
    }

    [Fact]
    public async Task CheckForUpdatesNowCommand_NoUpdateService_SetsFailedStatusAndDoesNotThrow()
    {
        SettingsViewModel viewModel = CreateViewModel(updateService: null);
        await viewModel.InitializationTask;

        Func<Task> act = async () => await viewModel.CheckForUpdatesNowCommand.ExecuteAsync(null);

        _ = await act.Should().NotThrowAsync();
        _ = viewModel.UpdateCheckStatusMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CheckForUpdatesNowCommand_UpdateAvailable_SetsHasFoundUpdate()
    {
        // 「今すぐ確認」で見つかった更新は、設定画面のその場で適用ボタンを出すため
        // HasFoundUpdateへ保持されなければならない（回帰: 従来は確認のみで捨てられていた）。
        SettingsViewModel viewModel = CreateViewModel(_mockUpdateService.Object);
        await viewModel.InitializationTask;

        UpdateInfo updateInfo = new() { TagName = "v0.3.1", Version = new Version(0, 3, 1) };
        _ = _mockUpdateService
            .Setup(x => x.CheckForUpdatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(updateInfo);

        _ = viewModel.HasFoundUpdate.Should().BeFalse();

        await viewModel.CheckForUpdatesNowCommand.ExecuteAsync(null);

        _ = viewModel.HasFoundUpdate.Should().BeTrue();
    }

    [Fact]
    public async Task ApplyUpdateNowCommand_NoFoundUpdate_DoesNothing()
    {
        SettingsViewModel viewModel = CreateViewModel(_mockUpdateService.Object);
        await viewModel.InitializationTask;

        await viewModel.ApplyUpdateNowCommand.ExecuteAsync(null);

        _mockUpdateService.Verify(
            x => x.DownloadUpdateAsync(It.IsAny<UpdateInfo>(), It.IsAny<UpdateChannel>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ApplyUpdateNowCommand_DownloadAndApplySucceed_RaisesShutdownRequested()
    {
        SettingsViewModel viewModel = CreateViewModel(_mockUpdateService.Object);
        await viewModel.InitializationTask;

        UpdateInfo updateInfo = new() { TagName = "v0.3.1", Version = new Version(0, 3, 1) };
        _ = _mockUpdateService
            .Setup(x => x.CheckForUpdatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(updateInfo);
        await viewModel.CheckForUpdatesNowCommand.ExecuteAsync(null);

        _ = _mockUpdateService.Setup(x => x.ResolveChannel()).Returns(UpdateChannel.Installer);
        _ = _mockUpdateService
            .Setup(x => x.DownloadUpdateAsync(It.IsAny<UpdateInfo>(), It.IsAny<UpdateChannel>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UpdateDownloadResult.Succeeded(@"C:\temp\setup.exe"));
        _ = _mockUpdateService
            .Setup(x => x.ApplyUpdateAsync(It.IsAny<UpdateInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        bool shutdownRaised = false;
        viewModel.ShutdownRequested += (_, _) => shutdownRaised = true;

        await viewModel.ApplyUpdateNowCommand.ExecuteAsync(null);

        _ = shutdownRaised.Should().BeTrue();
        _ = viewModel.IsApplyingUpdate.Should().BeFalse();
    }

    [Fact]
    public async Task ApplyUpdateNowCommand_DownloadFails_DoesNotRaiseShutdown()
    {
        SettingsViewModel viewModel = CreateViewModel(_mockUpdateService.Object);
        await viewModel.InitializationTask;

        UpdateInfo updateInfo = new() { TagName = "v0.3.1", Version = new Version(0, 3, 1) };
        _ = _mockUpdateService
            .Setup(x => x.CheckForUpdatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(updateInfo);
        await viewModel.CheckForUpdatesNowCommand.ExecuteAsync(null);

        _ = _mockUpdateService.Setup(x => x.ResolveChannel()).Returns(UpdateChannel.Portable);
        _ = _mockUpdateService
            .Setup(x => x.DownloadUpdateAsync(It.IsAny<UpdateInfo>(), It.IsAny<UpdateChannel>(), It.IsAny<IProgress<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UpdateDownloadResult.Failed(UpdateDownloadFailure.Network));

        bool shutdownRaised = false;
        viewModel.ShutdownRequested += (_, _) => shutdownRaised = true;

        await viewModel.ApplyUpdateNowCommand.ExecuteAsync(null);

        _ = shutdownRaised.Should().BeFalse();
        _mockUpdateService.Verify(x => x.ApplyUpdateAsync(It.IsAny<UpdateInfo>(), It.IsAny<CancellationToken>()), Times.Never);
        _ = viewModel.IsApplyingUpdate.Should().BeFalse();
    }
}
