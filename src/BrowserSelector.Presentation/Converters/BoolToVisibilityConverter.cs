using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BrowserSelector.Presentation.Converters;

/// <summary>
/// bool値をVisibilityに変換するコンバーター
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // Invertパラメータが指定されている場合は値を反転
            if (parameter is string param && param.Equals("Invert", StringComparison.OrdinalIgnoreCase))
            {
                boolValue = !boolValue;
            }

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            bool result = visibility == Visibility.Visible;

            // Invertパラメータが指定されている場合は値を反転
            if (parameter is string param && param.Equals("Invert", StringComparison.OrdinalIgnoreCase))
            {
                result = !result;
            }

            return result;
        }
        return false;
    }
}
