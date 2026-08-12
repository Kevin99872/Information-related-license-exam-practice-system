using Microsoft.EntityFrameworkCore;
using B3.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B3.Data;

/// <summary>
/// 考試種類 Repository - 存取 catalog.db 的考試種類目錄
/// </summary>
public class ExamCategoryRepository
{
    private readonly ExamCatalogDbContext _context;

    public ExamCategoryRepository(ExamCatalogDbContext context)
    {
        _context = context;
    }

    /// <summary>依排序取得所有考試種類</summary>
    public async Task<List<ExamCategory>> GetAllOrderedAsync()
    {
        return await _context.ExamCategories
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }

    /// <summary>依考試種類代碼取得單筆 (找不到回傳 null)</summary>
    public async Task<ExamCategory?> GetByExamTypeAsync(string examType)
    {
        return await _context.ExamCategories
            .FirstOrDefaultAsync(c => c.ExamType == examType);
    }

    /// <summary>
    /// 若指定的考試種類尚無目錄卡片資料，動態建立一筆最基本的卡片
    /// (供匯入題庫時自動呼叫，讓新題庫能自動出現在首頁，而不需要寫死預設清單)
    /// </summary>
    public async Task EnsureExistsAsync(string examType)
    {
        if (string.IsNullOrWhiteSpace(examType))
        {
            return;
        }

        var existing = await GetByExamTypeAsync(examType);
        if (existing != null)
        {
            return;
        }

        var hasAny = await _context.ExamCategories.AnyAsync();
        var nextSortOrder = hasAny
            ? await _context.ExamCategories.MaxAsync(c => c.SortOrder) + 1
            : 0;

        _context.ExamCategories.Add(new ExamCategory
        {
            ExamType = examType,
            Title = examType,
            Description = $"由匯入的題庫「{examType}」自動建立。",
            Tag = string.Empty,
            IsHot = false,
            DurationMinutes = 60,
            SortOrder = nextSortOrder
        });

        await _context.SaveChangesAsync();
    }
}
