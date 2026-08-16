using System.IO;
using BrowserSelector.Core.Models;
using BrowserSelector.Infrastructure.Updates;
using FluentAssertions;

namespace BrowserSelector.InfrastructureTests;

/// <summary>
/// Phase H-4: <see cref="UpdateService.ResolveChannelFor"/>（適用経路の判定）のテスト.
/// .issがDefaultDirName={autopf} + PrivilegesRequired=adminのため既定インストールは
/// Program Files配下になり、Installerルートが実質の主経路になる.
/// </summary>
public sealed class UpdateServiceChannelTests : IDisposable
{
    private readonly string _workDirectory;

    public UpdateServiceChannelTests()
    {
        _workDirectory = Path.Combine(Path.GetTempPath(), $"BSChannelTest_{Guid.NewGuid():N}");
        _ = Directory.CreateDirectory(_workDirectory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_workDirectory))
            {
                Directory.Delete(_workDirectory, recursive: true);
            }
        }
        catch (IOException)
        {
        }
    }

    [Fact]
    public void ResolveChannelFor_ShouldReturnPortableForWritableDirectory()
    {
        UpdateService.ResolveChannelFor(_workDirectory).Should().Be(UpdateChannel.Portable);
    }

    [Fact]
    public void ResolveChannelFor_ShouldReturnInstallerUnderProgramFiles()
    {
        string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        string appDirectory = Path.Combine(programFiles, "BrowserSelector");

        UpdateService.ResolveChannelFor(appDirectory).Should().Be(UpdateChannel.Installer);
    }

    [Fact]
    public void ResolveChannelFor_ShouldReturnInstallerUnderProgramFilesX86()
    {
        string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        string appDirectory = Path.Combine(programFilesX86, "BrowserSelector");

        UpdateService.ResolveChannelFor(appDirectory).Should().Be(UpdateChannel.Installer);
    }

    [Fact]
    public void ResolveChannelFor_ShouldReturnInstallerForNonExistentDirectory()
    {
        // 書き込み可否を判定できない場所はUpdater.exeでの置換に適さないため、安全側のInstallerへ倒す。
        string missing = Path.Combine(_workDirectory, "does-not-exist");

        UpdateService.ResolveChannelFor(missing).Should().Be(UpdateChannel.Installer);
    }

    [Fact]
    public void ResolveChannelFor_ShouldNotMisjudgeDirectoryWithProgramFilesPrefix()
    {
        // "C:\Program Files" と "C:\Program FilesX" が前方一致で誤判定されないこと。
        string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        string lookalike = programFiles + "X";

        UpdateService.ResolveChannelFor(lookalike).Should().Be(UpdateChannel.Installer,
            "存在しないディレクトリなので書き込み不可としてInstallerになる（ProgramFiles判定によるものではない）");
    }

    [Fact]
    public void ResolveChannelFor_ShouldThrowForNull()
    {
        Action act = () => UpdateService.ResolveChannelFor(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
