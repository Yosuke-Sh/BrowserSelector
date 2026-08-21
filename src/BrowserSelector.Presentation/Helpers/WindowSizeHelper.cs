// <copyright file="WindowSizeHelper.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using System.Windows;

namespace BrowserSelector.Presentation.Helpers;

/// <summary>
/// 設定されたウィンドウサイズを常に適用するためのヘルパー。
/// <c>Window.SizeToContent</c> を使わず、<see cref="Core.Models.VisualSettings.InitialWindowWidth"/>/
/// <see cref="Core.Models.VisualSettings.InitialWindowHeight"/> を唯一の正としてウィンドウへ適用する
/// （ユーザーによるドラッグリサイズは可能だが、設定へは書き戻さない）.
/// </summary>
public static class WindowSizeHelper
{
    /// <summary>ウィンドウ幅の下限（px）.</summary>
    public const double MinWindowWidth = 400.0;

    /// <summary>ウィンドウ幅の上限（px）.</summary>
    public const double MaxWindowWidth = 2000.0;

    /// <summary>ウィンドウ高さの下限（px）.</summary>
    public const double MinWindowHeight = 300.0;

    /// <summary>ウィンドウ高さの上限（px）.</summary>
    public const double MaxWindowHeight = 1500.0;

    /// <summary>
    /// 設定値を有効範囲へ丸める。<c>NaN</c>・<c>Infinity</c>・0以下の場合は下限値へフォールバックする
    /// （設定ファイル破損時に <see cref="Window.Width"/> への代入が例外を投げるのを防ぐ）.
    /// </summary>
    /// <param name="configuredWidth">設定されたウィンドウ幅（px）.</param>
    /// <param name="configuredHeight">設定されたウィンドウ高さ（px）.</param>
    /// <returns>有効範囲に収めた幅・高さ.</returns>
    public static (double Width, double Height) ResolveSize(double configuredWidth, double configuredHeight)
    {
        double width = ResolveDimension(configuredWidth, MinWindowWidth, MaxWindowWidth);
        double height = ResolveDimension(configuredHeight, MinWindowHeight, MaxWindowHeight);
        return (width, height);
    }

    /// <summary>
    /// <paramref name="window"/> に設定サイズを適用する.
    /// </summary>
    /// <param name="window">適用対象のウィンドウ.</param>
    /// <param name="configuredWidth">設定されたウィンドウ幅（px）.</param>
    /// <param name="configuredHeight">設定されたウィンドウ高さ（px）.</param>
    public static void ApplyConfiguredSize(Window window, double configuredWidth, double configuredHeight)
    {
        ArgumentNullException.ThrowIfNull(window);
        (double width, double height) = ResolveSize(configuredWidth, configuredHeight);
        window.Width = width;
        window.Height = height;
    }

    /// <summary>
    /// 現在のウィンドウサイズと、設定を反映した目標サイズが実質的に異なるかどうかを判定する。
    /// 設定画面を開くたびに無条件でリサイズ・センタリング・ログ出力が発生していた冗長動作を避けるために使用する.
    /// </summary>
    /// <param name="currentWidth">現在のウィンドウ幅（px）.</param>
    /// <param name="currentHeight">現在のウィンドウ高さ（px）.</param>
    /// <param name="configuredWidth">設定されたウィンドウ幅（px）.</param>
    /// <param name="configuredHeight">設定されたウィンドウ高さ（px）.</param>
    /// <returns>目標サイズが現在のサイズと0.5px以上異なる場合は<c>true</c>.</returns>
    public static bool NeedsResize(double currentWidth, double currentHeight, double configuredWidth, double configuredHeight)
    {
        const double tolerance = 0.5;
        (double targetWidth, double targetHeight) = ResolveSize(configuredWidth, configuredHeight);
        return Math.Abs(currentWidth - targetWidth) > tolerance || Math.Abs(currentHeight - targetHeight) > tolerance;
    }

    private static double ResolveDimension(double value, double min, double max)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
        {
            return min;
        }

        return Math.Clamp(value, min, max);
    }
}
