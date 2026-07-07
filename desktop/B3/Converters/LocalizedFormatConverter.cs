using Avalonia.Data.Converters;
using B3.Localization;
using System;
using System.Globalization;

namespace B3.Converters;

/// <summary>
/// 在地化格式字串轉換器 - 以 ConverterParameter 指定字串表鍵值
/// 用法: Text="{Binding QuestionCount, Converter={StaticResource LocFmt}, ConverterParameter=QuestionsFmt}"
/// </summary>
public class LocalizedFormatConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        if (parameter is not string key || string.IsNullOrEmpty(key))
        {
            return value?.ToString() ?? string.Empty;
        }

        var format = LocalizationService.T(key);
        try
        {
            return string.Format(format, value);
        }
        catch (FormatException)
        {
            return format;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotSupportedException();
    }
}
