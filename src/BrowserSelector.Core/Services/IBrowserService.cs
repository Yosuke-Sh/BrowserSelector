// <copyright file="IBrowserService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// ブラウザ管理サービスのインターフェース
/// </summary>
public interface IBrowserService
{
    /// <summary>
    /// システムにインストールされているブラウザを検出
    /// </summary>
    Task<IEnumerable<Browser>> DetectBrowsersAsync();

    /// <summary>
    /// ブラウザを起動
    /// </summary>
    Task<bool> LaunchBrowserAsync(Browser browser, string url);

    /// <summary>
    /// ブラウザを追加
    /// </summary>
    Task<bool> AddBrowserAsync(Browser browser);

    /// <summary>
    /// ブラウザを更新
    /// </summary>
    Task<bool> UpdateBrowserAsync(Browser browser);

    /// <summary>
    /// ブラウザを削除
    /// </summary>
    Task<bool> RemoveBrowserAsync(Guid browserId);

    /// <summary>
    /// すべてのブラウザを取得
    /// </summary>
    Task<IEnumerable<Browser>> GetAllBrowsersAsync();

    /// <summary>
    /// デフォルトブラウザを設定
    /// </summary>
    Task<bool> SetDefaultBrowserAsync(Guid browserId);

    /// <summary>
    /// デフォルトブラウザを取得
    /// </summary>
    Task<Browser?> GetDefaultBrowserAsync();

    /// <summary>
    /// ブラウザの使用統計を更新
    /// </summary>
    Task UpdateBrowserUsageAsync(Guid browserId);

    /// <summary>
    /// ブラウザの使用統計を更新
    /// </summary>
    Task UpdateUsageAsync(Browser browser);
}
