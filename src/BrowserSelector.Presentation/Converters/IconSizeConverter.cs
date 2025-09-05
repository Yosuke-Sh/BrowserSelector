using System.Globalization;
using System.Windows.Data;

namespace BrowserSelector.Presentation.Converters;

/// <summary>
/// アイコンサイズを計算するコンバーター
/// テキスト表示無しの場合はアイコンを大きくする
/// </summary>
public class IconSizeConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not double baseSize || values[1] is not bool showBrowserName)
        {
            return 32.0; // デフォルトサイズ
        }

        // テキスト表示無しの場合はアイコンを1.5倍大きくする
        return showBrowserName ? baseSize : baseSize * 1.5;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
