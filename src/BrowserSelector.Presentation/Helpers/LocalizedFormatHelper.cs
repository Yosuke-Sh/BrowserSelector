using BrowserSelector.Core.Services;
using System.Globalization;

namespace BrowserSelector.Presentation.Helpers;

/// <summary>
/// 多言語化対応のフォーマットヘルパークラス
/// </summary>
public static class LocalizedFormatHelper
{
    private static ILocalizationService? _localizationService;

    /// <summary>
    /// ローカライゼーションサービスを設定
    /// </summary>
    public static void SetLocalizationService(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    /// <summary>
    /// 現在のカルチャーを取得
    /// </summary>
    public static CultureInfo CurrentCulture => _localizationService?.CurrentCulture ?? CultureInfo.CurrentCulture;

    /// <summary>
    /// 数値を現在のカルチャーでフォーマット
    /// </summary>
    public static string FormatNumber(double value, string? format = null)
    {
        return value.ToString(format, CurrentCulture);
    }

    /// <summary>
    /// 数値を現在のカルチャーでフォーマット
    /// </summary>
    public static string FormatNumber(int value, string? format = null)
    {
        return value.ToString(format, CurrentCulture);
    }

    /// <summary>
    /// 日付を現在のカルチャーでフォーマット
    /// </summary>
    public static string FormatDate(DateTime value, string? format = null)
    {
        return value.ToString(format ?? "d", CurrentCulture);
    }

    /// <summary>
    /// 日時を現在のカルチャーでフォーマット
    /// </summary>
    public static string FormatDateTime(DateTime value, string? format = null)
    {
        return value.ToString(format ?? "g", CurrentCulture);
    }

    /// <summary>
    /// 時刻を現在のカルチャーでフォーマット
    /// </summary>
    public static string FormatTime(DateTime value, string? format = null)
    {
        return value.ToString(format ?? "t", CurrentCulture);
    }

    /// <summary>
    /// パーセンテージを現在のカルチャーでフォーマット
    /// </summary>
    public static string FormatPercentage(double value, int decimals = 0)
    {
        return value.ToString($"P{decimals}", CurrentCulture);
    }

    /// <summary>
    /// 通貨を現在のカルチャーでフォーマット
    /// </summary>
    public static string FormatCurrency(decimal value, string? currencyCode = null)
    {
        if (string.IsNullOrEmpty(currencyCode))
        {
            return value.ToString("C", CurrentCulture);
        }
        else
        {
            return value.ToString($"C", CurrentCulture);
        }
    }

    /// <summary>
    /// ファイルサイズを現在のカルチャーでフォーマット
    /// </summary>
    public static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{FormatNumber(len, "F2")} {sizes[order]}";
    }

    /// <summary>
    /// 期間を現在のカルチャーでフォーマット
    /// </summary>
    public static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalDays >= 1)
        {
            return $"{FormatNumber((int)duration.TotalDays)} days, {FormatNumber(duration.Hours)} hours";
        }
        else if (duration.TotalHours >= 1)
        {
            return $"{FormatNumber((int)duration.TotalHours)} hours, {FormatNumber(duration.Minutes)} minutes";
        }
        else if (duration.TotalMinutes >= 1)
        {
            return $"{FormatNumber((int)duration.TotalMinutes)} minutes, {FormatNumber(duration.Seconds)} seconds";
        }
        else
        {
            return $"{FormatNumber(duration.TotalSeconds, "F1")} seconds";
        }
    }
}
