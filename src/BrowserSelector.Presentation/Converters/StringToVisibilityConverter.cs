// <copyright file="StringToVisibilityConverter.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BrowserSelector.Presentation.Converters;

/// <summary>
/// 文字列が空でない場合にVisibility.Visibleを返すコンバーター.
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool hasValue = value is string stringValue && !string.IsNullOrWhiteSpace(stringValue);
        return hasValue ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
