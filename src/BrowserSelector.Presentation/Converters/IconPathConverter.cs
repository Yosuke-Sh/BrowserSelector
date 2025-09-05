using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;

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

            System.Diagnostics.Debug.WriteLine($"IconPathConverter: IconPath='{iconPath}', ExecutablePath='{executablePath}', Name='{name}'");

            // 1. IconPathが設定されている場合はそれを優先
            if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
            {
                System.Diagnostics.Debug.WriteLine($"IconPathConverter: IconPathを使用 - {iconPath}");
                
                // IconPathがexeファイルの場合は、高解像度アイコンを抽出
                if (iconPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || 
                    iconPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var bitmap = ExtractHighQualityIcon(iconPath, 0); // 最初のアイコンを取得
                        if (bitmap != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"IconPathConverter: IconPathから高解像度アイコン抽出成功");
                            return bitmap;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"IconPathConverter: IconPathアイコン抽出エラー: {ex.Message}");
                    }
                }
                else
                {
                    // 画像ファイル（.ico, .png等）の場合は直接読み込み
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"IconPathConverter: 画像ファイルを直接読み込み");
                        return new BitmapImage(new Uri(iconPath));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"IconPathConverter: 画像ファイル読み込みエラー: {ex.Message}");
                    }
                }
            }

            // 2. ExecutablePathから高解像度アイコンを抽出
            if (!string.IsNullOrEmpty(executablePath) && File.Exists(executablePath))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"IconPathConverter: ExecutablePathから高解像度アイコン抽出 - {executablePath}");
                    var bitmap = ExtractHighQualityIcon(executablePath, 0); // 最初のアイコンを取得
                    if (bitmap != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"IconPathConverter: 高解像度アイコン抽出成功");
                        return bitmap;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"IconPathConverter: アイコン抽出エラー: {ex.Message}");
                }
            }

            // 3. デフォルトアイコンを返す
            System.Diagnostics.Debug.WriteLine($"IconPathConverter: デフォルトアイコンを使用");
            return GetDefaultIcon(name);
        }
        catch (Exception ex)
        {
            // エラーが発生した場合はデフォルトアイコンを返す
            System.Diagnostics.Debug.WriteLine($"IconPathConverter: 全体エラー: {ex.Message}");
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
                        System.Diagnostics.Debug.WriteLine($"アイコン元サイズ: {originalSize.Width}x{originalSize.Height}");
                        
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ExtractHighQualityIcon エラー: {ex.Message}");
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"アイコン変換エラー: {ex.Message}");
            return null;
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