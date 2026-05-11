using Microsoft.EntityFrameworkCore;
using B3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B3.Data;

/// <summary>
/// 用戶提交Repository - 管理UserSubmission表 記錄答題結果和成績
/// 職責: 新增/查詢提交記錄 用於成績統計和習題分析
/// </summary>
public class UserSubmissionRepository
{
    private readonly ExamDbContext _context;

    public UserSubmissionRepository(ExamDbContext context)
    {
        _context = context;
    }

    /// <summary>記錄用戶提交 包含程式碼和執行結果</summary>
    public async Task<UserSubmission> SubmitAsync(UserSubmission submission)
    {
        submission.SubmittedAt = DateTime.Now;
        _context.UserSubmissions.Add(submission);
        await _context.SaveChangesAsync();
        return submission;
    }

    /// <summary>取得指定題目的所有提交記錄 用於統計</summary>
    public async Task<List<UserSubmission>> GetByProblemIdAsync(int problemId)
    {
        return await _context.UserSubmissions
            .Where(s => s.ProblemId == problemId)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();
    }

    /// <summary>取得指定題目的成功提交次數</summary>
    public async Task<int> GetSuccessCountAsync(int problemId)
    {
        return await _context.UserSubmissions
            .Where(s => s.ProblemId == problemId && s.IsCorrect)
            .CountAsync();
    }

    /// <summary>取得指定題目的總提交次數</summary>
    public async Task<int> GetTotalSubmitCountAsync(int problemId)
    {
        return await _context.UserSubmissions
            .Where(s => s.ProblemId == problemId)
            .CountAsync();
    }

    /// <summary>計算指定題目的通過率</summary>
    public async Task<decimal> GetSuccessRateAsync(int problemId)
    {
        var total = await GetTotalSubmitCountAsync(problemId);
        if (total == 0) return 0;

        var success = await GetSuccessCountAsync(problemId);
        return (decimal)success / total * 100;
    }

    /// <summary>根據SubmissionId查詢提交記錄</summary>
    public async Task<UserSubmission?> GetByIdAsync(int id)
    {
        return await _context.UserSubmissions
            .Include(s => s.Problem)
            .FirstOrDefaultAsync(s => s.SubmissionId == id);
    }

    /// <summary>取得最後提交的記錄 用於展示最新答案</summary>
    public async Task<UserSubmission?> GetLatestSubmissionAsync(int problemId)
    {
        return await _context.UserSubmissions
            .Where(s => s.ProblemId == problemId)
            .OrderByDescending(s => s.SubmittedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>取得指定時間範圍的提交記錄</summary>
    public async Task<List<UserSubmission>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.UserSubmissions
            .Where(s => s.SubmittedAt >= startDate && s.SubmittedAt <= endDate)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();
    }

    /// <summary>更新提交記錄 例如更新執行結果</summary>
    public async Task<UserSubmission> UpdateAsync(UserSubmission submission)
    {
        _context.UserSubmissions.Update(submission);
        await _context.SaveChangesAsync();
        return submission;
    }

    /// <summary>刪除提交記錄</summary>
    public async Task DeleteAsync(int id)
    {
        var submission = await _context.UserSubmissions.FindAsync(id);
        if (submission != null)
        {
            _context.UserSubmissions.Remove(submission);
            await _context.SaveChangesAsync();
        }
    }
}
