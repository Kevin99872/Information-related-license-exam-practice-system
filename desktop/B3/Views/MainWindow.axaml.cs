using Avalonia.Controls;
using Avalonia;
using B3.ViewModels;
using Avalonia.Input;

namespace B3.Views;

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