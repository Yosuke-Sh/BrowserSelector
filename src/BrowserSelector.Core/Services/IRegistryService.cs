// <copyright file="IRegistryService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// Windowsレジストリからブラウザ惁E��を取得するサービスのインターフェース
/// </summary>
public interface IRegistryService
{
    /// <summary>
    /// レジストリからブラウザを検�E
    /// </summary>
    Task<IEnumerable<Browser>> DetectBrowsersFromRegistryAsync();
}

