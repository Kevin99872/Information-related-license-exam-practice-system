using System;

namespace B3.Models;

/// <summary>
/// 用戶提交記錄模型 - 2NF正規化 SubmissionId為主鍵
/// 關聯: Problem(多:1)
/// 作用: 記錄用戶答題提交結果 用於成績統計和習題分析
/// </summary>
public class UserSubmission
{
    public int SubmissionId { get; set; }

    /// <summary>提交的題目ID - FK連接Problems表</summary>
    public int ProblemId { get; set; }

    /// <summary>用戶提交的程式碼</summary>
    public string UserCode { get; set; } = string.Empty;

    /// <summary>提交時間</summary>
    public DateTime SubmittedAt { get; set; }

    /// <summary>是否答題正確</summary>
    public bool IsCorrect { get; set; }

    /// <summary>執行輸出結果</summary>
    public string OutputResult { get; set; } = string.Empty;

    // 導航屬性
    public Problem Problem { get; set; } = null!;
}
