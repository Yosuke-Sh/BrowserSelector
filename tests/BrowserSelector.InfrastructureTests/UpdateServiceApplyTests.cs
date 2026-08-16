// <copyright file="UpdateServiceApplyTests.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;
using BrowserSelector.Infrastructure.Updates;
using BrowserSelector.InfrastructureTests.TestDoubles;
using FluentAssertions;
using Moq;

namespace BrowserSelector.InfrastructureTests;

/// <summary>
/// Phase H-6: <see cref="UpdateService.ApplyUpdateAsync"/>の2つの適用経路のテスト.
/// 実際にインストーラやUpdater.exeを起動させないため<see cref="IProcessLauncher"/>を差し替える.
/// </summary>
public sealed class UpdateServiceApplyTests : IDisposable
{
    private readonly string _workDirectory;
    private readonly RecordingProcessLauncher _launcher = new();

    // 適用経路のテストはHTTPを一切使わないが、UpdateServiceの構築にファクトリが要るため用意する。
    private readonly StubHttpMessageHandler _handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

    public UpdateServiceApplyTests()
    {
        _workDirectory = Path.Combine(Path.GetTempPath(), $"BSApplyTest_{Guid.NewGuid():N}");
        _ = Directory.CreateDirectory(_workDirectory);
    }

    public void Dispose()
    {
        _handler.Dispose();

        try
        {
            if (Directory.Exists(_workDirectory))
            {
                Directory.Delete(_workDirectory, recursive: true);
            }
        }
        catch (IOException)
        {
            // 後始末の失敗はテスト結果に影響させない。
        }
    }

    [Fact]
    public async Task ApplyUpdateAsync_NotDownloaded_ReturnsFalseWithoutLaunching()
    {
        using UpdateService service = CreateService();
        UpdateInfo updateInfo = new() { IsDownloaded = false, LocalFilePath = null };

        bool result = await service.ApplyUpdateAsync(updateInfo);

        result.Should().BeFalse();
        _launcher.Started.Should().BeEmpty();
    }

    [Fact]
    public async Task ApplyUpdateAsync_DownloadedFlagWithoutPath_ReturnsFalse()
    {
        using UpdateService service = CreateService();
        UpdateInfo updateInfo = new() { IsDownloaded = true, LocalFilePath = string.Empty };

        bool result = await service.ApplyUpdateAsync(updateInfo);

        result.Should().BeFalse();
        _launcher.Started.Should().BeEmpty();
    }

