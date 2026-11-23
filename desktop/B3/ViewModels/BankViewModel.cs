using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using B3.Data;
using B3.Models;
using B3.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace B3.ViewModels;

/// <summary>
/// 已載入題庫ViewModel - 題庫統計與列表
/// </summary>
public partial class BankViewModel : ViewModelBase
{
    private readonly ExamDbContext _dbContext = new();
    private readonly ProblemRepository _problemRepo;
    private readonly UserSubmissionRepository _submissionRepo;
    private readonly ProblemImportService _importService = new();
    private MainWindowViewModel? _mainViewModel;

    /// <summary>題庫統計</summary>
    [ObservableProperty]
    private int loadedBankCount = 0;

    /// <summary>題目總數</summary>
    [ObservableProperty]
    private int totalQuestionCount = 0;

    /// <summary>已作答題數</summary>
    [ObservableProperty]
    private int answeredQuestionCount = 0;

    /// <summary>整體正確率</summary>
    [ObservableProperty]
    private int accuracyPercent = 0;

    /// <summary>狀態訊息</summary>
    [ObservableProperty]
    private string statusMessage = string.Empty;

    /// <summary>題庫名稱搜尋關鍵字 (即時篩選官方題庫清單)</summary>
    [ObservableProperty]
    private string searchKeyword = string.Empty;

    /// <summary>未篩選的完整官方題庫清單</summary>
    private readonly System.Collections.Generic.List<BankItem> _allOfficialBanks = new();

    /// <summary>官方題庫清單</summary>
    [ObservableProperty]
    private ObservableCollection<BankItem> officialBanks = new();

    /// <summary>自訂題庫清單</summary>
    [ObservableProperty]
    private ObservableCollection<BankItem> customBanks = new();

    public BankViewModel()
    {
        _problemRepo = new ProblemRepository(_dbContext);
        _submissionRepo = new UserSubmissionRepository(_dbContext);
        OfficialBanks = new ObservableCollection<BankItem>();
        CustomBanks = new ObservableCollection<BankItem>();

        _ = LoadAsync();
    }

    public void SetMainViewModel(MainWindowViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }

    /// <summary>重新整理</summary>
    [RelayCommand]
    public async Task RefreshAsync()
    {
        await LoadAsync();
    }

    /// <summary>匯入 TQC 題庫</summary>
    [RelayCommand]
    public async Task ImportTqcAsync()
    {
        var folder = FindSeedFolder("TQC-problem-list");
        if (string.IsNullOrWhiteSpace(folder))
        {
            StatusMessage = "找不到 TQC-problem-list 資料夾";
            return;
        }

        var count = await _importService.ImportFromFolderAsync(folder);
        StatusMessage = count == 0 ? "沒有新增題目" : $"已匯入 {count} 題";
        await LoadAsync();
    }

    [RelayCommand]
    public void ToggleBankExpansion(BankItem? bank)
    {
        if (bank == null)
        {
            return;
        }

        bank.IsExpanded = !bank.IsExpanded;
    }

    [RelayCommand]
    public async Task DeleteProblemAsync(Problem? problem)
    {
        if (problem == null)
        {
            return;
        }

        await _problemRepo.DeleteAsync(problem.ProblemId);
        StatusMessage = $"已刪除題目 {problem.ProblemCode}";
        await LoadAsync();
    }

    [RelayCommand]
    public void EditProblem(Problem? problem)
    {
        if (problem == null || _mainViewModel == null)
        {
            return;
        }

        _mainViewModel.ShowImport(problem);
        StatusMessage = $"已開啟題目 {problem.ProblemCode} 供編輯";
    }

    /// <summary>載入題庫統計</summary>
    private async Task LoadAsync()
    {
        var problems = await _problemRepo.GetAllAsync();
        var submissions = await _submissionRepo.GetAllAsync();

        TotalQuestionCount = problems.Count;
        LoadedBankCount = problems.Select(p => p.ExamType).Distinct().Count();
        AnsweredQuestionCount = submissions.Count;
        AccuracyPercent = submissions.Count == 0
            ? 0
            : (int)Math.Round(submissions.Count(s => s.IsCorrect) * 100.0 / submissions.Count);
        StatusMessage = TotalQuestionCount == 0 ? "尚無題庫" : $"已載入 {TotalQuestionCount} 題";

        _allOfficialBanks.Clear();
        foreach (var group in problems.GroupBy(p => p.ExamType))
        {
            var problemIds = group.Select(p => p.ProblemId).ToHashSet();
            var answered = submissions.Where(s => problemIds.Contains(s.ProblemId))
                .Select(s => s.ProblemId)
                .Distinct()
                .Count();
            var progress = group.Count() == 0 ? 0 : (int)Math.Round(answered * 100.0 / group.Count());
            var updated = group.Max(p => p.UpdatedAt).ToString("yyyy-MM-dd");
            var bank = new BankItem(
                $"{group.Key} 題庫",
                "使用中",
                "官方",
                group.Count(),
                updated,
                progress,
                ""
            )
            {
                Problems = new ObservableCollection<Problem>(group.OrderBy(p => p.ProblemCode))
            };

            _allOfficialBanks.Add(bank);
        }

        ApplyBankFilter();
    }

    partial void OnSearchKeywordChanged(string value) => ApplyBankFilter();

    /// <summary>依搜尋關鍵字篩選題庫清單 (比對題庫名稱，不分大小寫)</summary>
    private void ApplyBankFilter()
    {
        var keyword = SearchKeyword?.Trim() ?? string.Empty;
        OfficialBanks.Clear();
        foreach (var bank in _allOfficialBanks)
        {
            if (keyword.Length == 0 || bank.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                OfficialBanks.Add(bank);
            }
        }
    }

    /// <summary>由執行路徑往上搜尋題庫資料夾</summary>
    private string? FindSeedFolder(string folderName)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 6 && current != null; i++)
        {
            var candidate = Path.Combine(current.FullName, folderName);
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
            current = current.Parent;
        }

        return null;
    }
}

/// <summary>
/// 題庫項目
/// </summary>
public partial class BankItem : ObservableObject
{
    public BankItem(string title, string status, string source, int questionCount, string updatedDate, int progressPercent, string accentColor)
    {
        Title = title;
        Status = status;
        Source = source;
        QuestionCount = questionCount;
        UpdatedDate = updatedDate;
        ProgressPercent = progressPercent;
        AccentColor = accentColor;
    }

    public string Title { get; }
    public string Status { get; }
    public string Source { get; }
    public int QuestionCount { get; }
    public string UpdatedDate { get; }
    public int ProgressPercent { get; }
    public string AccentColor { get; }

    [ObservableProperty]
    private bool isExpanded;

    [ObservableProperty]
    private ObservableCollection<Problem> problems = new();
}
