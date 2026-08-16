// <copyright file="IRegistryService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// Windowsレジストリからブラウザ情報を取得するサービスのインターフェース.
/// </summary>
public interface IRegistryService
{
    /// <summary>
    /// レジストリからブラウザを検出.
    /// </summary>
    /// <returns>検出されたブラウザの一覧.</returns>
    Task<IEnumerable<Browser>> DetectBrowsersFromRegistryAsync();
}