    [Fact]
    public async Task ApplyUpdateAsync_PortableRoute_StartsUpdaterWithNamedArguments()
    {
        string extracted = CreateExtractedDirectory();
        CreateUpdaterExecutable();

        using UpdateService service = CreateService();

        bool result = await service.ApplyUpdateAsync(CreateDownloaded(extracted));

        result.Should().BeTrue();
        _launcher.Started.Should().ContainSingle();

        ProcessStartInfo startInfo = _launcher.Started[0];
        startInfo.FileName.Should().EndWith(UpdateService.UpdaterExecutableName);

        // UseShellExecute=false（昇格不要な経路のため）。
        startInfo.UseShellExecute.Should().BeFalse();

        startInfo.ArgumentList.Should().ContainInOrder("--mode", "apply-zip");
        startInfo.ArgumentList.Should().ContainInOrder("--source", extracted);
        startInfo.ArgumentList.Should().ContainInOrder("--target", _workDirectory);
        startInfo.ArgumentList.Should().Contain("--backup");
        startInfo.ArgumentList.Should().ContainInOrder("--exe", UpdateService.ApplicationExecutableName);

        int pidIndex = startInfo.ArgumentList.IndexOf("--pid");
        pidIndex.Should().BeGreaterThanOrEqualTo(0);
        startInfo.ArgumentList[pidIndex + 1].Should().Be(Environment.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task ApplyUpdateAsync_PortableRouteWithoutUpdaterExecutable_ReturnsFalse()
    {
        string extracted = CreateExtractedDirectory();

        // BrowserSelector.Updater.exe を配置しない。
        using UpdateService service = CreateService();

        bool result = await service.ApplyUpdateAsync(CreateDownloaded(extracted));

        result.Should().BeFalse();
        _launcher.Started.Should().BeEmpty();
    }

    [Fact]
    public async Task ApplyUpdateAsync_PortableRouteWithMissingSource_ReturnsFalse()
    {
        CreateUpdaterExecutable();
        using UpdateService service = CreateService();

        bool result = await service.ApplyUpdateAsync(
            CreateDownloaded(Path.Combine(_workDirectory, "does-not-exist")));

        result.Should().BeFalse();
        _launcher.Started.Should().BeEmpty();
    }

    [Fact]
    public async Task ApplyUpdateAsync_InstallerRoute_StartsInstallerElevatedAndSilent()
    {
        string installerPath = Path.Combine(_workDirectory, "BrowserSelector-Setup-v0.3.0.exe");
        await File.WriteAllTextAsync(installerPath, "installer");

        // Program Files配下を模擬できないため、経路を直接指定してインストーラ起動のみを検証する。
        using UpdateService service = CreateService(baseDirectory: Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));

        bool result = await service.ApplyUpdateAsync(CreateDownloaded(installerPath));

        result.Should().BeTrue();
        _launcher.Started.Should().ContainSingle();

        ProcessStartInfo startInfo = _launcher.Started[0];
        startInfo.FileName.Should().Be(installerPath);

        // 昇格が必要なためUseShellExecute=true + Verb=runas。
        startInfo.UseShellExecute.Should().BeTrue();
        startInfo.Verb.Should().Be("runas");

        // /VERYSILENTではなく/SILENT（進捗を見せた方が不安が小さい）。
        startInfo.Arguments.Should().Be(UpdateService.InstallerArguments);
        startInfo.Arguments.Should().NotContain("/VERYSILENT");
    }

    [Fact]
    public async Task ApplyUpdateAsync_InstallerRouteUacCancelled_ReturnsFalseWithoutThrowing()
    {
        string installerPath = Path.Combine(_workDirectory, "BrowserSelector-Setup-v0.3.0.exe");
        await File.WriteAllTextAsync(installerPath, "installer");

        _launcher.ThrowOnStart = RecordingProcessLauncher.CreateUacCancellation();

        using UpdateService service = CreateService(baseDirectory: Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));

        bool result = await service.ApplyUpdateAsync(CreateDownloaded(installerPath));

        // ユーザーが意図してキャンセルした操作。例外にせずfalseで返す。
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ApplyUpdateAsync_InstallerRouteWithMissingFile_ReturnsFalse()
    {
        using UpdateService service = CreateService(baseDirectory: Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));

        bool result = await service.ApplyUpdateAsync(
            CreateDownloaded(Path.Combine(_workDirectory, "missing-installer.exe")));

        result.Should().BeFalse();
        _launcher.Started.Should().BeEmpty();
    }

    [Fact]
    public async Task ApplyUpdateAsync_Canceled_Throws()
    {
        using UpdateService service = CreateService();
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        Func<Task> act = () => service.ApplyUpdateAsync(CreateDownloaded(_workDirectory), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ApplyUpdateAsync_NullUpdateInfo_Throws()
    {
        using UpdateService service = CreateService();

        Func<Task> act = () => service.ApplyUpdateAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    private static UpdateInfo CreateDownloaded(string localPath) => new()
    {
        Version = new Version(0, 3, 0),
        TagName = "v0.3.0",
        IsDownloaded = true,
        LocalFilePath = localPath,
    };

    private string CreateExtractedDirectory()
    {
        string extracted = Path.Combine(_workDirectory, "extracted");
        _ = Directory.CreateDirectory(extracted);
        File.WriteAllText(Path.Combine(extracted, UpdateService.ApplicationExecutableName), "exe");
        return extracted;
    }

    private void CreateUpdaterExecutable() =>
        File.WriteAllText(Path.Combine(_workDirectory, UpdateService.UpdaterExecutableName), "updater");

    private UpdateService CreateService(string? baseDirectory = null)
    {
        Mock<ISettingsService> settingsService = new();
        _ = settingsService.Setup(s => s.LoadAppSettingsAsync()).ReturnsAsync(new AppSettings());

        return new UpdateService(
            TestHttpClientFactory.Create(UpdateService.HttpClientName, _handler),
            settingsService.Object,
            Mock.Of<ILogService>(),
            Path.Combine(_workDirectory, "etag.json"),
            new Version(0, 2, 0),
            baseDirectory ?? _workDirectory,
            _launcher);
    }
}
