using Avalonia.Data.Converters;
using B3.Localization;
using System;
using System.Globalization;

namespace B3.Converters
{
    public class DifficultyToLabelConverter : IValueConverter
    {
        public static DifficultyToLabelConverter Instance { get; } = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            var label = LocalizationService.T("DiffUnknown");
            if (value is int i)
            {
                label = i switch
                {
                    1 => LocalizationService.T("DiffEasy"),
                    2 => LocalizationService.T("DiffMedium"),
                    3 => LocalizationService.T("DiffHard"),
                    _ => LocalizationService.T("DiffUnknown"),
                };
            }

            // ConverterParameter 可指定格式字串鍵值 (例如 DifficultyFmt = "難度：{0}")
            if (parameter is string formatKey && !string.IsNullOrEmpty(formatKey))
            {
                return string.Format(LocalizationService.T(formatKey), label);
            }

            return label;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is string s)
            {
                return s switch
                {
                    "簡單" or "Easy" => 1,
                    "中等" or "Medium" => 2,
                    "困難" or "Hard" => 3,
                    _ => 0,
                };
            }

            return 0;
        }
    }
}
