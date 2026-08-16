// <copyright file="IndexToHotkeyBadgeConverter.cs" company="BrowserSelector">
// Copyright (c) BrowserSelector. All rights reserved.
// </copyright>

using System.Globalization;
using System.Windows;
using System.Windows.Data;
using BrowserSelector.Presentation.Helpers;

namespace BrowserSelector.Presentation.Converters;

/// <summary>
/// <see cref="System.Windows.Controls.ItemsControl.AlternationIndex"/>（表示順インデックス）を
/// ホットキーバッジ文字（'1'-'9', 'A'-'Z'）へ変換する。バインド元は
/// <see cref="System.Windows.FrameworkElement.Tag"/> 経由で渡された整数インデックスを想定する（Phase C-3/C-4）.
/// <c>ConverterParameter="Visibility"</c> を指定した場合は、対応するバッジが無い（35個超）場合に
/// <see cref="Visibility.Collapsed"/> を返す（バッジ表示用Borderの可視性制御に使う）.
/// </summary>
public sealed class IndexToHotkeyBadgeConverter : IValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        char? badge = value is int index ? HotkeyResolver.GetBadgeForIndex(index) : null;

        bool wantsVisibility = parameter is string paramText && paramText.Equals("Visibility", StringComparison.OrdinalIgnoreCase);
        if (wantsVisibility)
        {
            return badge.HasValue ? Visibility.Visible : Visibility.Collapsed;
        }

        return badge?.ToString();
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
