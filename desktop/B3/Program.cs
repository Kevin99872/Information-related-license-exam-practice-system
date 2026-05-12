using Avalonia;
using System;
using B3.Data;
using B3.Services;
using System.Diagnostics;
using System.IO;

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
            // 匯入本地TQC題庫
            SeedTqcProblems();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"數據庫初始化失敗: {ex.Message}");
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
                Debug.WriteLine("找不到TQC-problem-list題庫資料夾");
                return;
            }

            var importer = new ProblemImportService();
            var count = importer.ImportIfEmptyAsync(folder).GetAwaiter().GetResult();
            if (count > 0)
            {
                Debug.WriteLine($"已匯入TQC題庫: {count} 題");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"題庫匯入失敗: {ex.Message}");
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
