using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BrowserSelector.Presentation.Converters;

/// <summary>
/// Color型をBrush型に変換するコンバーター.
/// </summary>
public class ColorToBrushConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Color color)
        {
            return new SolidColorBrush(color);
        }

        // デフォルトの背景色
        return new SolidColorBrush(Colors.Transparent);
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SolidColorBrush brush)
        {
            return brush.Color;
        }

        // デフォルトの色
        return Colors.Transparent;
    }
}
