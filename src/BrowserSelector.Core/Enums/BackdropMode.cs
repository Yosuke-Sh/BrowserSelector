// <copyright file="BackdropMode.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Core.Enums;

/// <summary>
/// ウィンドウ背景の描画方式を表す列挙型（Phase E-1: 外観タブ）.
/// DWMバックドロップに対応する3種（Mica/Acrylic/MicaAlt）に加え、
/// DWM非対応環境向けの半透明単色と、アクセシビリティ用の完全不透明を含む.
/// </summary>
public enum BackdropMode
{
    /// <summary>Mica（既定の不透明多層ブラー）.</summary>
    Mica,

    /// <summary>Acrylic（半透明・強めのブラー）.</summary>
    Acrylic,

    /// <summary>MicaAlt（タブ付きウィンドウ向けの濃いMica）.</summary>
    MicaAlt,

    /// <summary>半透明単色ブラシ（DWM非対応環境向け）.</summary>
    SolidTranslucent,

    /// <summary>完全不透明（ハイコントラスト・低スペック環境向け）.</summary>
    Opaque,
}
