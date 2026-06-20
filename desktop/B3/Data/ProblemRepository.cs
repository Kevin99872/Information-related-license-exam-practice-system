using Microsoft.EntityFrameworkCore;
using B3.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B3.Data;

/// <summary>
/// 題目Repository - 通用數據訪問模式 封裝問題表操作
/// 職責: 新增/修改/查詢/刪除Problem 維持2NF完整性
/// </summary>
public class ProblemRepository
{
    private readonly ExamDbContext _context;

    public ProblemRepository(ExamDbContext context)
    {
        _context = context;
    }

    /// <summary>新增題目 同時初始化關聯的TestCases和Reviews</summary>
    public async Task<Problem> AddAsync(Problem problem)
    {
        problem.CreatedAt = System.DateTime.Now;
        problem.UpdatedAt = System.DateTime.Now;
        _context.Problems.Add(problem);
        await _context.SaveChangesAsync();
        return problem;
    }

    /// <summary>取得所有題目 包含TestCases導航屬性</summary>
    public async Task<List<Problem>> GetAllAsync()
    {
        return await _context.Problems
            .Include(p => p.TestCases)
            .Include(p => p.Reviews)
            .ToListAsync();
    }

    /// <summary>根據題目ID查詢 包含完整的測試案例</summary>
    public async Task<Problem?> GetByIdAsync(int id)
    {
        return await _context.Problems
            .Include(p => p.TestCases)
            .Include(p => p.Reviews)
            .Include(p => p.Submissions)
            .FirstOrDefaultAsync(p => p.ProblemId == id);
    }

    /// <summary>根據題目代碼查詢 如PYD101</summary>
    public async Task<Problem?> GetByCodeAsync(string code)
    {
        return await _context.Problems
            .Include(p => p.TestCases)
            .FirstOrDefaultAsync(p => p.ProblemCode == code);
    }

    /// <summary>按考照類型過濾 如TQC/CPE</summary>
    public async Task<List<Problem>> GetByExamTypeAsync(string examType)
    {
        return await _context.Problems
            .Where(p => p.ExamType == examType && p.Status == "Active")
            .ToListAsync();
    }

    /// <summary>統計各考照類型的 Active 題目數量 - 供首頁題庫卡片顯示題數</summary>
    public async Task<Dictionary<string, int>> GetActiveCountByExamTypeAsync()
    {
        return await _context.Problems
            .Where(p => p.Status == "Active")
            .GroupBy(p => p.ExamType)
            .Select(g => new { ExamType = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ExamType, x => x.Count);
    }

    /// <summary>按狀態過濾 Draft/Active/Archived</summary>
    public async Task<List<Problem>> GetByStatusAsync(string status)
    {
        return await _context.Problems
            .Where(p => p.Status == status)
            .ToListAsync();
    }

    /// <summary>更新題目資訊</summary>
    public async Task<Problem> UpdateAsync(Problem problem)
    {
        problem.UpdatedAt = System.DateTime.Now;
        _context.Problems.Update(problem);
        await _context.SaveChangesAsync();
        return problem;
    }

    /// <summary>刪除題目 級聯刪除所有關聯TestCases/Reviews/Submissions</summary>
    public async Task DeleteAsync(int id)
    {
        var problem = await _context.Problems.FindAsync(id);
        if (problem != null)
        {
            _context.Problems.Remove(problem);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>取得待審核的題目</summary>
    public async Task<List<Problem>> GetPendingReviewAsync()
    {
        return await _context.Problems
            .Where(p => p.Reviews.Any(r => r.Status == "Pending"))
            .ToListAsync();
    }
}
