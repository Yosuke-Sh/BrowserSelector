using BrowserSelector.Core.Models;
using FluentAssertions;

namespace BrowserSelector.CoreTests;

/// <summary>
/// Phase H-1で再設計した<see cref="UpdateInfo"/>および関連型のテスト.
/// v0.2.0までの18プロパティ版は、GitHub Releasesに対応物が無いプロパティが永久に既定値のままだった。
/// ここでは新モデルの既定値と、失敗理由を区別できる結果型の振る舞いを固定する.
/// </summary>
public class UpdateInfoTests
{
    [Fact]
    public void UpdateInfo_Defaults_ShouldBeEmptyAndNotDownloaded()
    {
        var info = new UpdateInfo();

        info.Version.Should().Be(new Version(0, 0, 0));
        info.TagName.Should().BeEmpty();
        info.ReleaseNotes.Should().BeEmpty();
        info.ReleasePageUrl.Should().BeEmpty();
        info.PublishedAt.Should().BeNull();
        info.IsPrerelease.Should().BeFalse();
        info.InstallerAsset.Should().BeNull();
        info.PortableAsset.Should().BeNull();
        info.ChecksumsAsset.Should().BeNull();
        info.LocalFilePath.Should().BeNull();
        info.IsDownloaded.Should().BeFalse();
    }

    [Fact]
    public void UpdateAsset_ShouldExposeConstructorValuesAndNullChecksum()
    {
        var asset = new UpdateAsset("BrowserSelector-Setup-v0.3.0.exe", new Uri("https://github.com/x/y/releases/download/v0.3.0/a.exe"), 1234);

        asset.Name.Should().Be("BrowserSelector-Setup-v0.3.0.exe");
        asset.DownloadUrl.Host.Should().Be("github.com");
        asset.Size.Should().Be(1234);

        // SHA256はSHA256SUMS.txtを解析するまで未解決（H-4で設定される）。
        asset.Sha256.Should().BeNull();
    }

    [Fact]
    public void UpdateAsset_ShouldSupportValueEquality()
    {
        var url = new Uri("https://github.com/x/y/releases/download/v0.3.0/a.exe");
        var a = new UpdateAsset("a.exe", url, 10) { Sha256 = "abc" };
        var b = new UpdateAsset("a.exe", url, 10) { Sha256 = "abc" };

        a.Should().Be(b);
    }

    [Fact]
    public void UpdateDownloadResult_Succeeded_ShouldCarryPathAndNoFailure()
    {
        UpdateDownloadResult result = UpdateDownloadResult.Succeeded(@"C:\temp\a.exe");

        result.Success.Should().BeTrue();
        result.FilePath.Should().Be(@"C:\temp\a.exe");
        result.Failure.Should().Be(UpdateDownloadFailure.None);
    }

    [Theory]
    [InlineData(UpdateDownloadFailure.Network)]
    [InlineData(UpdateDownloadFailure.ChecksumMismatch)]
    [InlineData(UpdateDownloadFailure.ChecksumUnavailable)]
    [InlineData(UpdateDownloadFailure.Canceled)]
    [InlineData(UpdateDownloadFailure.Io)]
    public void UpdateDownloadResult_Failed_ShouldPreserveFailureReason(UpdateDownloadFailure failure)
    {
        // UIが「チェックサム不一致（危険）」と「ネットワーク断（無害）」を区別できることが
        // 結果型を導入した目的なので、理由が保持されることを明示的に固定する。
        UpdateDownloadResult result = UpdateDownloadResult.Failed(failure);

        result.Success.Should().BeFalse();
        result.FilePath.Should().BeNull();
        result.Failure.Should().Be(failure);
    }

    [Fact]
    public void UpdateDownloadResult_Canceled_ShouldMapToCanceledFailure()
    {
        UpdateDownloadResult.Canceled().Failure.Should().Be(UpdateDownloadFailure.Canceled);
    }

    [Fact]
    public void UpdateChannel_ShouldDefineInstallerAndPortableOnly()
    {
        Enum.GetValues<UpdateChannel>().Should().BeEquivalentTo(
            new[] { UpdateChannel.Installer, UpdateChannel.Portable });
    }

    [Fact]
    public void UpdateAvailableEventArgs_ShouldExposeUpdateInfo()
    {
        var info = new UpdateInfo { TagName = "v0.3.0" };

        new UpdateAvailableEventArgs(info).UpdateInfo.Should().BeSameAs(info);
    }
}
