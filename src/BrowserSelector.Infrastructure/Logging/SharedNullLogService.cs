// <copyright file="SharedNullLogService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using BrowserSelector.Core.Enums;
using BrowserSelector.Core.Models;
using BrowserSelector.Core.Services;

namespace BrowserSelector.Infrastructure.Logging;

/// <summary>
/// ログサービスが未指定の呼び出し元向けの何もしない実装.
/// DI外でサービスを簡易生成する際、そのたびに<see cref="LogService"/>（ファイルI/O・タイマー付き）
/// を新規生成して破棄されないまま放置される事態を避けるための共有インスタンス.
/// </summary>
public sealed class SharedNullLogService : ILogService
{
    private SharedNullLogService()
    {
    }

    /// <summary>
    /// 共有インスタンス.
    /// </summary>
    public static SharedNullLogService Instance { get; } = new();

    /// <inheritdoc/>
    public void LogTrace(string message, string? category = null, Exception? exception = null)
    {
    }

    /// <inheritdoc/>
    public void LogDebug(string message, string? category = null, Exception? exception = null)
    {
    }

    /// <inheritdoc/>
    public void LogInformation(string message, string? category = null, Exception? exception = null)
    {
    }

    /// <inheritdoc/>
    public void LogWarning(string message, string? category = null, Exception? exception = null)
    {
    }

    /// <inheritdoc/>
    public void LogError(string message, string? category = null, Exception? exception = null)
    {
    }

    /// <inheritdoc/>
    public void LogCritical(string message, string? category = null, Exception? exception = null)
    {
    }

    /// <inheritdoc/>
    public void Log(LogLevel level, string message, string? category = null, Exception? exception = null)
    {
    }

    /// <inheritdoc/>
    public void LogDetailed(
        LogLevel level,
        string message,
        string? category = null,
        string? eventId = null,
        string? requestTarget = null,
        string? userInfo = null,
        string? processTarget = null,
        string? processAction = null,
        string? processResult = null,
        Exception? exception = null)
    {
    }

    /// <inheritdoc/>
    public void UpdateSettings(LogSettings settings)
    {
    }

    /// <inheritdoc/>
    public void ClearLogs()
    {
    }

    /// <inheritdoc/>
    public void CleanupOldLogs()
    {
    }

    /// <inheritdoc/>
    public string GetLogContent(int maxLines = 1000)
    {
        return string.Empty;
    }

    /// <inheritdoc/>
    public string GetLogFilePath()
    {
        return string.Empty;
    }
}
