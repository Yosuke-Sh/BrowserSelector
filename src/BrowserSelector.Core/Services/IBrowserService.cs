// <copyright file="IBrowserService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// ブラウザ管理サービスのインターフェース.
/// </summary>
public interface IBrowserService
{
    /// <summary>
    /// システムにインストールされているブラウザを検出.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<IEnumerable<Browser>> DetectBrowsersAsync();

    /// <summary>
    /// ブラウザを起動.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> LaunchBrowserAsync(Browser browser, string url);

    /// <summary>
    /// ブラウザを追加.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> AddBrowserAsync(Browser browser);

    /// <summary>
    /// ブラウザを更新.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> UpdateBrowserAsync(Browser browser);

    /// <summary>
    /// ブラウザを削除.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> RemoveBrowserAsync(Guid browserId);

    /// <summary>
    /// すべてのブラウザを取得.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<IEnumerable<Browser>> GetAllBrowsersAsync();

    /// <summary>
    /// デフォルトブラウザを設定.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> SetDefaultBrowserAsync(Guid browserId);

    /// <summary>
    /// デフォルトブラウザを取得.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Browser?> GetDefaultBrowserAsync();

    /// <summary>
    /// ブラウザの使用統計を更新.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task UpdateBrowserUsageAsync(Guid browserId);

    /// <summary>
    /// ブラウザの使用統計を更新.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task UpdateUsageAsync(Browser browser);
}
