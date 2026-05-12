using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

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

    /// <summary>每行卡片數</summary>
    [ObservableProperty]
    private int cardsPerRow = 3;

    /// <summary>顯示進度條</summary>
    [ObservableProperty]
    private bool showProgress = true;

    public StyleViewModel()
    {
        Themes = new ObservableCollection<ThemeOption>
        {
            new("藍色", true),
            new("深綠", false),
            new("紫羅蘭", false)
        };
        SelectedTheme = Themes[0];
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
