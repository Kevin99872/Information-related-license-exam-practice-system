using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System;
using System.Linq;
using Avalonia.Markup.Xaml;
using B3.ViewModels;
using B3.Views;
using B3.Services;

namespace B3;

public partial class App : Application
{
    public override void Initialize()
    {
        LoggerService.LogDebug("App.Initialize() 開始...");
        try
        {
            AvaloniaXamlLoader.Load(this);
            LoggerService.LogDebug("Xaml資源加載完成");
        }
        catch (Exception ex)
        {
            LoggerService.LogError("Xaml加載失敗", ex);
            throw;
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        LoggerService.LogDebug("App.OnFrameworkInitializationCompleted() 開始...");
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
                LoggerService.LogDebug("主窗口創建成功");
            }

            base.OnFrameworkInitializationCompleted();
            LoggerService.Log("App 初始化完成");
        }
        catch (Exception ex)
        {
            LoggerService.LogError("App 初始化失敗", ex);
            throw;
        }
    }
}
