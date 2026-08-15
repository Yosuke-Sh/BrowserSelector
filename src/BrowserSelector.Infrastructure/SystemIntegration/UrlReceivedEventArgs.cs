// <copyright file="UrlReceivedEventArgs.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
namespace BrowserSelector.Infrastructure.SystemIntegration;

/// <summary>
/// <see cref="SingleInstanceManager.UrlReceived"/> イベントの引数.
/// </summary>
public sealed class UrlReceivedEventArgs : EventArgs
{
    /// <summary>
    /// <see cref="UrlReceivedEventArgs"/> クラスの新しいインスタンスを初期化します.
    /// </summary>
    /// <param name="url">後続インスタンスから転送されたURL.</param>
    // CA1054/CA1056: 空文字は「URL指定無し、ウィンドウ復元のみ要求」を表す正当な値であり、
    // Uri型では表現できないため文字列のまま保持する意図的な設計。
#pragma warning disable CA1054, CA1056
    public UrlReceivedEventArgs(string url)
    {
        Url = url;
    }

    /// <summary>
    /// 後続インスタンスから転送されたURL（空文字の場合はウィンドウ復元のみの要求）.
    /// </summary>
    public string Url { get; }
#pragma warning restore CA1054, CA1056
}
