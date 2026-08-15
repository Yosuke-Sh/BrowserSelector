using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using BrowserSelector.Core.Services;

namespace BrowserSelector.Presentation.Converters;

/// <summary>
/// アイコンパスの変換を行うコンバーター.
/// 実際の抽出・キャッシュ処理は <see cref="IIconCacheService"/> に委譲する.
/// </summary>
public class IconPathConverter : IMultiValueConverter
{
    /// <summary>
    /// デフォルトアイコンのパス.
    /// </summary>
    private const string DefaultIconPath = "/BrowserSelector.Presentation;component/Resources/Images/Icon_Browser.png";

    /// <summary>
    /// 表示目標のアイコンサイズ（ピクセル）.
    /// </summary>
    private const int DefaultIconSize = 48;

    private static IIconCacheService? _iconCacheService;

    /// <summary>
    /// アイコン抽出・キャッシュを担うサービスを設定します.
    /// XAMLリソースとして生成されるコンバーターはDI経由でインスタンス化できないため、
    /// アプリ起動時に静的に注入する.
    /// </summary>
    /// <param name="iconCacheService">アイコンキャッシュサービス.</param>
    public static void SetIconCacheService(IIconCacheService iconCacheService)
    {
        _iconCacheService = iconCacheService;
    }

    /// <inheritdoc/>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(values);

        // IconPath、ExecutablePath、Name、IconIndexの順で取得
        string? iconPath = values.Length > 0 ? values[0] as string : null;
        string? executablePath = values.Length > 1 ? values[1] as string : null;
        int iconIndex = values.Length > 3 && values[3] is int idx ? idx : 0;

        if (_iconCacheService == null)
        {
            return DefaultIconPath;
        }

        // 1. IconPathが設定されている場合はそれを優先
        if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
        {
            BitmapSource? bitmap = _iconCacheService.GetIcon(iconPath, iconIndex, DefaultIconSize);
            if (bitmap != null)
            {
                return bitmap;
            }
        }

        // 2. ExecutablePathからアイコンを取得
        if (!string.IsNullOrEmpty(executablePath) && File.Exists(executablePath))
        {
            BitmapSource? bitmap = _iconCacheService.GetIcon(executablePath, iconIndex, DefaultIconSize);
            if (bitmap != null)
            {
                return bitmap;
            }
        }

        // 3. デフォルトアイコンを返す
        return DefaultIconPath;
    }

    /// <inheritdoc/>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
