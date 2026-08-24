using Avalonia.Controls;
using Avalonia.Interactivity;
using B3.ViewModels;

namespace B3.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    /// <summary>畫面實際顯示時才觸發考照列表載入，而非在 ViewModel 建構時就預先查詢</summary>
    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is HomeViewModel vm)
        {
            await vm.EnsureCardsLoadedAsync();
        }
    }
}
