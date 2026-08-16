using System.ComponentModel;
using BrowserSelector.Core.Models;
using FluentAssertions;

namespace BrowserSelector.CoreTests;

/// <summary>
/// Phase H-1で<see cref="AppSettings"/>へ追加した4プロパティのテスト.
/// SettingsServiceはAppSettingsを丸ごとシリアライズするため、既定値が正しくないと
/// 既存settings.jsonを読んだ際に意図しない挙動（例: 常にプレリリースを拾う）になる.
/// </summary>
public class AppSettingsUpdateTests
{
    [Fact]
    public void LastUpdateCheckUtc_DefaultShouldBeNull()
    {
        // nullは「一度もチェックしていない」を意味し、初回起動時に即チェックさせるための値。
        new AppSettings().LastUpdateCheckUtc.Should().BeNull();
    }

    [Fact]
    public void SkippedUpdateVersion_DefaultShouldBeEmpty()
    {
        new AppSettings().SkippedUpdateVersion.Should().BeEmpty();
    }

    [Fact]
    public void IncludePrereleases_DefaultShouldBeFalse()
    {
        // 既定でプレリリースを配らないこと。ここがtrueだと一般ユーザーにbetaが降ってしまう。
        new AppSettings().IncludePrereleases.Should().BeFalse();
    }

    [Fact]
    public void UpdatePendingOnNextLaunch_DefaultShouldBeFalse()
    {
        new AppSettings().UpdatePendingOnNextLaunch.Should().BeFalse();
    }

    [Fact]
    public void UpdateProperties_ShouldRaisePropertyChanged()
    {
        var settings = new AppSettings();
        var changed = new List<string?>();
        ((INotifyPropertyChanged)settings).PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        settings.LastUpdateCheckUtc = DateTimeOffset.UtcNow;
        settings.SkippedUpdateVersion = "0.3.0";
        settings.IncludePrereleases = true;
        settings.UpdatePendingOnNextLaunch = true;

        changed.Should().Contain(nameof(AppSettings.LastUpdateCheckUtc))
            .And.Contain(nameof(AppSettings.SkippedUpdateVersion))
            .And.Contain(nameof(AppSettings.IncludePrereleases))
            .And.Contain(nameof(AppSettings.UpdatePendingOnNextLaunch));
    }

    [Fact]
    public void ExistingUpdateSettings_ShouldKeepPreviousDefaults()
    {
        // v0.2.0以前から存在する2項目。H-1の追加でここが変わっていないことを保証する。
        var settings = new AppSettings();

        settings.CheckForUpdates.Should().BeTrue();
        settings.UpdateCheckInterval.Should().Be(24);
    }
}
