using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace KursMVVM.Converters;

public class StringToDecimalConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal d) return d.ToString("F2", culture);
        return value?.ToString() ?? string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var str = value?.ToString();
        if (string.IsNullOrWhiteSpace(str)) return 0m;
        if (decimal.TryParse(str, NumberStyles.Any, culture, out var result)) return result;
        return 0m;
    }
}
