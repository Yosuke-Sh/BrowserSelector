using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BrowserSelector.Presentation.Converters;

/// <summary>
/// アイコンのマージンを計算するコンバーター
/// テキスト表示無しの場合はマージンを0にする.
/// </summary>
public class IconMarginConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool showBrowserName)
        {
            // テキスト表示無しの場合はマージンを0にする
            return showBrowserName ? new Thickness(0, 0, 0, 5) : new Thickness(0);
        }

        return new Thickness(0, 0, 0, 5); // デフォルトマージン
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
