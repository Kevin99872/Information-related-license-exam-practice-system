using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System;
using System.Linq;
using System.Diagnostics;
using Avalonia.Markup.Xaml;
using B3.ViewModels;
using B3.Views;

namespace B3;

public partial class App : Application
{
    public override void Initialize()
    {
        Debug.WriteLine("App.Initialize() 開始...");
        try
        {
            AvaloniaXamlLoader.Load(this);
            Debug.WriteLine("Xaml資源加載完成");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Xaml加載失敗: {ex.Message}");
            throw;
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Debug.WriteLine("App.OnFrameworkInitializationCompleted() 開始...");
        try
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var vm = new MainWindowViewModel();
                var mainWindow = new MainWindow
                {
                    DataContext = vm,
                };
                desktop.MainWindow = mainWindow;
                Debug.WriteLine("主窗口創建成功");
            }

            base.OnFrameworkInitializationCompleted();
            Debug.WriteLine("App 初始化完成");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"App 初始化失敗: {ex.Message}");
            Debug.WriteLine($"堆棧跟蹤: {ex.StackTrace}");
            throw;
        }
    }
}
