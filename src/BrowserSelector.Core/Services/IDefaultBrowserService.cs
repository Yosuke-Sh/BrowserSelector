// <copyright file="IDefaultBrowserService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

namespace BrowserSelector.Core.Services;

/// <summary>
/// OS（Windows）レベルの既定ブラウザ判定・設定導線を提供するサービス。
/// <see cref="IBrowserService.SetDefaultBrowserAsync(Guid)"/>/<see cref="IBrowserService.GetDefaultBrowserAsync"/>は
/// アプリ内で自動起動対象とする「既定タイル」を指すものであり、本サービスが表すOSの既定ブラウザとは異なる概念である.
/// </summary>
public interface IDefaultBrowserService
{
    /// <summary>
    /// BrowserSelectorがWindowsの既定ブラウザ（https用ハンドラー）として設定されているかどうかを判定する.
    /// </summary>
    /// <returns>既定ブラウザであれば<see langword="true"/>.</returns>
    bool IsDefaultBrowser();

    /// <summary>
    /// Windowsの「既定のアプリ」設定画面をBrowserSelectorの項目にフォーカスした状態で開く。
    /// Windows 11ではプロトコル単位で既定アプリを選択する方式のため、OSに直接設定を書き込むことはできない.
    /// </summary>
    void OpenDefaultAppsSettings();
}
