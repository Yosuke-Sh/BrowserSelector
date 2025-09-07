// <copyright file="ISettingsService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// 設定管理サービスのインターフェース
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// アプリケーション設定を読み込み
    /// </summary>
    Task<AppSettings> LoadAppSettingsAsync();

    /// <summary>
    /// アプリケーション設定を保存
    /// </summary>
    Task<bool> SaveAppSettingsAsync(AppSettings settings);

    /// <summary>
    /// 視覚設定を読み込み
    /// </summary>
    Task<VisualSettings> LoadVisualSettingsAsync();

    /// <summary>
    /// 視覚設定を保存
    /// </summary>
    Task<bool> SaveVisualSettingsAsync(VisualSettings settings);

    /// <summary>
    /// ログ設定を読み込み
    /// </summary>
    Task<LogSettings> LoadLogSettingsAsync();

    /// <summary>
    /// ログ設定を保存
    /// </summary>
    Task<bool> SaveLogSettingsAsync(LogSettings settings);

    /// <summary>
    /// 設定ファイルのパスを取得
    /// </summary>
    string GetSettingsFilePath();

    /// <summary>
    /// 設定を初期値にリセット
    /// </summary>
    Task<bool> ResetSettingsAsync();

    /// <summary>
    /// 設定をファイルからインポート
    /// </summary>
    Task<bool> ImportSettingsAsync(string filePath);

    /// <summary>
    /// 設定をファイルにエクスポート
    /// </summary>
    Task<bool> ExportSettingsAsync(string filePath);
}
