using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using B3.Data;
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

        OfficialBanks.Clear();
        foreach (var group in problems.GroupBy(p => p.ExamType))
        {
            var problemIds = group.Select(p => p.ProblemId).ToHashSet();
            var answered = submissions.Where(s => problemIds.Contains(s.ProblemId))
                .Select(s => s.ProblemId)
                .Distinct()
                .Count();
            var progress = group.Count() == 0 ? 0 : (int)Math.Round(answered * 100.0 / group.Count());
            var updated = group.Max(p => p.UpdatedAt).ToString("yyyy-MM-dd");
            OfficialBanks.Add(new BankItem(
                $"{group.Key} 題庫",
                "使用中",
                "官方",
                group.Count(),
                updated,
                progress,
                ""
            ));
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
public class BankItem
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
}
