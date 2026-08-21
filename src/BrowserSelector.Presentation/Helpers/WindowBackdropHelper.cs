// <copyright file="WindowBackdropHelper.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace BrowserSelector.Presentation.Helpers;

/// <summary>
/// DWM (Desktop Window Manager) を用いてウィンドウにガラス効果（Mica / Acrylic）と
/// 角丸・ダークモード追従を適用するヘルパー.
/// <see cref="AllowsTransparency"/> は使わず、<c>DwmSetWindowAttribute</c> による
/// バックドロップ合成でアイコンの鮮明さとGPUレンダリングを維持する（Phase C-1）.
/// </summary>
public static class WindowBackdropHelper
{
    /// <summary>
    /// Windows 11 22H2（<c>DWMWA_SYSTEMBACKDROP_TYPE</c> が使用可能になるビルド番号）.
    /// </summary>
    public const int Windows11Build22H2 = 22621;

    /// <summary>
    /// Windows 11 21H2（<c>DWMWA_MICA_EFFECT</c> のみ使用可能なビルド番号帯の下限）.
    /// </summary>
    public const int Windows11Build21H2 = 22000;

    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaWindowCornerPreference = 33;
    private const int DwmwaSystemBackdropType = 38;
    private const int DwmwaMicaEffect = 1029;

    private const int DwmwcpDoNotRound = 1;
    private const int DwmwcpRound = 2;
    private const int DwmwcpRoundSmall = 3;

    /// <summary>
    /// 角丸半径スライダー（0〜20px）のうち、<c>DWMWCP_ROUNDSMALL</c>（小さめの角丸）を
    /// 適用する上限値。これ未満は小さめ、これ以上は通常の丸みを適用する.
    /// </summary>
    private const double SmallCornerRadiusThreshold = 8;

    // Windows 11 22H2 (build 22621) 以降で有効な DWMSBT 値
    private const int DwmsbtMainWindow = 2; // Mica
    private const int DwmsbtTransientWindow = 3; // Acrylic
    private const int DwmsbtTabbedWindow = 4; // MicaAlt

    /// <summary>
    /// バックドロップの種類.
    /// </summary>
    public enum BackdropKind
    {
        /// <summary>Mica（既定の不透明多層ブラー）.</summary>
        Mica,

        /// <summary>Acrylic（半透明・強めのブラー）.</summary>
        Acrylic,

        /// <summary>MicaAlt（タブ付きウィンドウ向けの濃いMica）.</summary>
        MicaAlt,
    }

    /// <summary>
    /// OSビルド番号に応じたDWMバックドロップの対応状況.
    /// </summary>
    public enum DwmBackdropSupport
    {
        /// <summary>Windows 10以下。DWMバックドロップ非対応、フォールバック必須.</summary>
        Unsupported,

        /// <summary>Windows 11 21H2。<c>DWMWA_MICA_EFFECT</c>のみ対応.</summary>
        MicaEffectOnly,

        /// <summary>Windows 11 22H2以降。<c>DWMWA_SYSTEMBACKDROP_TYPE</c>でMica/Acrylic/MicaAltを選択可能.</summary>
        SystemBackdropType,
    }

    /// <summary>
    /// Gets 現在のOSビルド番号を取得する（テスト容易性のためpublicにして参照可能にする）.
    /// </summary>
    public static int OsBuildNumber => Environment.OSVersion.Version.Build;

    /// <summary>
    /// 不透明フォールバック（半透明単色ブラシ）を使うべきかどうかを判定する（テスト容易性のため <see cref="Apply"/> から分離）.
    /// ハイコントラストモード、または「透明効果オフ」設定時は常にフォールバックする（アクセシビリティ要件）.
    /// </summary>
    /// <param name="isHighContrast"><see cref="SystemParameters.HighContrast"/> の値.</param>
    /// <param name="glassEffectEnabled">ユーザー設定でガラス効果が有効か.</param>
    /// <returns>フォールバックすべき場合は <see langword="true"/>.</returns>
    public static bool ShouldUseOpaqueFallback(bool isHighContrast, bool glassEffectEnabled)
    {
        return isHighContrast || !glassEffectEnabled;
    }

    /// <summary>
    /// OSビルド番号から、DWMバックドロップの適用方式を判定する（テスト容易性のため <see cref="Apply"/> から分離）.
    /// </summary>
    /// <param name="osBuild">OSビルド番号.</param>
    /// <returns>適用可能な方式.</returns>
    public static DwmBackdropSupport ResolveBackdropSupport(int osBuild)
    {
        if (osBuild >= Windows11Build22H2)
        {
            return DwmBackdropSupport.SystemBackdropType;
        }

        if (osBuild >= Windows11Build21H2)
        {
            return DwmBackdropSupport.MicaEffectOnly;
        }

        return DwmBackdropSupport.Unsupported;
    }

