using System.Globalization;
using System.Windows.Data;

namespace BrowserSelector.Presentation.Converters;

/// <summary>
/// Boolean値を反転するコンバーター
/// </summary>
public class InvertBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool boolValue && !boolValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool boolValue && !boolValue;
    }
}
