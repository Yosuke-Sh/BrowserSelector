// <copyright file="IIconCacheService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using System.Windows.Media.Imaging;

namespace BrowserSelector.Core.Services;

/// <summary>
/// 実行ファイル・画像ファイルからアイコンを取得しキャッシュするサービスのインターフェース.
/// </summary>
public interface IIconCacheService
{
    /// <summary>
    /// 指定されたファイルパスとアイコンインデックス、表示サイズに対応するアイコンを取得します.
    /// メモリキャッシュ・ディスクキャッシュを優先的に参照し、無ければ抽出してキャッシュに格納します.
    /// </summary>
    /// <param name="filePath">実行ファイルまたは画像ファイルのパス.</param>
    /// <param name="iconIndex">アイコンインデックス（実行ファイル内の何番目のアイコンか）.</param>
    /// <param name="size">表示目標のアイコンサイズ（ピクセル、正方形）.</param>
    /// <returns>取得したアイコンのビットマップ。取得に失敗した場合は null.</returns>
    BitmapSource? GetIcon(string filePath, int iconIndex, int size);

    /// <summary>
    /// メモリキャッシュを全て破棄します（ディスクキャッシュは保持されます）.
    /// </summary>
    void ClearMemoryCache();
}
