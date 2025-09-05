using System.Globalization;
using System.Windows.Data;

namespace BrowserSelector.Presentation.Converters;

/// <summary>
/// 文字列が空でない場合にtrueを返すコンバーター
/// </summary>
public class StringToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            return !string.IsNullOrWhiteSpace(stringValue);
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

