using System;
using System.Diagnostics;

namespace B3.Services;

/// <summary>
/// 統一日誌服務 - 集中管理應用程式日誌輸出
/// </summary>
public static class LoggerService
{
    /// <summary>記錄訊息</summary>
    public static void Log(string message)
    {
        Debug.WriteLine($"[INFO] {DateTime.Now:HH:mm:ss} - {message}");
    }

    /// <summary>記錄錯誤</summary>
    public static void LogError(string message, Exception? ex = null)
    {
        Debug.WriteLine($"[ERROR] {DateTime.Now:HH:mm:ss} - {message}");
        if (ex != null)
        {
            Debug.WriteLine($"[ERROR] 例外: {ex.Message}");
            Debug.WriteLine($"[ERROR] 堆棧跟蹤: {ex.StackTrace}");
        }
    }

    /// <summary>記錄警告</summary>
    public static void LogWarning(string message)
    {
        Debug.WriteLine($"[WARN] {DateTime.Now:HH:mm:ss} - {message}");
    }

    /// <summary>記錄調試訊息</summary>
    public static void LogDebug(string message)
    {
#if DEBUG
        Debug.WriteLine($"[DEBUG] {DateTime.Now:HH:mm:ss} - {message}");
#endif
    }
}
