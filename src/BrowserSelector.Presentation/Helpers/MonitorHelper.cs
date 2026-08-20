// <copyright file="MonitorHelper.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace BrowserSelector.Presentation.Helpers;

/// <summary>
/// ウィンドウをマルチモニター環境で適切な位置（カーソルのあるモニター、または
/// ウィンドウが現在表示されているモニター）に中央寄せするヘルパー。
/// Presentation層はWinFormsを参照しないため<c>System.Windows.Forms.Screen</c>は使わず、
/// Win32 P/Invoke（<c>GetCursorPos</c>/<c>MonitorFromPoint</c>/<c>GetMonitorInfo</c>）で実装する.
/// </summary>
public static class MonitorHelper
{
    private const uint MonitorDefaultToNearest = 2;
    private const uint MdtEffectiveDpi = 0;
    private const double DefaultDpi = 96.0;

    /// <summary>
    /// 現在のマウスカーソルがあるモニターの作業領域中央にウィンドウを配置する.
    /// アプリ起動時（URLリンクをクリックしたモニターと同一モニターに表示する用途）に使用する.
    /// </summary>
    /// <param name="window">配置対象のウィンドウ.</param>
    public static void CenterOnCursorMonitor(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (!TryGetCursorPosition(out NativeMethods.POINT cursorPoint))
        {
            return;
        }

        PositionOnMonitor(window, NativeMethods.MonitorFromPoint(cursorPoint, MonitorDefaultToNearest));
    }

    /// <summary>
    /// ウィンドウが現在表示されているモニターの作業領域中央にウィンドウを配置し直す.
    /// 設定変更等でサイズが変わった際、既存の表示モニターを維持する用途に使用する.
    /// </summary>
    /// <param name="window">配置対象のウィンドウ.</param>
    public static void CenterOnWindowMonitor(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        WindowInteropHelper interopHelper = new(window);
        if (interopHelper.Handle == IntPtr.Zero)
        {
            return;
        }

        IntPtr monitor = NativeMethods.MonitorFromWindow(interopHelper.Handle, MonitorDefaultToNearest);
        PositionOnMonitor(window, monitor);
    }

    /// <summary>
    /// 作業領域内での中央寄せ位置を計算する（純粋関数・単体テスト用）.
    /// 結果は作業領域内にクランプされ、ウィンドウが作業領域より大きい場合でも
    /// タイトルバーが画面外に出ないようにする.
    /// </summary>
    /// <param name="workLeft">作業領域の左端（px）.</param>
    /// <param name="workTop">作業領域の上端（px）.</param>
    /// <param name="workWidth">作業領域の幅（px）.</param>
    /// <param name="workHeight">作業領域の高さ（px）.</param>
    /// <param name="windowWidth">ウィンドウの幅（px）.</param>
    /// <param name="windowHeight">ウィンドウの高さ（px）.</param>
    /// <returns>中央寄せされたウィンドウの左上座標（px）.</returns>
    public static (double Left, double Top) CalculateCenteredPosition(
        double workLeft,
        double workTop,
        double workWidth,
        double workHeight,
        double windowWidth,
        double windowHeight)
    {
        double left = workLeft + ((workWidth - windowWidth) / 2.0);
        double top = workTop + ((workHeight - windowHeight) / 2.0);

        // 作業領域より大きいウィンドウ等でタイトルバーが画面外に出ないようクランプする。
        double maxLeft = workLeft + Math.Max(0, workWidth - windowWidth);
        double maxTop = workTop + Math.Max(0, workHeight - windowHeight);
        left = Math.Clamp(left, workLeft, Math.Max(workLeft, maxLeft));
        top = Math.Clamp(top, workTop, Math.Max(workTop, maxTop));

        return (left, top);
    }

