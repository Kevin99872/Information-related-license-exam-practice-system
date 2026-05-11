using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using B3.Models;
using B3.Data;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace B3.ViewModels;

/// <summary>
/// 考試ViewModel - 管理考試流程 計時器 題目隨機抽選 答案判題
/// 職責: 控制考試狀態 呼叫TestCaseRepository進行自動判題
/// 綁定: ExamView
/// </summary>
public partial class ExamViewModel : ViewModelBase
{
    private readonly ProblemRepository _problemRepo;
    private readonly TestCaseRepository _testCaseRepo;
    private readonly UserSubmissionRepository _submissionRepo;
    private ExamDbContext _dbContext = null!;
    private System.Timers.Timer? _examTimer;

    /// <summary>當前考題</summary>
    [ObservableProperty]
    private Problem? currentProblem;

    /// <summary>當前考題的測試案例</summary>
    [ObservableProperty]
    private ObservableCollection<TestCase> currentTestCases = new();

    /// <summary>使用者提交的程式碼</summary>
    [ObservableProperty]
    private string userCode = string.Empty;

    /// <summary>執行輸出結果</summary>
    [ObservableProperty]
    private string outputResult = string.Empty;

    /// <summary>剩餘考試時間(秒)</summary>
    [ObservableProperty]
    private int remainingSeconds = 0;

    /// <summary>考試是否進行中</summary>
    [ObservableProperty]
    private bool isExamActive = false;

    /// <summary>目前已答題數</summary>
    [ObservableProperty]
    private int answeredCount = 0;

    /// <summary>總題數</summary>
    [ObservableProperty]
    private int totalProblems = 0;

    /// <summary>是否判題中</summary>
    [ObservableProperty]
    private bool isJudging = false;

    public ExamViewModel()
    {
        _dbContext = new ExamDbContext();
        _problemRepo = new ProblemRepository(_dbContext);
        _testCaseRepo = new TestCaseRepository(_dbContext);
        _submissionRepo = new UserSubmissionRepository(_dbContext);
    }

    /// <summary>開始考試 初始化計時器和題目</summary>
    public async Task StartExamAsync(int examDurationMinutes, string examType)
    {
        IsExamActive = true;
        RemainingSeconds = examDurationMinutes * 60;
        AnsweredCount = 0;

        // 載入該考照類型的所有題目
        var problems = await _problemRepo.GetByExamTypeAsync(examType);
        TotalProblems = problems.Count;

        // 隨機取得下一題
        await SelectNextProblemAsync(problems);

        // 啟動計時器
        _examTimer = new System.Timers.Timer(1000);
        _examTimer.Elapsed += (s, e) =>
        {
            RemainingSeconds--;
            if (RemainingSeconds <= 0)
            {
                // TODO: 時間到 自動結束考試
                EndExamCommand.Execute(null);
            }
        };
        _examTimer.Start();
    }

    /// <summary>隨機選取下一題</summary>
    private async Task SelectNextProblemAsync(System.Collections.Generic.List<Problem> problems)
    {
        if (problems.Count == 0) return;

        var random = new Random();
        CurrentProblem = problems[random.Next(problems.Count)];

        // 載入該題的示例測試案例
        var testCases = await _testCaseRepo.GetExamplesByProblemIdAsync(CurrentProblem.ProblemId);
        CurrentTestCases = new ObservableCollection<TestCase>(testCases);

        UserCode = string.Empty;
        OutputResult = string.Empty;
    }

    /// <summary>提交答案 進行自動判題</summary>
    public async Task SubmitAnswerAsync()
    {
        if (CurrentProblem == null) return;

        IsJudging = true;
        try
        {
            // 取得所有測試案例(含隱藏)
            var testCases = await _testCaseRepo.GetByProblemIdAsync(CurrentProblem.ProblemId);

            bool isCorrect = true;
            var output = new System.Text.StringBuilder();

            // TODO: 實現程式碼執行和輸出比對邏輯
            // 現階段僅模擬判題過程
            foreach (var testCase in testCases)
            {
                output.AppendLine($"Input: {testCase.Input}");
                output.AppendLine($"Expected: {testCase.ExpectedOutput}");
                output.AppendLine("---");
            }

            OutputResult = output.ToString();

            // 記錄提交
            var submission = new UserSubmission
            {
                ProblemId = CurrentProblem.ProblemId,
                UserCode = UserCode,
                IsCorrect = isCorrect,
                OutputResult = OutputResult
            };

            await _submissionRepo.SubmitAsync(submission);
            AnsweredCount++;
        }
        finally
        {
            IsJudging = false;
        }
    }

    /// <summary>跳過當前題目</summary>
    [RelayCommand]
    public void SkipProblem()
    {
        // TODO: 載入下一題
    }

    /// <summary>結束考試</summary>
    [RelayCommand]
    public void EndExam()
    {
        IsExamActive = false;
        _examTimer?.Stop();
        _examTimer?.Dispose();

        // TODO: 導向成績評測畫面
        Debug.WriteLine($"考試結束 答對: {AnsweredCount}/{TotalProblems}");
    }
}
