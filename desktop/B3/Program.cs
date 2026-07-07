using Avalonia;
using Avalonia.Rendering;
using System;
using B3.Data;
using B3.Services;
using System.IO;

namespace B3;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            LoggerService.Log("應用啟動...");
            ExamDbContext.Initialize();
            ExamCatalogDbContext.Initialize();
            SeedTqcProblems();
        }
        catch (Exception ex)
        {
            LoggerService.LogError("數據庫初始化失敗", ex);
        }

        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            LoggerService.LogError("應用啟動失敗", ex);
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
#if WINDOWS
            // Prefer Skia GPU renderer on Windows for smoother GPU-accelerated rendering
            .UseSkia()
#endif
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace()
            // 提高渲染計時器頻率至 200fps，讓動畫與過渡更平滑 (實際顯示幀率仍受螢幕更新率限制)
            .AfterPlatformServicesSetup(_ => TrySetRenderTimerFps(200));

    /// <summary>
    /// 覆寫平台預設的渲染計時器頻率 (Windows 預設為 UiThreadRenderTimer 60fps)。
    /// Avalonia 12 的參考組件將 AvaloniaLocator.CurrentMutable 與 UiThreadRenderTimer(int) 隱藏，
    /// 但執行期為 public，故以反射存取；失敗時保留平台預設值。
    /// </summary>
    private static void TrySetRenderTimerFps(int fps)
    {
        try
        {
            var avaloniaBase = typeof(Avalonia.Rendering.IRenderTimer).Assembly;
            var timerType = avaloniaBase.GetType("Avalonia.Rendering.UiThreadRenderTimer");
            var locatorType = avaloniaBase.GetType("Avalonia.AvaloniaLocator");
            if (timerType == null || locatorType == null)
            {
                return;
            }

            var timer = Activator.CreateInstance(timerType, fps);
            var locator = locatorType.GetProperty("CurrentMutable")?.GetValue(null);
            if (timer == null || locator == null)
            {
                return;
            }

            var bindMethod = locatorType.GetMethod("Bind")?.MakeGenericMethod(typeof(Avalonia.Rendering.IRenderTimer));
            var helper = bindMethod?.Invoke(locator, null);
            helper?.GetType().GetMethod("ToConstant")?.MakeGenericMethod(timerType).Invoke(helper, new[] { timer });
            LoggerService.Log($"渲染計時器已設定為 {fps}fps");
        }
        catch (Exception ex)
        {
            LoggerService.LogWarning($"設定渲染幀率失敗，沿用平台預設值: {ex.Message}");
        }
    }

    /// <summary>
    /// 啟動時匯入TQC本地題庫
    /// </summary>
    private static void SeedTqcProblems()
    {
        try
        {
            var folder = FindSeedFolder("TQC-problem-list");
            if (string.IsNullOrWhiteSpace(folder))
            {
                LoggerService.LogWarning("找不到TQC-problem-list題庫資料夾");
                return;
            }

            var importer = new ProblemImportService();
            var count = importer.ImportIfEmptyAsync(folder).GetAwaiter().GetResult();
            if (count > 0)
            {
                LoggerService.Log($"已匯入TQC題庫: {count} 題");
            }
        }
        catch (Exception ex)
        {
            LoggerService.LogError("題庫匯入失敗", ex);
        }
    }

    /// <summary>
    /// 尋找相對路徑下的題庫資料夾
    /// </summary>
    /// <summary>由執行路徑往上搜尋題庫資料夾</summary>
    private static string? FindSeedFolder(string folderName)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 6 && current != null; i++)
        {
            var candidate = Path.Combine(current.FullName, folderName);
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
            current = current.Parent;
        }

        return null;
    }
}
