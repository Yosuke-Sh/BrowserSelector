// <copyright file="UpdateChannel.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Core.Models;

/// <summary>
/// 更新の適用経路（Phase H-1）.
/// 現在の実行ファイルがインストーラ配置かポータブル配置かで決まる.
/// </summary>
public enum UpdateChannel
{
    /// <summary>
    /// インストーラ（BrowserSelector-Setup-v*.exe）をサイレント実行して適用する経路.
    /// Program Files配下へのインストール時の主経路.
    /// </summary>
    Installer = 0,

    /// <summary>
    /// ポータブルZIPを展開し、BrowserSelector.Updater.exeがファイルを置換する経路.
    /// </summary>
    Portable = 1,
}
