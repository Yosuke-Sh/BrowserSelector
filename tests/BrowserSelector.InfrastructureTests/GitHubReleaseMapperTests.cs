using System.Text.Json;
using BrowserSelector.Core.Models;
using BrowserSelector.Infrastructure.Updates;
using FluentAssertions;

namespace BrowserSelector.InfrastructureTests;

/// <summary>
/// Phase H-2: GitHub APIレスポンス → <see cref="UpdateInfo"/> の変換テスト.
/// v0.2.0まではこの変換層が無く、GitHubのJSONをUpdateInfoへ直接デシリアライズしていたため
/// Versionが常に空文字となり更新が原理的に検出されなかった。その回帰を防ぐのが主目的.
/// </summary>
public class GitHubReleaseMapperTests
{
    private const string InstallerUrl = "https://github.com/Yosuke-Sh/BrowserSelector/releases/download/v0.3.0/BrowserSelector-Setup-v0.3.0.exe";
    private const string PortableUrl = "https://github.com/Yosuke-Sh/BrowserSelector/releases/download/v0.3.0/BrowserSelector-v0.3.0-win-x64.zip";
    private const string ChecksumsUrl = "https://github.com/Yosuke-Sh/BrowserSelector/releases/download/v0.3.0/SHA256SUMS.txt";

    [Theory]
    [InlineData("v0.3.0", 0, 3, 0)]
    [InlineData("V0.3.0", 0, 3, 0)]
    [InlineData("0.3.0", 0, 3, 0)]
    [InlineData("v1.2.3", 1, 2, 3)]
    public void TryParseVersion_ShouldStripLeadingV(string tag, int major, int minor, int build)
    {
        GitHubReleaseMapper.TryParseVersion(tag, out Version? version).Should().BeTrue();
        version.Should().Be(new Version(major, minor, build));
    }

    [Theory]
    [InlineData("v0.3.0-beta1")]
    [InlineData("v0.3.0-rc.1")]
    [InlineData("v0.3.0+build42")]
    public void TryParseVersion_ShouldTruncatePrereleaseAndBuildMetadata(string tag)
    {
        // System.Versionは "-beta1" を解釈できないため、数値部分のみを比較対象にする。
        GitHubReleaseMapper.TryParseVersion(tag, out Version? version).Should().BeTrue();
        version.Should().Be(new Version(0, 3, 0));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("v")]
    [InlineData("latest")]
    [InlineData("vNext")]
    public void TryParseVersion_ShouldFailForUnparsableTags(string? tag)
    {
        GitHubReleaseMapper.TryParseVersion(tag, out Version? version).Should().BeFalse();
        version.Should().BeNull();
    }

    [Fact]
    public void TryMap_ShouldMapAllCoreFields()
    {
        GitHubRelease release = Deserialize($$"""
        {
          "tag_name": "v0.3.0",
          "name": "BrowserSelector v0.3.0",
          "body": "## Added\n- auto update",
          "draft": false,
          "prerelease": false,
          "published_at": "2026-08-16T12:00:00Z",
          "html_url": "https://github.com/Yosuke-Sh/BrowserSelector/releases/tag/v0.3.0",
          "assets": []
        }
        """);

        GitHubReleaseMapper.TryMap(release, out UpdateInfo? info).Should().BeTrue();
        info!.Version.Should().Be(new Version(0, 3, 0));
        info.TagName.Should().Be("v0.3.0");
        info.ReleaseNotes.Should().Contain("auto update");
        info.ReleasePageUrl.Should().Be("https://github.com/Yosuke-Sh/BrowserSelector/releases/tag/v0.3.0");
        info.PublishedAt.Should().Be(DateTimeOffset.Parse("2026-08-16T12:00:00Z", System.Globalization.CultureInfo.InvariantCulture));
        info.IsPrerelease.Should().BeFalse();
    }

    [Fact]
    public void TryMap_ShouldResolveAllThreeAssets()
    {
        GitHubRelease release = Deserialize($$"""
        {
          "tag_name": "v0.3.0",
          "assets": [
            { "name": "BrowserSelector-Setup-v0.3.0.exe", "browser_download_url": "{{InstallerUrl}}", "size": 4321 },
            { "name": "BrowserSelector-v0.3.0-win-x64.zip", "browser_download_url": "{{PortableUrl}}", "size": 8765 },
            { "name": "SHA256SUMS.txt", "browser_download_url": "{{ChecksumsUrl}}", "size": 200 }
          ]
        }
        """);

        GitHubReleaseMapper.TryMap(release, out UpdateInfo? info).Should().BeTrue();

        info!.InstallerAsset.Should().NotBeNull();
        info.InstallerAsset!.Name.Should().Be("BrowserSelector-Setup-v0.3.0.exe");
        info.InstallerAsset.Size.Should().Be(4321);

        info.PortableAsset.Should().NotBeNull();
        info.PortableAsset!.Name.Should().Be("BrowserSelector-v0.3.0-win-x64.zip");

        info.ChecksumsAsset.Should().NotBeNull();
        info.ChecksumsAsset!.Name.Should().Be("SHA256SUMS.txt");
    }

    [Fact]
    public void TryMap_ShouldLeaveMissingAssetsNull()
    {
        GitHubRelease release = Deserialize($$"""
        {
          "tag_name": "v0.3.0",
          "assets": [
            { "name": "BrowserSelector-v0.3.0-win-x64.zip", "browser_download_url": "{{PortableUrl}}", "size": 1 }
          ]
        }
        """);

        GitHubReleaseMapper.TryMap(release, out UpdateInfo? info).Should().BeTrue();
        info!.PortableAsset.Should().NotBeNull();
        info.InstallerAsset.Should().BeNull();
        info.ChecksumsAsset.Should().BeNull();
    }

