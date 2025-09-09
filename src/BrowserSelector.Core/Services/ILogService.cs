// <copyright file="ILogService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using BrowserSelector.Core.Enums;
using BrowserSelector.Core.Models;

namespace BrowserSelector.Core.Services;

/// <summary>
/// 繝ｭ繧ｰ繧ｵ繝ｼ繝薙せ縺ｮ繧､繝ｳ繧ｿ繝ｼ繝輔ぉ繝ｼ繧ｹ.
/// </summary>
public interface ILogService
{
    /// <summary>
    /// 繝医Ξ繝ｼ繧ｹ繝ｬ繝吶Ν縺ｮ繝ｭ繧ｰ繧貞・蜉・.
    /// </summary>
    void LogTrace(string message, string? category = null, Exception? exception = null);

    /// <summary>
    /// 繝・ヰ繝・げ繝ｬ繝吶Ν縺ｮ繝ｭ繧ｰ繧貞・蜉・.
    /// </summary>
    void LogDebug(string message, string? category = null, Exception? exception = null);

    /// <summary>
    /// 諠・ｱ繝ｬ繝吶Ν縺ｮ繝ｭ繧ｰ繧貞・蜉・.
    /// </summary>
    void LogInformation(string message, string? category = null, Exception? exception = null);

    /// <summary>
    /// 隴ｦ蜻翫Ξ繝吶Ν縺ｮ繝ｭ繧ｰ繧貞・蜉・.
    /// </summary>
    void LogWarning(string message, string? category = null, Exception? exception = null);

    /// <summary>
    /// 繧ｨ繝ｩ繝ｼ繝ｬ繝吶Ν縺ｮ繝ｭ繧ｰ繧貞・蜉・.
    /// </summary>
    void LogError(string message, string? category = null, Exception? exception = null);

    /// <summary>
    /// 閾ｴ蜻ｽ逧・お繝ｩ繝ｼ繝ｬ繝吶Ν縺ｮ繝ｭ繧ｰ繧貞・蜉・.
    /// </summary>
    void LogCritical(string message, string? category = null, Exception? exception = null);

    /// <summary>
    /// 謖・ｮ壹＆繧後◆繝ｬ繝吶Ν縺ｮ繝ｭ繧ｰ繧貞・蜉・.
    /// </summary>
    void Log(LogLevel level, string message, string? category = null, Exception? exception = null);

    /// <summary>
    /// 隧ｳ邏ｰ諠・ｱ莉倥″縺ｮ繝ｭ繧ｰ繧貞・蜉・.
    /// </summary>
    void LogDetailed(LogLevel level, string message, string? category = null,
                    string? eventId = null, string? requestTarget = null, string? userInfo = null,
                    string? processTarget = null, string? processAction = null, string? processResult = null,
                    Exception? exception = null);

    /// <summary>
    /// 繝ｭ繧ｰ險ｭ螳壹ｒ譖ｴ譁ｰ.
    /// </summary>
    void UpdateSettings(LogSettings settings);

    /// <summary>
    /// 繝ｭ繧ｰ繝輔ぃ繧､繝ｫ繧偵け繝ｪ繧｢.
    /// </summary>
    void ClearLogs();

    /// <summary>
    /// 蜿､縺・Ο繧ｰ繝輔ぃ繧､繝ｫ繧貞炎髯､.
    /// </summary>
    void CleanupOldLogs();

    /// <summary>
    /// 繝ｭ繧ｰ繝輔ぃ繧､繝ｫ縺ｮ蜀・ｮｹ繧貞叙蠕・.
    /// </summary>
    /// <returns></returns>
    string GetLogContent(int maxLines = 1000);

    /// <summary>
    /// 繝ｭ繧ｰ繝輔ぃ繧､繝ｫ縺ｮ繝代せ繧貞叙蠕・.
    /// </summary>
    /// <returns></returns>
    string GetLogFilePath();
}
