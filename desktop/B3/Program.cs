using Avalonia;
using System;
using B3.Data;

namespace B3;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            // 初始化數據庫 第一次運行時自動建立
            ExamDbContext.Initialize();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"數據庫初始化失敗: {ex.Message}");
        }

        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"應用啟動失敗: {ex.Message}");
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