    /// <summary>
    /// 角丸半径設定値（px, 0〜20を想定）から、DWMの<c>DWMWA_WINDOW_CORNER_PREFERENCE</c>へ渡す値を決定する
    /// （テスト容易性のため <see cref="Apply"/> から分離）。
    /// DWMは数値半径を直接指定できず「丸めない／小さめに丸める／通常に丸める」の3段階のみ制御可能なため、
    /// 0は丸めない、1〜7pxは小さめ、8px以上は通常の丸みへ丸め込む.
    /// </summary>
    /// <param name="cornerRadiusPreference">設定された角丸半径（px）.</param>
    /// <returns><c>DWMWA_WINDOW_CORNER_PREFERENCE</c>に渡す値.</returns>
    public static int ResolveCornerPreference(double cornerRadiusPreference)
    {
        if (cornerRadiusPreference <= 0)
        {
            return DwmwcpDoNotRound;
        }

        return cornerRadiusPreference < SmallCornerRadiusThreshold ? DwmwcpRoundSmall : DwmwcpRound;
    }

    /// <summary>
    /// ウィンドウにDWMバックドロップを適用する。<see cref="Window.SourceInitialized"/> 以降（HWND確定後）に呼び出すこと.
    /// ハイコントラストモード・透明効果オフ設定・Windows 10以下では不透明な単色ブラシへフォールバックする.
    /// </summary>
    /// <param name="window">対象ウィンドウ.</param>
    /// <param name="kind">要求するバックドロップ種別.</param>
    /// <param name="isDarkMode">ダークテーマとして描画するか（ウィンドウ枠の <c>DWMWA_USE_IMMERSIVE_DARK_MODE</c> と中身のテーマを一致させる）.</param>
    /// <param name="glassEffectEnabled">ユーザー設定でガラス効果が有効か（<see cref="Core.Models.AppSettings.EnableGlassEffect"/>）.</param>
    /// <param name="cornerRadiusPreference">
    /// 外観タブ（Phase E-1）の角丸半径設定（px）。<see cref="ResolveCornerPreference(double)"/> を参照.
    /// </param>
    /// <returns>実際にDWMバックドロップが適用された場合は <see langword="true"/>。フォールバック（半透明単色ブラシ）を行った場合は <see langword="false"/>.</returns>
    public static bool Apply(Window window, BackdropKind kind, bool isDarkMode, bool glassEffectEnabled, double cornerRadiusPreference = 1)
    {
        ArgumentNullException.ThrowIfNull(window);

        WindowInteropHelper interopHelper = new(window);
        IntPtr hwnd = interopHelper.Handle;
        if (hwnd == IntPtr.Zero)
        {
            return false;
        }

        // ダークモード追従（枠の色）は常に試行する。中身のテーマと必ず一致させること（C-0とC-1の不整合防止）。
        int darkModeValue = isDarkMode ? 1 : 0;
        _ = TryDwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkMode, ref darkModeValue, sizeof(int));

        int cornerPreference = ResolveCornerPreference(cornerRadiusPreference);
        _ = TryDwmSetWindowAttribute(hwnd, DwmwaWindowCornerPreference, ref cornerPreference, sizeof(int));

        if (ShouldUseOpaqueFallback(SystemParameters.HighContrast, glassEffectEnabled))
        {
            ApplyOpaqueFallback(window, isDarkMode);
            return false;
        }

        DwmBackdropSupport support = ResolveBackdropSupport(OsBuildNumber);
        if (support == DwmBackdropSupport.SystemBackdropType)
        {
            int backdropValue = kind switch
            {
                BackdropKind.Acrylic => DwmsbtTransientWindow,
                BackdropKind.MicaAlt => DwmsbtTabbedWindow,
                _ => DwmsbtMainWindow,
            };

            bool applied = TryDwmSetWindowAttribute(hwnd, DwmwaSystemBackdropType, ref backdropValue, sizeof(int));
            if (applied)
            {
                window.Background = Brushes.Transparent;
                return true;
            }
        }
        else if (support == DwmBackdropSupport.MicaEffectOnly)
        {
            // Windows 11 21H2 は DWMWA_SYSTEMBACKDROP_TYPE 未対応。DWMWA_MICA_EFFECT で代替する。
            int micaValue = 1;
            bool applied = TryDwmSetWindowAttribute(hwnd, DwmwaMicaEffect, ref micaValue, sizeof(int));
            if (applied)
            {
                window.Background = Brushes.Transparent;
                return true;
            }
        }

        // Windows 10以下、またはDWM呼び出し失敗時は半透明単色ブラシへフォールバック。
        ApplyOpaqueFallback(window, isDarkMode);
        return false;
    }

    private static void ApplyOpaqueFallback(Window window, bool isDarkMode)
    {
        // アクセシビリティ要件・Windows 10以下・透明効果オフ設定時のフォールバック。
        // 半透明ではあるが視認性を優先し、既存のBackgroundBrushConverter/トークンに委ねられるよう
        // 呼び出し側（MainWindow）でBackgroundのローカル値を上書きしないことを前提に、ここではTransparent以外を明示する。
        Color fallbackColor = isDarkMode ? Color.FromArgb(0xFF, 0x20, 0x20, 0x26) : Color.FromArgb(0xFF, 0xFA, 0xFA, 0xFA);
        window.Background = new SolidColorBrush(fallbackColor);
    }

    private static bool TryDwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size)
    {
        try
        {
            int hr = DwmSetWindowAttribute(hwnd, attribute, ref value, size);
            return hr == 0; // S_OK
        }
        catch (EntryPointNotFoundException)
        {
            return false;
        }
        catch (DllNotFoundException)
        {
            return false;
        }
    }

    [DllImport("dwmapi.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);
}
