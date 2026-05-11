using Microsoft.EntityFrameworkCore;
using B3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B3.Data;

/// <summary>
/// 測試案例Repository - 管理TestCase表 實現2NF設計中的從表操作
/// 職責: 新增/修改/查詢/刪除TestCase 確保輸入輸出分離
/// </summary>
public class TestCaseRepository
{
    private readonly ExamDbContext _context;

    public TestCaseRepository(ExamDbContext context)
    {
        _context = context;
    }

    /// <summary>為指定題目新增測試案例</summary>
    public async Task<TestCase> AddAsync(TestCase testCase)
    {
        _context.TestCases.Add(testCase);
        await _context.SaveChangesAsync();
        return testCase;
    }

    /// <summary>批量新增測試案例</summary>
    public async Task<List<TestCase>> AddMultipleAsync(List<TestCase> testCases)
    {
        _context.TestCases.AddRange(testCases);
        await _context.SaveChangesAsync();
        return testCases;
    }

    /// <summary>取得指定題目的所有測試案例 按OrderIndex排序</summary>
    public async Task<List<TestCase>> GetByProblemIdAsync(int problemId)
    {
        return await _context.TestCases
            .Where(t => t.ProblemId == problemId)
            .OrderBy(t => t.OrderIndex)
            .ToListAsync();
    }

    /// <summary>取得示例測試案例 考試時向使用者顯示</summary>
    public async Task<List<TestCase>> GetExamplesByProblemIdAsync(int problemId)
    {
        return await _context.TestCases
            .Where(t => t.ProblemId == problemId && t.IsExample)
            .OrderBy(t => t.OrderIndex)
            .ToListAsync();
    }

    /// <summary>取得非示例的測試案例 用於自動判題</summary>
    public async Task<List<TestCase>> GetHiddenByProblemIdAsync(int problemId)
    {
        return await _context.TestCases
            .Where(t => t.ProblemId == problemId && !t.IsExample)
            .ToListAsync();
    }

    /// <summary>根據TestCaseId查詢單一案例</summary>
    public async Task<TestCase?> GetByIdAsync(int id)
    {
        return await _context.TestCases.FindAsync(id);
    }

    /// <summary>更新測試案例</summary>
    public async Task<TestCase> UpdateAsync(TestCase testCase)
    {
        _context.TestCases.Update(testCase);
        await _context.SaveChangesAsync();
        return testCase;
    }

    /// <summary>刪除指定測試案例</summary>
    public async Task DeleteAsync(int id)
    {
        var testCase = await _context.TestCases.FindAsync(id);
        if (testCase != null)
        {
            _context.TestCases.Remove(testCase);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>刪除題目下所有測試案例</summary>
    public async Task DeleteByProblemIdAsync(int problemId)
    {
        var testCases = await GetByProblemIdAsync(problemId);
        _context.TestCases.RemoveRange(testCases);
        await _context.SaveChangesAsync();
    }
}
