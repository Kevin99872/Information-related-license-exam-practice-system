using System;

namespace B3.Models;

/// <summary>
/// 題目審核記錄模型 - 2NF正規化 ReviewId為主鍵
/// 關聯: Problem(多:1)
/// 作用: 追蹤題目審核流程 Pending->Approved or Rejected
/// </summary>
public class ProblemReview
{
    public int ReviewId { get; set; }

    /// <summary>被審核的題目ID - FK連接Problems表</summary>
    public int ProblemId { get; set; }

    /// <summary>審核者名稱</summary>
    public string ReviewerName { get; set; } = string.Empty;

    /// <summary>審核狀態 - Pending/Approved/Rejected</summary>
    public string Status { get; set; } = "Pending";

    /// <summary>審核意見評論</summary>
    public string Comments { get; set; } = string.Empty;

    /// <summary>審核時間</summary>
    public DateTime? ReviewedAt { get; set; }

    // 導航屬性
    public Problem Problem { get; set; } = null!;
}
