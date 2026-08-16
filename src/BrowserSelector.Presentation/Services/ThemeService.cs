// <copyright file="ThemeService.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>
using System.Windows;
using BrowserSelector.Core.Services;
using Microsoft.Win32;
using ThemeMode = BrowserSelector.Core.Enums.ThemeMode;

namespace BrowserSelector.Presentation.Services;

/// <summary>
/// アプリの外観テーマ（ライト/ダーク/システム追従）を切り替えるサービス.
/// <see cref="Application.Resources"/> の <see cref="ResourceDictionary.MergedDictionaries"/> を
/// トークン定義（Tokens.Light.xaml / Tokens.Dark.xaml）で差し替える方式で実現する.
/// </summary>
public sealed class ThemeService : IThemeService, IDisposable
{
    private const string LightTokensUri = "/BrowserSelector.Presentation;component/Resources/Themes/Tokens.Light.xaml";
    private const string DarkTokensUri = "/BrowserSelector.Presentation;component/Resources/Themes/Tokens.Dark.xaml";
    private const string PersonalizeKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string AppsUseLightThemeValueName = "AppsUseLightTheme";

    private readonly ILogService? _logService;
    private bool _disposed;

    /// <summary>
    /// <see cref="ThemeService"/> クラスの新しいインスタンスを初期化します.
    /// </summary>
    /// <param name="logService">ログサービス（省略可）.</param>
    public ThemeService(ILogService? logService = null)
    {
        _logService = logService;
        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
    }

    /// <inheritdoc/>
    public event EventHandler? ActiveThemeChanged;

    /// <inheritdoc/>
    public ThemeMode CurrentMode { get; private set; } = ThemeMode.Light;

    /// <inheritdoc/>
    public bool IsDarkThemeActive { get; private set; }

    /// <inheritdoc/>
    public void ApplyTheme(ThemeMode mode)
    {
        CurrentMode = mode;
        bool resolvedDark = mode switch
        {
            ThemeMode.Dark => true,
            ThemeMode.Light => false,
            ThemeMode.System => IsSystemThemeDark(),
            _ => false,
        };

        ApplyResolvedTheme(resolvedDark);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
    }

    private static bool IsSystemThemeDark()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(PersonalizeKeyPath);
            object? value = key?.GetValue(AppsUseLightThemeValueName);
            if (value is int intValue)
            {
                return intValue == 0;
            }
        }
        catch (System.Security.SecurityException)
        {
            // レジストリアクセス不可時はライトへフォールバック
        }
        catch (System.IO.IOException)
        {
            // レジストリアクセス不可時はライトへフォールバック
        }

        return false;
    }

    private void ApplyResolvedTheme(bool dark)
    {
        Application? app = Application.Current;
        if (app == null)
        {
            return;
        }

        string tokensUri = dark ? DarkTokensUri : LightTokensUri;
        ResourceDictionary tokens = new() { Source = new Uri(tokensUri, UriKind.Relative) };

        // 既存のトークン辞書（Tokens.Light/Dark由来）だけを除去し、Controls.xaml等の他の辞書は残す
        for (int i = app.Resources.MergedDictionaries.Count - 1; i >= 0; i--)
        {
            Uri? source = app.Resources.MergedDictionaries[i].Source;
            if (source != null && (source.OriginalString == LightTokensUri || source.OriginalString == DarkTokensUri))
            {
                app.Resources.MergedDictionaries.RemoveAt(i);
            }
        }

        app.Resources.MergedDictionaries.Insert(0, tokens);

        bool changed = IsDarkThemeActive != dark;
        IsDarkThemeActive = dark;

        if (changed)
        {
            ActiveThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        _logService?.LogDebug($"テーマ適用完了: Mode={CurrentMode}, ResolvedDark={dark}", nameof(ThemeService));
    }

    private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (CurrentMode != ThemeMode.System || e.Category != UserPreferenceCategory.General)
        {
            return;
        }

        Application? app = Application.Current;
        app?.Dispatcher.Invoke(() => ApplyResolvedTheme(IsSystemThemeDark()));
    }
}
