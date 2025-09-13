// <copyright file="ISettingsService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// 設定管理サービスのインターフェース.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// アプリケーション設定を読み込み.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<AppSettings> LoadAppSettingsAsync();

    /// <summary>
    /// アプリケーション設定を保存.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> SaveAppSettingsAsync(AppSettings settings);

    /// <summary>
    /// 視覚設定を読み込み.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<VisualSettings> LoadVisualSettingsAsync();

    /// <summary>
    /// 視覚設定を保存.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> SaveVisualSettingsAsync(VisualSettings settings);

    /// <summary>
    /// ログ設定を読み込み.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<LogSettings> LoadLogSettingsAsync();

    /// <summary>
    /// ログ設定を保存.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> SaveLogSettingsAsync(LogSettings settings);

    /// <summary>
    /// 設定ファイルのパスを取得.
    /// </summary>
    /// <returns></returns>
    string GetSettingsFilePath();

    /// <summary>
    /// 設定を初期値にリセット.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> ResetSettingsAsync();

    /// <summary>
    /// 設定をファイルからインポート.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> ImportSettingsAsync(string filePath);

    /// <summary>
    /// 設定をファイルにエクスポート.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> ExportSettingsAsync(string filePath);
}
