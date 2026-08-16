using BrowserSelector.Core;
using FluentAssertions;

namespace BrowserSelector.CoreTests;

/// <summary>
/// <see cref="AppInfo"/> のテストクラス（Phase E-2b/E-2c）.
/// 検証ゲート: <see cref="AppInfo.CurrentVersion"/> が決め打ちの既定値 1.0.0 を返さないこと、
/// リポジトリURL群が正しく組み立てられることを確認する.
/// </summary>
public class AppInfoTests
{
    [Fact]
    public void CurrentVersion_ShouldNotBeDefaultOneDotZero()
    {
        // Phase E-2b: Directory.Build.propsでVersionを一元管理する前は、
        // App.csprojにバージョンメタデータが無くAssembly.GetName().Versionが既定値1.0.0.0を返していた。
        // これがUpdateServiceのバージョン決め打ちの根本原因だった（Phase Hの更新判定が永久に走らない不具合）。
        AppInfo.CurrentVersion.Should().NotBe(new Version(1, 0, 0, 0));
        AppInfo.CurrentVersion.Should().NotBe(new Version(0, 0, 0, 0));
    }

    [Fact]
    public void CurrentVersion_ShouldNotBeNull()
    {
        AppInfo.CurrentVersion.Should().NotBeNull();
    }

    [Fact]
    public void RepositoryUrl_ShouldPointToConfiguredOwnerAndRepo()
    {
        AppInfo.RepositoryUrl.Should().Be($"https://github.com/{AppInfo.RepositoryOwner}/{AppInfo.RepositoryName}");
    }

    [Fact]
    public void IssuesUrl_ShouldBeUnderRepositoryUrl()
    {
        AppInfo.IssuesUrl.Should().Be($"{AppInfo.RepositoryUrl}/issues");
    }

    [Fact]
    public void ReleasesUrl_ShouldBeUnderRepositoryUrl()
    {
        AppInfo.ReleasesUrl.Should().Be($"{AppInfo.RepositoryUrl}/releases");
    }

    [Fact]
    public void LatestReleaseApiUrl_ShouldPointToGitHubApi()
    {
        AppInfo.LatestReleaseApiUrl.Should().Be(
            $"https://api.github.com/repos/{AppInfo.RepositoryOwner}/{AppInfo.RepositoryName}/releases/latest");
    }

    [Fact]
    public void RepositoryOwner_ShouldNotBePlaceholder()
    {
        // ServiceCollectionExtensions.cs:47に元々あったプレースホルダ"your-repo"の再発防止
        AppInfo.RepositoryOwner.Should().NotBe("your-repo");
        AppInfo.RepositoryName.Should().NotBe("your-repo");
    }
}
