using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace B3.Converters
{
    public class DifficultyToLabelConverter : IValueConverter
    {
        public static DifficultyToLabelConverter Instance { get; } = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is int i)
            {
                return i switch
                {
                    1 => "簡單",
                    2 => "中等",
                    3 => "困難",
                    _ => "未知",
                };
            }
            return "未知";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string s)
            {
                return s switch
                {
                    "簡單" => 1,
                    "中等" => 2,
                    "困難" => 3,
                    _ => 0,
                };
            }

            return 0;
        }
    }
}
