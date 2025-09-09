using BrowserSelector.Core.Enums;
using System.Globalization;
using System.Windows.Data;

namespace BrowserSelector.Presentation.Converters;

/// <summary>
/// グラデーション方向の列挙型をコンボボックスのインデックスに変換するコンバーター.
/// </summary>
public class GradientDirectionToIndexConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is GradientDirection direction)
        {
            return (int)direction;
        }
        return 0; // デフォルトは垂直（インデックス0）
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int index)
        {
            return (GradientDirection)index;
        }
        return GradientDirection.Vertical; // デフォルトは垂直
    }
}
