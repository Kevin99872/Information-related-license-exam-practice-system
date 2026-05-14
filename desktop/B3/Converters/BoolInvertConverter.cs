using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace B3.Converters;

/// <summary>
/// 布尔反轉轉換器 - 用於反轉布爾值 避免重複的IsNotXyz屬性
/// </summary>
public class BoolInvertConverter : IValueConverter
{
    public static BoolInvertConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        return value is bool b ? !b : false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        return value is bool b ? !b : false;
    }
}
