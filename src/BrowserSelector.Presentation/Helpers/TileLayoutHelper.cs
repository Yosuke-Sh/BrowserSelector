// <copyright file="TileLayoutHelper.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Presentation.Helpers;

/// <summary>
/// ブラウザタイルの列数計算を行う唯一の計算元。
/// <see cref="System.Windows.Controls.Primitives.UniformGrid"/> のレイアウトと
/// 矢印キー移動（Phase C-4）の両方がこのメソッドを共有することで、
/// 表示上の列数とキーボード移動時の列数を常に一致させる（Phase C-3）.
/// </summary>
public static class TileLayoutHelper
{
    /// <summary>
    /// 既定のタイル幅（マージン込みの概算・px）.
    /// </summary>
    public const double DefaultTileWidth = 120.0;

    /// <summary>
    /// タイル1個あたりのマージン合計（<c>BrowserButtonStyle</c>の<c>Margin="8"</c>により左右で計16px）.
    /// <see cref="Core.Models.VisualSettings.BrowserButtonWidth"/>から実効タイル幅を算出する際に使用する.
    /// </summary>
    public const double TileMarginTotal = 16.0;

    /// <summary>
    /// 利用可能な幅とタイル1個あたりの幅からグリッドの列数を計算する.
    /// BrowserChooser3の <c>CalculateColumnsPerRow()</c> に倣い、最低1列を保証する.
    /// </summary>
    /// <param name="availableWidth">レイアウト可能な幅（px）.</param>
    /// <param name="tileWidth">タイル1個あたりの幅（px、マージン込み）.</param>
    /// <param name="itemCount">タイルの総数. 0以下の場合は1列を返す.</param>
    /// <returns>列数（1以上）.</returns>
    public static int CalculateColumns(double availableWidth, double tileWidth, int itemCount)
    {
        if (itemCount <= 0)
        {
            return 1;
        }

        if (double.IsNaN(availableWidth) || double.IsInfinity(availableWidth) || availableWidth <= 0 ||
            double.IsNaN(tileWidth) || tileWidth <= 0)
        {
            return Math.Max(1, itemCount);
        }

        int columns = (int)(availableWidth / tileWidth);
        columns = Math.Max(1, columns);

        // 列数がアイテム数を超える場合は、アイテム数を上限にする（UniformGridが無駄に幅を取らないように）。
        return Math.Min(columns, itemCount);
    }

    /// <summary>
    /// グリッド上のインデックスに対して、矢印キー移動後のインデックスを計算する（端で回り込み）.
    /// </summary>
    /// <param name="currentIndex">現在フォーカスされているインデックス（0始まり）.</param>
    /// <param name="itemCount">アイテム総数.</param>
    /// <param name="columns">列数（<see cref="CalculateColumns"/> の結果を使うこと）.</param>
    /// <param name="direction">移動方向.</param>
    /// <returns>移動後のインデックス. <paramref name="itemCount"/> が0の場合は-1.</returns>
    public static int MoveIndex(int currentIndex, int itemCount, int columns, TileNavigationDirection direction)
    {
        if (itemCount <= 0)
        {
            return -1;
        }

        columns = Math.Max(1, columns);
        currentIndex = Math.Clamp(currentIndex, 0, itemCount - 1);

        int row = currentIndex / columns;
        int col = currentIndex % columns;
        int rowCount = (int)Math.Ceiling(itemCount / (double)columns);

        switch (direction)
        {
            case TileNavigationDirection.Right:
                col++;
                if (col >= columns || (row * columns) + col >= itemCount)
                {
                    col = 0;
                    row = (row + 1) % rowCount;
                }

                break;
            case TileNavigationDirection.Left:
                col--;
                if (col < 0)
                {
                    row = row == 0 ? rowCount - 1 : row - 1;
                    int lastRowItemCount = itemCount - (row * columns);
                    col = Math.Min(columns, lastRowItemCount) - 1;
                }

                break;
            case TileNavigationDirection.Down:
                row++;
                if (row >= rowCount || (row * columns) + col >= itemCount)
                {
                    row = 0;
                }

                break;
            case TileNavigationDirection.Up:
                row--;
                if (row < 0)
                {
                    row = rowCount - 1;
                    if ((row * columns) + col >= itemCount)
                    {
                        row--;
                    }
                }

                break;
            default:
                return currentIndex;
        }

        int newIndex = (row * columns) + col;
        return Math.Clamp(newIndex, 0, itemCount - 1);
    }
}

/// <summary>
/// タイルグリッド上での移動方向.
/// </summary>
public enum TileNavigationDirection
{
    /// <summary>右へ移動.</summary>
    Right,

    /// <summary>左へ移動.</summary>
    Left,

    /// <summary>下へ移動.</summary>
    Down,

    /// <summary>上へ移動.</summary>
    Up,
}
