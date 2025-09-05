using BrowserSelector.Core.Models;
using System.Globalization;
using System.Windows.Data;

namespace BrowserSelector.Presentation.Converters;

/// <summary>
/// ブラウザタイプに基づいて編集・削除ボタンの有効/無効を制御するコンバーター
/// </summary>
public class BrowserTypeToEditEnabledConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is BrowserType browserType)
        {
            // すべてのブラウザを編集可能にする
            return true;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
