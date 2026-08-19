using BrowserSelector.Presentation.Helpers;
using FluentAssertions;

namespace BrowserSelector.UnitTests;

public class TileLayoutHelperTests
{
    [Theory]
    [InlineData(500, 120, 10, 4)]
    [InlineData(1000, 120, 3, 3)] // 列数がアイテム数を超えない
    [InlineData(120, 120, 10, 1)]
    public void CalculateColumns_ReturnsExpectedColumnCount(double availableWidth, double tileWidth, int itemCount, int expected)
    {
        int columns = TileLayoutHelper.CalculateColumns(availableWidth, tileWidth, itemCount);

        _ = columns.Should().Be(expected);
    }

    [Fact]
    public void CalculateColumns_WithZeroItemCount_ReturnsOne()
    {
        int columns = TileLayoutHelper.CalculateColumns(500, 120, 0);

        _ = columns.Should().Be(1);
    }

    [Fact]
    public void CalculateColumns_WithCustomButtonWidthPlusMarginTotal_MatchesExpectedTileCount()
    {
        // VisualSettings.BrowserButtonWidthを変更した場合の実効タイル幅
        // （TileMarginTotal込み）で列数計算が正しく行われることを確認する。
        // 従来はDefaultTileWidth(120px)固定だったため、ボタン幅を変えると
        // 列数計算とタイル実サイズが食い違いタイルが重なる不具合があった。
        double customButtonWidth = 200.0;
        double effectiveTileWidth = customButtonWidth + TileLayoutHelper.TileMarginTotal; // 216px

        int columns = TileLayoutHelper.CalculateColumns(1000, effectiveTileWidth, 10);

        _ = columns.Should().Be(4); // 1000 / 216 = 4.63 -> 4列
    }

    [Theory]
    [InlineData(0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void CalculateColumns_WithInvalidAvailableWidth_FallsBackToItemCount(double availableWidth)
    {
        int columns = TileLayoutHelper.CalculateColumns(availableWidth, 120, 5);

        _ = columns.Should().Be(5);
    }

    [Fact]
    public void MoveIndex_Right_MovesToNextColumn()
    {
        int newIndex = TileLayoutHelper.MoveIndex(0, 6, 3, TileNavigationDirection.Right);

        _ = newIndex.Should().Be(1);
    }

    [Fact]
    public void MoveIndex_Right_AtRowEnd_WrapsToNextRow()
    {
        // 3列グリッドで index=2（1行目末尾）からRight移動 -> index=3（2行目先頭）へ回り込み
        int newIndex = TileLayoutHelper.MoveIndex(2, 6, 3, TileNavigationDirection.Right);

        _ = newIndex.Should().Be(3);
    }

    [Fact]
    public void MoveIndex_Right_AtLastItem_WrapsToFirstRow()
    {
        // 3列6アイテムの末尾(index=5)からRight移動 -> 先頭行(index=0)へ回り込み
        int newIndex = TileLayoutHelper.MoveIndex(5, 6, 3, TileNavigationDirection.Right);

        _ = newIndex.Should().Be(0);
    }

    [Fact]
    public void MoveIndex_Left_AtRowStart_WrapsToPreviousRow()
    {
        // 3列グリッドで index=3（2行目先頭）からLeft移動 -> index=2（1行目末尾）へ回り込み
        int newIndex = TileLayoutHelper.MoveIndex(3, 6, 3, TileNavigationDirection.Left);

        _ = newIndex.Should().Be(2);
    }

    [Fact]
    public void MoveIndex_Left_AtFirstItem_WrapsToLastItem()
    {
        int newIndex = TileLayoutHelper.MoveIndex(0, 6, 3, TileNavigationDirection.Left);

        _ = newIndex.Should().Be(5);
    }

    [Fact]
    public void MoveIndex_Down_MovesToNextRowSameColumn()
    {
        int newIndex = TileLayoutHelper.MoveIndex(0, 6, 3, TileNavigationDirection.Down);

        _ = newIndex.Should().Be(3);
    }

    [Fact]
    public void MoveIndex_Down_AtLastRow_WrapsToFirstRow()
    {
        int newIndex = TileLayoutHelper.MoveIndex(3, 6, 3, TileNavigationDirection.Down);

        _ = newIndex.Should().Be(0);
    }

    [Fact]
    public void MoveIndex_Up_AtFirstRow_WrapsToLastRow()
    {
        int newIndex = TileLayoutHelper.MoveIndex(0, 6, 3, TileNavigationDirection.Up);

        _ = newIndex.Should().Be(3);
    }

    [Fact]
    public void MoveIndex_WithZeroItemCount_ReturnsNegativeOne()
    {
        int newIndex = TileLayoutHelper.MoveIndex(0, 0, 3, TileNavigationDirection.Right);

        _ = newIndex.Should().Be(-1);
    }

    [Fact]
    public void MoveIndex_WithIncompleteLastRow_HandlesWrapCorrectly()
    {
        // 3列、5アイテム（最終行は2個だけ）。index=4（最終行末尾）からRight -> index=0へ
        int newIndex = TileLayoutHelper.MoveIndex(4, 5, 3, TileNavigationDirection.Right);

        _ = newIndex.Should().Be(0);
    }
}
