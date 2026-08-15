using System.Windows.Input;
using BrowserSelector.Presentation.Helpers;
using FluentAssertions;

namespace BrowserSelector.UnitTests;

public class HotkeyResolverTests
{
    [Theory]
    [InlineData(Key.D1, '1')]
    [InlineData(Key.D5, '5')] // BrowserChooser3の教訓: Key.ToString()は"D5"になるが、正規化後は'5'
    [InlineData(Key.D9, '9')]
    [InlineData(Key.NumPad5, '5')]
    [InlineData(Key.A, 'A')]
    [InlineData(Key.Z, 'Z')]
    public void Resolve_ValidHotkeyKey_ReturnsNormalizedChar(Key key, char expected)
    {
        char? result = HotkeyResolver.Resolve(key, ModifierKeys.None);

        _ = result.Should().Be(expected);
    }

    [Fact]
    public void Resolve_D0_ReturnsNull()
    {
        // '0'キーはホットキーバッジ範囲外（1-9のみ）
        char? result = HotkeyResolver.Resolve(Key.D0, ModifierKeys.None);

        _ = result.Should().BeNull();
    }

    [Theory]
    [InlineData(ModifierKeys.Control)]
    [InlineData(ModifierKeys.Alt)]
    public void Resolve_WithCtrlOrAltModifier_ReturnsNull(ModifierKeys modifiers)
    {
        char? result = HotkeyResolver.Resolve(Key.D5, modifiers);

        _ = result.Should().BeNull();
    }

    [Fact]
    public void Resolve_WithShiftModifier_StillResolves()
    {
        char? result = HotkeyResolver.Resolve(Key.D5, ModifierKeys.Shift);

        _ = result.Should().Be('5');
    }

    [Fact]
    public void Resolve_NonHotkeyKey_ReturnsNull()
    {
        char? result = HotkeyResolver.Resolve(Key.F1, ModifierKeys.None);

        _ = result.Should().BeNull();
    }

    [Fact]
    public void GetBadgeForIndex_FirstNineIndices_ReturnDigits()
    {
        for (int i = 0; i < 9; i++)
        {
            char? badge = HotkeyResolver.GetBadgeForIndex(i);
            _ = badge.Should().Be((char)('1' + i));
        }
    }

    [Fact]
    public void GetBadgeForIndex_TenthIndex_ReturnsA()
    {
        char? badge = HotkeyResolver.GetBadgeForIndex(9);

        _ = badge.Should().Be('A');
    }

    [Fact]
    public void GetBadgeForIndex_LastValidIndex_ReturnsZ()
    {
        char? badge = HotkeyResolver.GetBadgeForIndex(34);

        _ = badge.Should().Be('Z');
    }

    [Fact]
    public void GetBadgeForIndex_OutOfRange_ReturnsNull()
    {
        char? outOfRange = HotkeyResolver.GetBadgeForIndex(35);

        _ = outOfRange.Should().BeNull();
    }

    [Fact]
    public void GetBadgeForIndex_NegativeIndex_ReturnsNull()
    {
        char? badge = HotkeyResolver.GetBadgeForIndex(-1);

        _ = badge.Should().BeNull();
    }

    [Fact]
    public void BadgeSequence_HasThirtyFiveEntries()
    {
        // '1'-'9'(9個) + 'A'-'Z'(26個) = 35個
        _ = HotkeyResolver.BadgeSequence.Should().HaveCount(35);
        _ = HotkeyResolver.BadgeSequence[0].Should().Be('1');
        _ = HotkeyResolver.BadgeSequence[8].Should().Be('9');
        _ = HotkeyResolver.BadgeSequence[9].Should().Be('A');
        _ = HotkeyResolver.BadgeSequence[34].Should().Be('Z');
    }
}
