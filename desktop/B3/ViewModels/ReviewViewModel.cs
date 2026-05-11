using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using B3.Models;
using B3.Data;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace B3.ViewModels;

/// <summary>
/// 題目審核ViewModel - 管理題目審核流程
/// 職責: 與ProblemReviewRepository通訊 處理審核邏輯 Pending->Approved/Rejected
/// 綁定: ReviewView
/// </summary>
public partial class ReviewViewModel : ViewModelBase
{
    private readonly ProblemReviewRepository _reviewRepo;
    private readonly ProblemRepository _problemRepo;
    private ExamDbContext _dbContext = null!;

    /// <summary>待審核的題目列表</summary>
    [ObservableProperty]
    private ObservableCollection<Problem> pendingProblems = new();

    /// <summary>當前正在審核的題目</summary>
    [ObservableProperty]
    private Problem? currentReviewProblem;

    /// <summary>當前審核記錄</summary>
    [ObservableProperty]
    private ProblemReview? currentReview;

    /// <summary>審核意見</summary>
    [ObservableProperty]
    private string reviewComments = string.Empty;

    /// <summary>審核者名稱</summary>
    [ObservableProperty]
    private string reviewerName = string.Empty;

    /// <summary>是否正在處理</summary>
    [ObservableProperty]
    private bool isProcessing = false;

    public ReviewViewModel()
    {
        _dbContext = new ExamDbContext();
        _reviewRepo = new ProblemReviewRepository(_dbContext);
        _problemRepo = new ProblemRepository(_dbContext);
    }

    /// <summary>載入所有待審核題目</summary>
    public async Task LoadPendingReviewsAsync()
    {
        IsProcessing = true;
        try
        {
            var pending = await _problemRepo.GetPendingReviewAsync();
            PendingProblems = new ObservableCollection<Problem>(pending);

            if (pending.Count > 0)
            {
                CurrentReviewProblem = pending[0];
                await LoadReviewDetailsAsync(CurrentReviewProblem.ProblemId);
            }
        }
        finally
        {
            IsProcessing = false;
        }
    }

    /// <summary>載入指定題目的審核詳情</summary>
    private async Task LoadReviewDetailsAsync(int problemId)
    {
        var reviews = await _reviewRepo.GetByProblemIdAsync(problemId);
        if (reviews.Count > 0)
        {
            CurrentReview = reviews[0];
            ReviewComments = CurrentReview.Comments;
        }
    }

    /// <summary>審核通過 更新狀態為Approved</summary>
    public async Task ApproveAsync()
    {
        if (CurrentReview == null) return;

        IsProcessing = true;
        try
        {
            await _reviewRepo.ApproveAsync(CurrentReview.ReviewId, ReviewerName);
            // 同時更新問題狀態為Active
            if (CurrentReviewProblem != null)
            {
                CurrentReviewProblem.Status = "Active";
                await _problemRepo.UpdateAsync(CurrentReviewProblem);
            }
            await LoadPendingReviewsAsync();
        }
        finally
        {
            IsProcessing = false;
        }
    }

    /// <summary>審核拒絕 更新狀態為Rejected並記錄評論</summary>
    public async Task RejectAsync()
    {
        if (CurrentReview == null) return;

        IsProcessing = true;
        try
        {
            await _reviewRepo.RejectAsync(CurrentReview.ReviewId, ReviewerName, ReviewComments);
            await LoadPendingReviewsAsync();
        }
        finally
        {
            IsProcessing = false;
        }
    }

    /// <summary>更新審核意見</summary>
    public async Task UpdateCommentsAsync()
    {
        if (CurrentReview == null) return;

        IsProcessing = true;
        try
        {
            await _reviewRepo.UpdateCommentsAsync(CurrentReview.ReviewId, ReviewComments);
        }
        finally
        {
            IsProcessing = false;
        }
    }

    /// <summary>選擇待審核題目</summary>
    public async Task SelectProblemAsync(Problem problem)
    {
        CurrentReviewProblem = problem;
        ReviewComments = string.Empty;
        await LoadReviewDetailsAsync(problem.ProblemId);
    }
}
