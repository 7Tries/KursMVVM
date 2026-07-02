using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace KursMVVM.Converters;

public class DateTimeToDateTimeOffsetConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dt) return new DateTimeOffset(dt);
        if (value is DateTimeOffset dto) return dto;
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTimeOffset dto) return dto.DateTime;
        if (value is DateTime dt) return dt;
        return DateTime.Now;
    }
}
