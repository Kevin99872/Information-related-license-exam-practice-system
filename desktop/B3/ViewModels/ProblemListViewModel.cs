using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using B3.Models;
using B3.Data;
using B3.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace B3.ViewModels;

/// <summary>
/// 題目列表ViewModel - 管理題目瀏覽和篩選
/// 職責: 與ProblemRepository通訊 提供題目列表 支援篩選排序
/// 綁定: View層 MainWindow 和 ProblemListView
/// </summary>
public partial class ProblemListViewModel : ViewModelBase
{
    private readonly ProblemRepository _problemRepo;
    private ExamDbContext _dbContext = null!;

    /// <summary>題目列表集合 與View綁定</summary>
    [ObservableProperty]
    private ObservableCollection<Problem> problems = new();

    /// <summary>篩選考照類型 TQC/CPE/Leetcode</summary>
    [ObservableProperty]
    private string selectedExamType = "TQC";

    /// <summary>篩選題目狀態 Active/Draft等</summary>
    [ObservableProperty]
    private string selectedStatus = "Active";

    /// <summary>搜尋關鍵字</summary>
    [ObservableProperty]
    private string searchKeyword = string.Empty;

    /// <summary>是否正在載入</summary>
    [ObservableProperty]
    private bool isLoading = false;

    public ProblemListViewModel()
    {
        _dbContext = new ExamDbContext();
        _problemRepo = new ProblemRepository(_dbContext);
        
        LoggerService.LogDebug("ProblemListViewModel 初始化...");
        _ = LoadProblemsAsync();
    }

    /// <summary>初始化 載入所有題目</summary>
    public async Task InitializeAsync()
    {
        await LoadProblemsAsync();
    }

    /// <summary>從數據庫載入題目 支援篩選</summary>
    public async Task LoadProblemsAsync()
    {
        IsLoading = true;
        try
        {
            var allProblems = await _problemRepo.GetAllAsync();

            // 按考照類型篩選
            if (!string.IsNullOrEmpty(SelectedExamType) && SelectedExamType != "All")
            {
                allProblems = allProblems
                    .Where(p => p.ExamType == SelectedExamType)
                    .ToList();
            }

            // 按狀態篩選
            if (!string.IsNullOrEmpty(SelectedStatus) && SelectedStatus != "All")
            {
                allProblems = allProblems
                    .Where(p => p.Status == SelectedStatus)
                    .ToList();
            }

            // 按關鍵字搜尋
            if (!string.IsNullOrEmpty(SearchKeyword))
            {
                allProblems = allProblems
                    .Where(p => p.Title.Contains(SearchKeyword) || p.ProblemCode.Contains(SearchKeyword))
                    .ToList();
            }

            Problems = new ObservableCollection<Problem>(allProblems);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>篩選條件變更時重新載入</summary>
    partial void OnSelectedExamTypeChanged(string value)
    {
        _ = LoadProblemsAsync();
    }

    partial void OnSelectedStatusChanged(string value)
    {
        _ = LoadProblemsAsync();
    }

    partial void OnSearchKeywordChanged(string value)
    {
        _ = LoadProblemsAsync();
    }

    /// <summary>新增題目</summary>
    public async Task AddProblemAsync(Problem problem)
    {
        await _problemRepo.AddAsync(problem);
        await LoadProblemsAsync();
    }

    /// <summary>刪除題目</summary>
    public async Task DeleteProblemAsync(Problem problem)
    {
        // TODO: 確認對話框
        await _problemRepo.DeleteAsync(problem.ProblemId);
        await LoadProblemsAsync();
    }

    /// <summary>取得待審核題目</summary>
    public async Task GetPendingReviewsAsync()
    {
        IsLoading = true;
        try
        {
            var pending = await _problemRepo.GetPendingReviewAsync();
            Problems = new ObservableCollection<Problem>(pending);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
