namespace B3.Models;

/// <summary>
/// 考試種類目錄 - 儲存可選題庫的考試種類與介紹名稱
/// 與 Problems 題庫分離，存放於獨立的 catalog.db
/// </summary>
public class ExamCategory
{
    public int ExamCategoryId { get; set; }

    /// <summary>考試種類代碼 - 對應 Problems.ExamType (TQC/CPE/APCS...)</summary>
    public string ExamType { get; set; } = string.Empty;

    /// <summary>顯示名稱 / 介紹名稱</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>介紹說明</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>標籤 (熱門 / 新增...)</summary>
    public string Tag { get; set; } = string.Empty;

    /// <summary>建議考試時間 (分鐘)</summary>
    public int DurationMinutes { get; set; }

    /// <summary>是否為熱門卡片 (true 顯示於熱門區，false 顯示於其他區)</summary>
    public bool IsHot { get; set; }

    /// <summary>排序順序</summary>
    public int SortOrder { get; set; }
}
