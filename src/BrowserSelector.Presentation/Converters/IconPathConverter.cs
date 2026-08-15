using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BrowserSelector.Presentation.Converters;

/// <summary>
/// アイコンパスの変換を行うコンバーター.
/// </summary>
public class IconPathConverter : IMultiValueConverter
{
    /// <summary>
    /// デフォルトアイコンのパス.
    /// </summary>
    private const string DefaultIconPath = "/BrowserSelector.Presentation;component/Resources/Images/Icon_Browser.png";

    /// <inheritdoc/>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(values);
        try
        {
            // IconPath、ExecutablePath、Name、IconIndexの順で取得
            string? iconPath = values[0] as string;
            string? executablePath = values[1] as string;
            string? name = values[2] as string;
            int iconIndex = values.Length > 3 && values[3] is int idx ? idx : 0;

            // デバッグログ追加
            System.Diagnostics.Debug.WriteLine($"IconPathConverter: {name}, IconPath: {iconPath}, ExecutablePath: {executablePath}, IconIndex: {iconIndex}");

            // 1. IconPathが設定されている場合はそれを優先
            if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
            {

                // IconPathがexeファイルの場合は、高解像度アイコンを抽出
                if (iconPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                    iconPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        BitmapImage? bitmap = ExtractHighQualityIcon(iconPath, iconIndex); // 指定されたインデックスのアイコンを取得
                        if (bitmap != null)
                        {
                            return bitmap;
                        }
                    }
                    // CA1031: アイコン抽出処理はWin32 API/System.Drawing/ファイルIO由来の多様な例外を返しうるベストエフォート処理。失敗時はデフォルトアイコンにフォールバックするための意図的な汎用catch。
                    #pragma warning disable CA1031
                    catch (Exception)
                    {
                        // アイコン抽出エラーは無視
                    }
                    #pragma warning restore CA1031
                }
                else
                {
                    // 画像ファイル（.ico, .png等）の場合は直接読み込み
                    try
                    {
                        return new BitmapImage(new Uri(iconPath));
                    }
                    // CA1031: アイコン抽出処理はWin32 API/System.Drawing/ファイルIO由来の多様な例外を返しうるベストエフォート処理。失敗時はデフォルトアイコンにフォールバックするための意図的な汎用catch。
                    #pragma warning disable CA1031
                    catch (Exception)
                    {
                        // 画像ファイル読み込みエラーは無視
                    }
                    #pragma warning restore CA1031
                }
            }

            // 2. ExecutablePathから高解像度アイコンを抽出
            if (!string.IsNullOrEmpty(executablePath) && File.Exists(executablePath))
            {
                try
                {
                    BitmapImage? bitmap = ExtractHighQualityIcon(executablePath, iconIndex); // 指定されたインデックスのアイコンを取得
                    if (bitmap != null)
                    {
                        return bitmap;
                    }
                }
                // CA1031: アイコン抽出処理はWin32 API/System.Drawing/ファイルIO由来の多様な例外を返しうるベストエフォート処理。失敗時はデフォルトアイコンにフォールバックするための意図的な汎用catch。
                #pragma warning disable CA1031
                catch (Exception)
                {
                    // アイコン抽出エラーは無視
                }
                #pragma warning restore CA1031
            }

            // 3. デフォルトアイコンを返す
            return DefaultIconPath;
        }
        // CA1031: アイコン抽出処理はWin32 API/System.Drawing/ファイルIO由来の多様な例外を返しうるベストエフォート処理。失敗時はデフォルトアイコンにフォールバックするための意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception)
        {
            // エラーが発生した場合はデフォルトアイコンを返す
            return DefaultIconPath;
        }
        #pragma warning restore CA1031
    }

    /// <inheritdoc/>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern int ExtractIconEx(string szFileName, int nIconIndex, out IntPtr phiconLarge, out IntPtr phiconSmall, int nIcons);

    /// <summary>
    /// 高解像度アイコンを抽出します.
    /// </summary>
    /// <param name="filePath">ファイルパス.</param>
    /// <param name="iconIndex">アイコンインデックス（0が最初のアイコン）.</param>
    /// <returns>高解像度BitmapImage.</returns>
    private BitmapImage? ExtractHighQualityIcon(string filePath, int iconIndex)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"ExtractHighQualityIcon開始: {filePath}, IconIndex: {iconIndex}");
            // まず、利用可能なアイコン数を取得
            int iconCount = ExtractIconEx(filePath, -1, out IntPtr dummy1, out IntPtr dummy2, 0);
            System.Diagnostics.Debug.WriteLine($"利用可能なアイコン数: {iconCount}");

            if (iconCount > 0 && iconIndex < iconCount)
            {
                // 指定されたインデックスのアイコンを抽出
                System.Diagnostics.Debug.WriteLine($"IconIndex {iconIndex} でアイコン抽出を試行");
                if (ExtractIconEx(filePath, iconIndex, out IntPtr largeIcon, out IntPtr smallIcon, 1) > 0 && largeIcon != IntPtr.Zero)
                {
                    System.Diagnostics.Debug.WriteLine($"IconIndex {iconIndex} のアイコン抽出成功");
                    using System.Drawing.Icon icon = System.Drawing.Icon.FromHandle(largeIcon);

                    // アイコンの元のサイズを取得
                    System.Drawing.Size originalSize = icon.Size;

                    // リサイズせずに元のアイコンをそのまま使用
                    BitmapImage bitmap = ConvertIconToHighQualityBitmapImage(icon);
                    return bitmap;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"IconIndex {iconIndex} のアイコン抽出失敗");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"IconIndex {iconIndex} は範囲外、フォールバックを使用");
                // フォールバック: 標準のExtractAssociatedIconを使用
                System.Drawing.Icon? icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                if (icon != null)
                {
                    System.Diagnostics.Debug.WriteLine($"ExtractAssociatedIcon成功");
                    // リサイズせずに元のアイコンをそのまま使用
                    System.Drawing.Size originalSize = icon.Size;
                    BitmapImage bitmap = ConvertIconToHighQualityBitmapImage(icon);
                    return bitmap;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"ExtractAssociatedIcon失敗");
                }
            }
        }
        // CA1031: アイコン抽出処理はWin32 API/System.Drawing/ファイルIO由来の多様な例外を返しうるベストエフォート処理。失敗時はデフォルトアイコンにフォールバックするための意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            // アイコン抽出エラーは無視
            System.Diagnostics.Debug.WriteLine($"ExtractHighQualityIconエラー: {ex.Message}");
        }
        #pragma warning restore CA1031

        return null;
    }

    /// <summary>
    /// アイコンを高品質なBitmapImageに変換.
    /// </summary>
    /// <param name="icon">変換するアイコン.</param>
    /// <returns>高品質なBitmapImage.</returns>
    private BitmapImage ConvertIconToHighQualityBitmapImage(System.Drawing.Icon icon)
    {
        try
        {
            using MemoryStream stream = new();
            icon.Save(stream);
            stream.Position = 0;

            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;

            // DPI設定を明示的に指定
            bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            // 高品質スケーリングを有効化
            RenderOptions.SetBitmapScalingMode(bitmap, BitmapScalingMode.HighQuality);
            RenderOptions.SetEdgeMode(bitmap, EdgeMode.Aliased);

            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
        // CA1031: アイコン抽出処理はWin32 API/System.Drawing/ファイルIO由来の多様な例外を返しうるベストエフォート処理。失敗時はデフォルトアイコンにフォールバックするための意図的な汎用catch。
        #pragma warning disable CA1031
        catch (Exception)
        {
            return null!;
        }
        #pragma warning restore CA1031
    }

}
