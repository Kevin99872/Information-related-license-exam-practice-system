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
}
