using Avalonia.Controls;
using Avalonia.Input;
using B3.ViewModels;

namespace B3.Views;

/// <summary>
/// 主視窗 - 側欄寬度動畫由 XAML 的 Width Transition 處理
/// 連結: MainWindowViewModel.SidebarWidth / IsSidebarExpanded
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
    }

    private void OnSidebarPointerEntered(object? sender, PointerEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SetSidebarHover(true);
        }
    }

    private void OnSidebarPointerExited(object? sender, PointerEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SetSidebarHover(false);
        }
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.UpdateResponsiveState(Bounds.Width);
        }
    }
}
