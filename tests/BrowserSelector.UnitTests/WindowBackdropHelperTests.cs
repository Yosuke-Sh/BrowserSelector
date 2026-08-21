using BrowserSelector.Presentation.Helpers;
using FluentAssertions;

namespace BrowserSelector.UnitTests;

public class WindowBackdropHelperTests
{
    [Fact]
    public void OsBuildNumber_ReturnsPositiveValue()
    {
        // CI・実行環境を問わず、実際のOSビルド番号を返すこと（決め打ち値でないこと）の回帰確認
        int build = WindowBackdropHelper.OsBuildNumber;

        _ = build.Should().BePositive();
    }

    [Theory]
    [InlineData(true, true)] // ハイコントラスト時は常にフォールバック
    [InlineData(true, false)]
    [InlineData(false, false)] // ガラス効果オフ設定時もフォールバック
    public void ShouldUseOpaqueFallback_ReturnsTrue_WhenHighContrastOrGlassDisabled(bool isHighContrast, bool glassEffectEnabled)
    {
        bool result = WindowBackdropHelper.ShouldUseOpaqueFallback(isHighContrast, glassEffectEnabled);

        _ = result.Should().BeTrue();
    }

    [Fact]
    public void ShouldUseOpaqueFallback_ReturnsFalse_WhenNotHighContrastAndGlassEnabled()
    {
        bool result = WindowBackdropHelper.ShouldUseOpaqueFallback(isHighContrast: false, glassEffectEnabled: true);

        _ = result.Should().BeFalse();
    }

    [Theory]
    [InlineData(22621, WindowBackdropHelper.DwmBackdropSupport.SystemBackdropType)] // Windows 11 22H2 ちょうど
    [InlineData(23000, WindowBackdropHelper.DwmBackdropSupport.SystemBackdropType)] // それ以降
    [InlineData(22000, WindowBackdropHelper.DwmBackdropSupport.MicaEffectOnly)] // Windows 11 21H2 ちょうど
    [InlineData(22400, WindowBackdropHelper.DwmBackdropSupport.MicaEffectOnly)] // 21H2〜22H2未満
    [InlineData(19045, WindowBackdropHelper.DwmBackdropSupport.Unsupported)] // Windows 10
    [InlineData(21999, WindowBackdropHelper.DwmBackdropSupport.Unsupported)] // 22H2未満の境界直下
    public void ResolveBackdropSupport_ReturnsExpectedSupportLevel(int osBuild, WindowBackdropHelper.DwmBackdropSupport expected)
    {
        WindowBackdropHelper.DwmBackdropSupport result = WindowBackdropHelper.ResolveBackdropSupport(osBuild);

        _ = result.Should().Be(expected);
    }

    [Fact]
    public void Apply_WithNullWindow_ThrowsArgumentNullException()
    {
        Action act = () => WindowBackdropHelper.Apply(null!, WindowBackdropHelper.BackdropKind.Mica, isDarkMode: false, glassEffectEnabled: true);

        _ = act.Should().Throw<ArgumentNullException>();
    }

    // DWMWA_WINDOW_CORNER_PREFERENCEの値（WindowBackdropHelper内はprivate定数のためテスト側で直接値を持つ）。
    // DoNotRound=1, Round=2, RoundSmall=3.
    [Theory]
    [InlineData(0, 1)] // 角丸なし
    [InlineData(-5, 1)] // 負値も角丸なし側にフォールバック
    [InlineData(0.5, 3)] // 0<x<8 は小さめの角丸
    [InlineData(1, 3)]
    [InlineData(7, 3)]
    [InlineData(7.9, 3)]
    [InlineData(8, 2)] // 8px以上は通常の角丸
    [InlineData(20, 2)]
    public void ResolveCornerPreference_ReturnsExpectedDwmValue(double cornerRadiusPreference, int expected)
    {
        int result = WindowBackdropHelper.ResolveCornerPreference(cornerRadiusPreference);

        result.Should().Be(expected);
    }
}
