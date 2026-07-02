using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace KursMVVM.Converters;

public class StringToIntConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int i) return i.ToString(culture);
        return value?.ToString() ?? string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var str = value?.ToString();
        if (string.IsNullOrWhiteSpace(str)) return 0;
        if (int.TryParse(str, NumberStyles.Integer, culture, out var result)) return result;
        return 0;
    }
}
