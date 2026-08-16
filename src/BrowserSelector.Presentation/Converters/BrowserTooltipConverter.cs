// <copyright file="BrowserTooltipConverter.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using System.Globalization;
using System.Windows.Data;
using BrowserSelector.Presentation.Helpers;

namespace BrowserSelector.Presentation.Converters;

/// <summary>
/// ブラウザ名・実行パス・起動引数から、多言語対応のツールチップ文字列を組み立てる（Phase C-3）.
/// </summary>
public sealed class BrowserTooltipConverter : IMultiValueConverter
{
    /// <inheritdoc/>
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Length != 3)
        {
            return string.Empty;
        }

        string name = values[0] as string ?? string.Empty;
        string executablePath = values[1] as string ?? string.Empty;
        string arguments = values[2] as string ?? string.Empty;

        return LocalizedLogHelper.GetString("MainWindow.BrowserTooltip", name, executablePath, arguments);
    }

    /// <inheritdoc/>
    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
