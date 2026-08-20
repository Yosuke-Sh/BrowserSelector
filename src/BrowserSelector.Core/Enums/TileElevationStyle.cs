// <copyright file="TileElevationStyle.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Core.Enums;

/// <summary>
/// ブラウザタイルの立体表現（3D風エレベーション）の方式を表す列挙型.
/// タイル背景色に応じて自動生成される影色を使う方式・ベベル・枠線から選択できる.
/// </summary>
public enum TileElevationStyle
{
    /// <summary>立体表現なし（フラット）.</summary>
    None,

    /// <summary>オフセット影による立体表現（既定）.</summary>
    Shadow,

    /// <summary>上辺を明るく・下辺を暗くするベベルによる立体表現.</summary>
    Bevel,

    /// <summary>外側の縁取りによる浮き上がり表現.</summary>
    Outline,
}
