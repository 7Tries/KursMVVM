using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace KursMVVM.Converters;

public class StringToDateTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dt) return dt.ToString("yyyy-MM-dd", culture);
        return value?.ToString() ?? string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var str = value?.ToString();
        if (string.IsNullOrWhiteSpace(str)) return DateTime.Now;
        if (DateTime.TryParseExact(str, "yyyy-MM-dd", culture, DateTimeStyles.None, out var result)) return result;
        if (DateTime.TryParse(str, culture, DateTimeStyles.None, out result)) return result;
        return DateTime.Now;
    }
}