    [Fact]
    public void TryMap_ShouldHandleNullAssetsArray()
    {
        GitHubRelease release = Deserialize("""{ "tag_name": "v0.3.0" }""");

        GitHubReleaseMapper.TryMap(release, out UpdateInfo? info).Should().BeTrue();
        info!.InstallerAsset.Should().BeNull();
        info.PortableAsset.Should().BeNull();
        info.ChecksumsAsset.Should().BeNull();
    }

    [Fact]
    public void TryMap_ShouldExcludeDraftRelease()
    {
        GitHubRelease release = Deserialize("""{ "tag_name": "v0.3.0", "draft": true }""");

        GitHubReleaseMapper.TryMap(release, out UpdateInfo? info).Should().BeFalse();
        info.Should().BeNull();
    }

    [Fact]
    public void TryMap_ShouldPreservePrereleaseFlag()
    {
        // フィルタ自体はUpdateService側の責務。ここではフラグが失われないことだけを保証する。
        GitHubRelease release = Deserialize("""{ "tag_name": "v0.3.0-beta1", "prerelease": true }""");

        GitHubReleaseMapper.TryMap(release, out UpdateInfo? info).Should().BeTrue();
        info!.IsPrerelease.Should().BeTrue();
        info.Version.Should().Be(new Version(0, 3, 0));
    }

    [Fact]
    public void TryMap_ShouldFailWhenTagNameIsUnparsable()
    {
        GitHubRelease release = Deserialize("""{ "tag_name": "nightly" }""");

        GitHubReleaseMapper.TryMap(release, out UpdateInfo? info).Should().BeFalse();
        info.Should().BeNull();
    }

    [Fact]
    public void TryMap_ShouldReturnFalseForNullRelease()
    {
        GitHubReleaseMapper.TryMap(null, out UpdateInfo? info).Should().BeFalse();
        info.Should().BeNull();
    }

    [Theory]
    [InlineData("http://github.com/x/y/releases/download/v0.3.0/BrowserSelector-Setup-v0.3.0.exe")]
    [InlineData("https://evil.com/BrowserSelector-Setup-v0.3.0.exe")]
    [InlineData("https://evil-githubusercontent.com/BrowserSelector-Setup-v0.3.0.exe")]
    [InlineData("not-a-url")]
    public void TryMap_ShouldDropAssetWithUntrustedDownloadUrl(string url)
    {
        GitHubRelease release = Deserialize($$"""
        {
          "tag_name": "v0.3.0",
          "assets": [
            { "name": "BrowserSelector-Setup-v0.3.0.exe", "browser_download_url": "{{url}}", "size": 1 }
          ]
        }
        """);

        GitHubReleaseMapper.TryMap(release, out UpdateInfo? info).Should().BeTrue();

        // 署名が無い以上、信頼できないホストのアセットは「無かったもの」として扱う。
        info!.InstallerAsset.Should().BeNull();
    }

    [Fact]
    public void TryMap_ShouldClearReleasePageUrlWhenUntrusted()
    {
        GitHubRelease release = Deserialize("""
        { "tag_name": "v0.3.0", "html_url": "https://evil.com/releases/tag/v0.3.0" }
        """);

        GitHubReleaseMapper.TryMap(release, out UpdateInfo? info).Should().BeTrue();
        info!.ReleasePageUrl.Should().BeEmpty();
    }

    [Fact]
    public void TryMap_ShouldAcceptGithubUserContentAssetHost()
    {
        // GitHubはアセット配信をobjects.githubusercontent.comへリダイレクトすることがある。
        GitHubRelease release = Deserialize("""
        {
          "tag_name": "v0.3.0",
          "assets": [
            { "name": "SHA256SUMS.txt", "browser_download_url": "https://objects.githubusercontent.com/foo/SHA256SUMS.txt", "size": 1 }
          ]
        }
        """);

        GitHubReleaseMapper.TryMap(release, out UpdateInfo? info).Should().BeTrue();
        info!.ChecksumsAsset.Should().NotBeNull();
    }

    [Fact]
    public void TryMap_ShouldIgnoreUnrelatedAssetNames()
    {
        GitHubRelease release = Deserialize($$"""
        {
          "tag_name": "v0.3.0",
          "assets": [
            { "name": "README.md", "browser_download_url": "{{ChecksumsUrl}}", "size": 1 },
            { "name": "BrowserSelector-Setup-v0.3.0.exe.sig", "browser_download_url": "{{InstallerUrl}}", "size": 1 }
          ]
        }
        """);

        GitHubReleaseMapper.TryMap(release, out UpdateInfo? info).Should().BeTrue();
        info!.InstallerAsset.Should().BeNull();
        info.PortableAsset.Should().BeNull();
        info.ChecksumsAsset.Should().BeNull();
    }

    [Fact]
    public void TryMap_ShouldDefaultMissingStringFieldsToEmpty()
    {
        GitHubRelease release = Deserialize("""{ "tag_name": "v0.3.0" }""");

        GitHubReleaseMapper.TryMap(release, out UpdateInfo? info).Should().BeTrue();
        info!.ReleaseNotes.Should().BeEmpty();
        info.ReleasePageUrl.Should().BeEmpty();
        info.PublishedAt.Should().BeNull();
    }

    private static GitHubRelease Deserialize(string json) =>
        JsonSerializer.Deserialize<GitHubRelease>(json)!;
}
