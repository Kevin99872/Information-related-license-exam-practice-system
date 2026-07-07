using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace B3.Localization;

/// <summary>
/// XAML 在地化標記擴充 - 用法: Text="{loc:Loc SettingsSubtitle}"
/// 繫結至 LocalizationService 索引子，語言切換時自動更新畫面
/// </summary>
public class LocExtension : MarkupExtension
{
    public LocExtension(string key)
    {
        Key = key;
    }

    public string Key { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding($"[{Key}]")
        {
            Mode = BindingMode.OneWay,
            Source = LocalizationService.Instance
        };
    }
}