    private static void PositionOnMonitor(Window window, IntPtr monitor)
    {
        if (monitor == IntPtr.Zero)
        {
            return;
        }

        WindowInteropHelper interopHelper = new(window);
        if (interopHelper.Handle == IntPtr.Zero)
        {
            return;
        }

        NativeMethods.MONITORINFO monitorInfo = default;
        monitorInfo.cbSize = Marshal.SizeOf<NativeMethods.MONITORINFO>();
        if (!NativeMethods.GetMonitorInfo(monitor, ref monitorInfo))
        {
            return;
        }

        double scale = GetMonitorDpiScale(monitor, window);

        double workLeftPx = monitorInfo.rcWork.Left;
        double workTopPx = monitorInfo.rcWork.Top;
        double workWidthPx = monitorInfo.rcWork.Right - monitorInfo.rcWork.Left;
        double workHeightPx = monitorInfo.rcWork.Bottom - monitorInfo.rcWork.Top;

        double windowWidthPx = window.Width * scale;
        double windowHeightPx = window.Height * scale;

        (double leftPx, double topPx) = CalculateCenteredPosition(
            workLeftPx, workTopPx, workWidthPx, workHeightPx, windowWidthPx, windowHeightPx);

        // WPFのWindow.Left/Topはプライマリモニター基準のDIP座標であり、
        // Per-Monitor DPI環境（モニターごとに拡大率が異なる、または負座標のモニターがある場合）では
        // 単一のscale値で物理px座標から正しく変換できない（過去、この除算により
        // 負座標モニターでウィンドウが画面外へ配置される不具合があった）。
        // SetWindowPosへ物理px座標をそのまま渡すことで座標系の変換を一切行わずに配置する。
        _ = NativeMethods.SetWindowPos(
            interopHelper.Handle,
            IntPtr.Zero,
            (int)Math.Round(leftPx),
            (int)Math.Round(topPx),
            0,
            0,
            NativeMethods.SwpNoSize | NativeMethods.SwpNoZOrder | NativeMethods.SwpNoActivate);
    }

    private static double GetMonitorDpiScale(IntPtr monitor, Window window)
    {
        try
        {
            if (NativeMethods.GetDpiForMonitor(monitor, MdtEffectiveDpi, out uint dpiX, out _) == 0)
            {
                return dpiX / DefaultDpi;
            }
        }
        catch (DllNotFoundException)
        {
            // shcore.dllが無い環境（対応外OS等）はフォールバックする。
        }
        catch (EntryPointNotFoundException)
        {
            // 古いOSでGetDpiForMonitorが無い場合はフォールバックする。
        }

        Matrix toDevice = PresentationSource.FromVisual(window)?.CompositionTarget?.TransformToDevice ?? Matrix.Identity;
        return toDevice.M11 > 0 ? toDevice.M11 : 1.0;
    }

    private static bool TryGetCursorPosition(out NativeMethods.POINT point)
    {
        try
        {
            return NativeMethods.GetCursorPos(out point);
        }
        catch (EntryPointNotFoundException)
        {
            point = default;
            return false;
        }
    }

    // Win32 API のシグネチャ・構造体はネイティブ側の名前規約（PascalCase以外のフィールド名等）に
    // 厳密に合わせる必要があり、C#の命名規則（SA1307/S101）を意図的に適用しない。
    // 構造体フィールドの一部（X/Y/rcMonitor/dwFlags等）はP/InvokeマーシャリングのためだけにC#側では
    // 読み取らないためS1144（未使用フィールド）も併せて抑制する。構造体宣言をメソッドより先に
    // 置く必要がある（メソッドシグネチャで参照するため）ためSA1201も対象。
    // ネストしたクラスへ隔離することで、この一箇所にのみ抑制を限定する。
#pragma warning disable SA1307, S101, SA1310, SA1201, S1144
    private static class NativeMethods
    {
        public const uint SwpNoSize = 0x0001;
        public const uint SwpNoZOrder = 0x0004;
        public const uint SwpNoActivate = 0x0010;

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("shcore.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern int GetDpiForMonitor(IntPtr hmonitor, uint dpiType, out uint dpiX, out uint dpiY);

        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
    }
#pragma warning restore SA1307, S101, SA1310, SA1201, S1144
}
