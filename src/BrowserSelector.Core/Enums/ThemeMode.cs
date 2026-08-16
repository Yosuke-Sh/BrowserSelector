// <copyright file="ThemeMode.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
namespace BrowserSelector.Core.Enums;

/// <summary>
/// アプリの外観テーマを表す列挙型.
/// </summary>
public enum ThemeMode
{
    /// <summary>
    /// ライトテーマ固定.
    /// </summary>
    Light,

    /// <summary>
    /// ダークテーマ固定.
    /// </summary>
    Dark,

    /// <summary>
    /// OSの設定に追従.
    /// </summary>
    System,
}
