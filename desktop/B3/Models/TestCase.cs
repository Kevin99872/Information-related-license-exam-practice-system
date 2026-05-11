namespace B3.Models;

/// <summary>
/// 測試案例模型 - 2NF正規化 分離輸入輸出 TestCaseId為主鍵
/// 關聯: Problem(多:1)
/// 作用: 實現一個題目可對應多個輸入輸出案例
/// </summary>
public class TestCase
{
    public int TestCaseId { get; set; }

    /// <summary>所屬題目ID - FK連接Problems表</summary>
    public int ProblemId { get; set; }

    /// <summary>輸入數據</summary>
    public string Input { get; set; } = string.Empty;

    /// <summary>期望輸出</summary>
    public string ExpectedOutput { get; set; } = string.Empty;

    /// <summary>是否為示例 - 考試時顯示的參考案例</summary>
    public bool IsExample { get; set; }

    /// <summary>案例排序序號 - 決定顯示順序</summary>
    public int OrderIndex { get; set; }

    // 導航屬性
    public Problem Problem { get; set; } = null!;
}
