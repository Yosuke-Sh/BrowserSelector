// <copyright file="IThemeService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using BrowserSelector.Core.Enums;

namespace BrowserSelector.Core.Services;

/// <summary>
/// アプリの外観テーマ（ライト/ダーク/システム追従）を切り替えるサービスのインターフェース.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// 実際に適用されているテーマ（ライト/ダーク）が変化した際に発火するイベント.
    /// </summary>
    event EventHandler? ActiveThemeChanged;

    /// <summary>
    /// 現在適用されているテーマモード.
    /// </summary>
    ThemeMode CurrentMode { get; }

    /// <summary>
    /// 現在実際に描画に使われているテーマ（<see cref="ThemeMode.System"/> 選択時はOSの設定から解決した結果）.
    /// </summary>
    bool IsDarkThemeActive { get; }

    /// <summary>
    /// テーマモードを適用します.
    /// </summary>
    /// <param name="mode">適用するテーマモード.</param>
    void ApplyTheme(ThemeMode mode);
}
