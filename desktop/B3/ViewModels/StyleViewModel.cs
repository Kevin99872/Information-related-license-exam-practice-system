using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace B3.ViewModels;

/// <summary>
/// 樣式設定ViewModel - 介面主題與字體
/// </summary>
public partial class StyleViewModel : ViewModelBase
{
    /// <summary>主題清單</summary>
    [ObservableProperty]
    private ObservableCollection<ThemeOption> themes = new();

    /// <summary>選取主題</summary>
    [ObservableProperty]
    private ThemeOption? selectedTheme;

    /// <summary>字體大小</summary>
    [ObservableProperty]
    private int fontSize = 14;

    /// <summary>介面字體</summary>
    [ObservableProperty]
    private string fontFamily = "Segoe UI Variable Text, Microsoft JhengHei UI, Noto Sans TC";

    /// <summary>顯示進度條</summary>
    [ObservableProperty]
    private bool showProgress = true;

    public StyleViewModel()
    {
        Themes = new ObservableCollection<ThemeOption>
        {
            new("深色", true),
            new("淺色", false),
            new("系統設定", false)
        };
        SelectedTheme = Themes[0];

        try
        {
            var svc = new B3.Services.LocalSettingsService();
            var settings = svc.Load();
            FontSize = settings.FontSize;
            FontFamily = settings.FontFamily;
            ShowProgress = settings.ShowProgress;
            var theme = Themes.FirstOrDefault(t => t.Name == settings.ThemeName);
            if (theme != null) SelectedTheme = theme;
        }
        catch
        {
            // ignore and keep defaults
        }
    }

    partial void OnFontSizeChanged(int value)
    {
        SaveAndApply();
    }

    partial void OnFontFamilyChanged(string value)
    {
        SaveAndApply();
    }

    partial void OnShowProgressChanged(bool value)
    {
        SaveAndApply();
    }

    partial void OnSelectedThemeChanged(ThemeOption? value)
    {
        SaveAndApply();
    }

    private void SaveAndApply()
    {
        try
        {
            var svc = new B3.Services.LocalSettingsService();
            var settings = svc.Load();
            settings.ThemeName = SelectedTheme?.Name ?? settings.ThemeName;
            settings.FontFamily = FontFamily;
            settings.FontSize = FontSize;
            settings.ShowProgress = ShowProgress;
            svc.Save(settings);

            if (Avalonia.Application.Current is B3.App app)
            {
                app.ApplySettings(settings);
            }
        }
        catch
        {
            // ignore save errors
        }
    }
}

/// <summary>
/// 主題選項
/// </summary>
public class ThemeOption
{
    public ThemeOption(string name, bool isDefault)
    {
        Name = name;
        IsDefault = isDefault;
    }

    public string Name { get; }
    public bool IsDefault { get; }
}
