using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace BrowserSelector.Presentation.Converters;

/// <summary>
/// アイコンパスの変換を行うコンバーター
/// </summary>
public class IconPathConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            // IconPath、ExecutablePath、Nameの順で取得
            var iconPath = values[0] as string;
            var executablePath = values[1] as string;
            var name = values[2] as string;


            // 1. IconPathが設定されている場合はそれを優先
            if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
            {

                // IconPathがexeファイルの場合は、高解像度アイコンを抽出
                if (iconPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                    iconPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var bitmap = ExtractHighQualityIcon(iconPath, 0); // 最初のアイコンを取得
                        if (bitmap != null)
                        {
                            return bitmap;
                        }
                    }
                    catch (Exception)
                    {
                        // アイコン抽出エラーは無視
                    }
                }
                else
                {
                    // 画像ファイル（.ico, .png等）の場合は直接読み込み
                    try
                    {
                        return new BitmapImage(new Uri(iconPath));
                    }
                    catch (Exception)
                    {
                        // 画像ファイル読み込みエラーは無視
                    }
                }
            }

            // 2. ExecutablePathから高解像度アイコンを抽出
            if (!string.IsNullOrEmpty(executablePath) && File.Exists(executablePath))
            {
                try
                {
                    var bitmap = ExtractHighQualityIcon(executablePath, 0); // 最初のアイコンを取得
                    if (bitmap != null)
                    {
                        return bitmap;
                    }
                }
                catch (Exception)
                {
                    // アイコン抽出エラーは無視
                }
            }

            // 3. デフォルトアイコンを返す
            return GetDefaultIcon(name);
        }
        catch (Exception)
        {
            // エラーが発生した場合はデフォルトアイコンを返す
            return GetDefaultIcon(null);
        }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 高解像度アイコンを抽出します
    /// </summary>
    /// <param name="filePath">ファイルパス</param>
    /// <param name="iconIndex">アイコンインデックス（0が最初のアイコン）</param>
    /// <returns>高解像度BitmapImage</returns>
    private BitmapImage? ExtractHighQualityIcon(string filePath, int iconIndex)
    {
        try
        {
            // まず、利用可能なアイコン数を取得
            var iconCount = ExtractIconEx(filePath, -1, out IntPtr dummy1, out IntPtr dummy2, 0);

            if (iconCount > 0 && iconIndex < iconCount)
            {
                // 指定されたインデックスのアイコンを抽出
                if (ExtractIconEx(filePath, iconIndex, out IntPtr largeIcon, out IntPtr smallIcon, 1) > 0)
                {
                    if (largeIcon != IntPtr.Zero)
                    {
                        using var icon = System.Drawing.Icon.FromHandle(largeIcon);

                        // アイコンの元のサイズを取得
                        var originalSize = icon.Size;

                        // リサイズせずに元のアイコンをそのまま使用
                        var bitmap = ConvertIconToHighQualityBitmapImage(icon, originalSize);
                        return bitmap;
                    }
                }
            }
            else
            {
                // フォールバック: 標準のExtractAssociatedIconを使用
                var icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                if (icon != null)
                {
                    // リサイズせずに元のアイコンをそのまま使用
                    var originalSize = icon.Size;
                    var bitmap = ConvertIconToHighQualityBitmapImage(icon, originalSize);
                    return bitmap;
                }
            }
        }
        catch (Exception)
        {
            // アイコン抽出エラーは無視
        }

        return null;
    }


    /// <summary>
    /// アイコンを高品質なBitmapImageに変換
    /// </summary>
    /// <param name="icon">変換するアイコン</param>
    /// <param name="originalSize">元のアイコンサイズ</param>
    /// <returns>高品質なBitmapImage</returns>
    private BitmapImage ConvertIconToHighQualityBitmapImage(System.Drawing.Icon icon, System.Drawing.Size originalSize)
    {
        try
        {
            using var stream = new MemoryStream();
            icon.Save(stream);
            stream.Position = 0;

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            // リサイズせずに元のサイズでデコード
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
        catch (Exception)
        {
            return null!;
        }
    }

    /// <summary>
    /// デフォルトアイコンを取得
    /// </summary>
    private object GetDefaultIcon(string? browserName)
    {
        // デフォルトのブラウザアイコンを返す
        return "/BrowserSelector.Presentation;component/Resources/Images/Icon_Browser.png";
    }

    #region Win32 API
    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern int ExtractIconEx(string szFileName, int nIconIndex, out IntPtr phiconLarge, out IntPtr phiconSmall, int nIcons);
    #endregion
}