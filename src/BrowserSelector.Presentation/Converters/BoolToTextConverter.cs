using System.Globalization;
using System.Windows.Data;

namespace BrowserSelector.Presentation.Converters;

/// <summary>
/// bool値をテキストに変換するコンバーター
/// </summary>
public class BoolToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool boolValue ? boolValue ? "有効" : "無効" : "不明";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

