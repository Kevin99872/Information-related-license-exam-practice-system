using System;
using System.Collections.Generic;

namespace B3.Models;

/// <summary>
/// 題目模型 - 2NF正規化 ProblemId為主鍵
/// 關聯: TestCases(1:多) ProblemReviews(1:多) UserSubmissions(1:多)
/// </summary>
public class Problem
{
    public int ProblemId { get; set; }

    /// <summary>題目代碼 - 英文索引 PYD101、PYD102等</summary>
    public string ProblemCode { get; set; } = string.Empty;

    /// <summary>題目標題</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>題目完整描述說明</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>考照類型 - TQC/CPE/Leetcode等</summary>
    public string ExamType { get; set; } = string.Empty;

    /// <summary>難度等級 - 1(簡單)/2(中級)/3(困難)</summary>
    public int Difficulty { get; set; }

    /// <summary>題目狀態 - Draft/Active/Archived</summary>
    public string Status { get; set; } = "Draft";

    /// <summary>建立時間</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>更新時間</summary>
    public DateTime UpdatedAt { get; set; }

    // 導航屬性
    public ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
    public ICollection<ProblemReview> Reviews { get; set; } = new List<ProblemReview>();
    public ICollection<UserSubmission> Submissions { get; set; } = new List<UserSubmission>();
}
