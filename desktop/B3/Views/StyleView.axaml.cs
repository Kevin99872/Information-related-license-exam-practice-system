using Avalonia.Controls;
using Avalonia;
using B3.ViewModels;
using B3.Services;
using B3.Models;

namespace B3.Views;

public partial class StyleView : UserControl
{
    public StyleView()
    {
        InitializeComponent();
    }

        private void OnRestoreClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (DataContext is not StyleViewModel vm) return;

            // 恢復到預設值
            vm.FontFamily = "Segoe UI Variable Text, Microsoft JhengHei UI, Noto Sans TC";
            vm.FontSize = 14;
            vm.ShowProgress = true;
            vm.SelectedTheme = vm.Themes.Count > 0 ? vm.Themes[0] : null;

            // SaveAndApply is invoked by property change handlers
        }
}
