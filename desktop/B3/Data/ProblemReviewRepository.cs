using Microsoft.EntityFrameworkCore;
using B3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B3.Data;

/// <summary>
/// 題目審核Repository - 管理ProblemReview表 實現題目審核機制
/// 職責: 新增/修改/查詢審核記錄 追蹤Pending->Approved/Rejected流程
/// </summary>
public class ProblemReviewRepository
{
    private readonly ExamDbContext _context;

    public ProblemReviewRepository(ExamDbContext context)
    {
        _context = context;
    }

    /// <summary>新增審核記錄 初始狀態為Pending</summary>
    public async Task<ProblemReview> AddAsync(ProblemReview review)
    {
        review.Status = "Pending";
        _context.ProblemReviews.Add(review);
        await _context.SaveChangesAsync();
        return review;
    }

    /// <summary>取得指定題目的所有審核記錄</summary>
    public async Task<List<ProblemReview>> GetByProblemIdAsync(int problemId)
    {
        return await _context.ProblemReviews
            .Where(r => r.ProblemId == problemId)
            .OrderByDescending(r => r.ReviewedAt)
            .ToListAsync();
    }

    /// <summary>取得指定題目的最新審核記錄</summary>
    public async Task<ProblemReview?> GetLatestReviewAsync(int problemId)
    {
        return await _context.ProblemReviews
            .Where(r => r.ProblemId == problemId)
            .OrderByDescending(r => r.ReviewedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>取得所有待審核的題目審核記錄</summary>
    public async Task<List<ProblemReview>> GetPendingReviewsAsync()
    {
        return await _context.ProblemReviews
            .Where(r => r.Status == "Pending")
            .Include(r => r.Problem)
            .OrderBy(r => r.ReviewedAt)
            .ToListAsync();
    }

    /// <summary>取得指定狀態的審核記錄 Pending/Approved/Rejected</summary>
    public async Task<List<ProblemReview>> GetByStatusAsync(string status)
    {
        return await _context.ProblemReviews
            .Where(r => r.Status == status)
            .Include(r => r.Problem)
            .ToListAsync();
    }

    /// <summary>審核通過 更新狀態為Approved</summary>
    public async Task<ProblemReview> ApproveAsync(int reviewId, string reviewerName)
    {
        var review = await _context.ProblemReviews.FindAsync(reviewId);
        if (review != null)
        {
            review.Status = "Approved";
            review.ReviewerName = reviewerName;
            review.ReviewedAt = DateTime.Now;
            // TODO: 同時更新對應Problem的Status為Active
            _context.ProblemReviews.Update(review);
            await _context.SaveChangesAsync();
        }
        return review!;
    }

    /// <summary>審核拒絕 更新狀態為Rejected並記錄評論</summary>
    public async Task<ProblemReview> RejectAsync(int reviewId, string reviewerName, string comments)
    {
        var review = await _context.ProblemReviews.FindAsync(reviewId);
        if (review != null)
        {
            review.Status = "Rejected";
            review.ReviewerName = reviewerName;
            review.Comments = comments;
            review.ReviewedAt = DateTime.Now;
            _context.ProblemReviews.Update(review);
            await _context.SaveChangesAsync();
        }
        return review!;
    }

    /// <summary>更新審核意見</summary>
    public async Task<ProblemReview> UpdateCommentsAsync(int reviewId, string comments)
    {
        var review = await _context.ProblemReviews.FindAsync(reviewId);
        if (review != null)
        {
            review.Comments = comments;
            _context.ProblemReviews.Update(review);
            await _context.SaveChangesAsync();
        }
        return review!;
    }

    /// <summary>根據ReviewId查詢</summary>
    public async Task<ProblemReview?> GetByIdAsync(int id)
    {
        return await _context.ProblemReviews
            .Include(r => r.Problem)
            .FirstOrDefaultAsync(r => r.ReviewId == id);
    }

    /// <summary>刪除審核記錄</summary>
    public async Task DeleteAsync(int id)
    {
        var review = await _context.ProblemReviews.FindAsync(id);
        if (review != null)
        {
            _context.ProblemReviews.Remove(review);
            await _context.SaveChangesAsync();
        }
    }
}
