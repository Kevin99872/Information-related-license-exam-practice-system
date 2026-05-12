using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using B3.Models;
using B3.Data;
using B3.Services;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
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
    private readonly CodeJudgeService _judgeService = new();
    private readonly OllamaService _ollamaService = new();
    private readonly LocalSettingsService _settingsService = new();
    private ExamDbContext _dbContext = null!;
    private DispatcherTimer? _examTimer;
    private List<Problem> _remainingProblems = new();
    private readonly Random _random = new();

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

    /// <summary>格式化剩餘時間</summary>
    [ObservableProperty]
    private string remainingTimeText = "00:00";

    /// <summary>考試是否進行中</summary>
    [ObservableProperty]
    private bool isExamActive = false;

    /// <summary>考試尚未開始</summary>
    [ObservableProperty]
    private bool isExamInactive = true;

    /// <summary>目前已答題數</summary>
    [ObservableProperty]
    private int answeredCount = 0;

    /// <summary>總題數</summary>
    [ObservableProperty]
    private int totalProblems = 0;

    /// <summary>是否判題中</summary>
    [ObservableProperty]
    private bool isJudging = false;

    /// <summary>是否可提交</summary>
    [ObservableProperty]
    private bool isNotJudging = true;

    /// <summary>考試類型</summary>
    [ObservableProperty]
    private string examType = "TQC";

    /// <summary>考試時間(分鐘)</summary>
    [ObservableProperty]
    private int examDurationMinutes = 60;

    /// <summary>可選考照類型</summary>
    [ObservableProperty]
    private ObservableCollection<string> examTypes = new();

    /// <summary>可選語言</summary>
    [ObservableProperty]
    private ObservableCollection<string> languages = new();

    /// <summary>選取語言</summary>
    [ObservableProperty]
    private string selectedLanguage = "Python";

    /// <summary>答對題數</summary>
    [ObservableProperty]
    private int correctCount = 0;

    /// <summary>答對百分比</summary>
    [ObservableProperty]
    private int scorePercent = 0;

    /// <summary>考試是否完成</summary>
    [ObservableProperty]
    private bool isExamFinished = false;

    /// <summary>AI 問題</summary>
    [ObservableProperty]
    private string aiQuestion = string.Empty;

    /// <summary>AI 回答</summary>
    [ObservableProperty]
    private string aiAnswer = string.Empty;

    /// <summary>AI 分析</summary>
    [ObservableProperty]
    private string aiAnalysis = string.Empty;

    /// <summary>AI 忙碌中</summary>
    [ObservableProperty]
    private bool isAiBusy = false;

    /// <summary>AI 可用</summary>
    [ObservableProperty]
    private bool isAiNotBusy = true;

    /// <summary>上次提交程式</summary>
    [ObservableProperty]
    private string lastSubmittedCode = string.Empty;

    /// <summary>上次題目描述</summary>
    [ObservableProperty]
    private string lastProblemDescription = string.Empty;

    public ExamViewModel()
    {
        _dbContext = new ExamDbContext();
        _problemRepo = new ProblemRepository(_dbContext);
        _testCaseRepo = new TestCaseRepository(_dbContext);
        _submissionRepo = new UserSubmissionRepository(_dbContext);

        ExamTypes = new ObservableCollection<string>
        {
            "TQC",
            "CPE",
            "Leetcode"
        };

        Languages = new ObservableCollection<string>
        {
            "Python",
            "C#",
            "C++"
        };

        var settings = _settingsService.Load();
        SelectedLanguage = settings.DefaultLanguage;
    }

    /// <summary>開始考試命令</summary>
    [RelayCommand]
    public async Task StartExam()
    {
        await StartExamAsync(ExamDurationMinutes, ExamType);
    }

    /// <summary>開始考試 初始化計時器和題目</summary>
    public async Task StartExamAsync(int examDurationMinutes, string examType)
    {
        IsExamActive = true;
        IsExamFinished = false;
        RemainingSeconds = examDurationMinutes * 60;
        AnsweredCount = 0;
        CorrectCount = 0;
        AiQuestion = string.Empty;
        AiAnswer = string.Empty;
        AiAnalysis = string.Empty;
        LastSubmittedCode = string.Empty;
        LastProblemDescription = string.Empty;

        // 載入該考照類型的所有題目
        var problems = await _problemRepo.GetByExamTypeAsync(examType);
        if (problems.Count == 0)
        {
            OutputResult = "尚未找到可用題庫，請先匯入題庫。";
            IsExamActive = false;
            return;
        }

        TotalProblems = problems.Count;
        _remainingProblems = problems.ToList();

        // 隨機取得下一題
        await SelectNextProblemAsync();

        // 啟動計時器
        _examTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _examTimer.Tick += (_, _) =>
        {
            RemainingSeconds = Math.Max(0, RemainingSeconds - 1);
            if (RemainingSeconds == 0)
            {
                EndExamCommand.Execute(null);
            }
        };
        _examTimer.Start();
    }

    /// <summary>隨機選取下一題</summary>
    private async Task SelectNextProblemAsync()
    {
        if (_remainingProblems.Count == 0)
        {
            EndExamCommand.Execute(null);
            return;
        }

        var index = _random.Next(_remainingProblems.Count);
        CurrentProblem = _remainingProblems[index];
        _remainingProblems.RemoveAt(index);

        // 載入該題的示例測試案例
        var testCases = await _testCaseRepo.GetExamplesByProblemIdAsync(CurrentProblem.ProblemId);
        CurrentTestCases = new ObservableCollection<TestCase>(testCases);

        UserCode = string.Empty;
        OutputResult = string.Empty;
    }

    /// <summary>提交答案 進行自動判題</summary>
    [RelayCommand]
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

            foreach (var testCase in testCases)
            {
                var actual = await _judgeService.ExecuteAsync(SelectedLanguage, UserCode, testCase.Input);
                var passed = _judgeService.CompareOutput(actual, testCase.ExpectedOutput);
                if (!passed)
                {
                    isCorrect = false;
                }

                if (testCase.IsExample)
                {
                    output.AppendLine($"Input: {testCase.Input}");
                    output.AppendLine($"Expected: {testCase.ExpectedOutput}");
                    output.AppendLine($"Actual: {actual}");
                    output.AppendLine(passed ? "Result: OK" : "Result: WA");
                    output.AppendLine("---");
                }
            }

            OutputResult = output.ToString();
            if (isCorrect)
            {
                CorrectCount++;
            }

            // 記錄提交
            var submission = new UserSubmission
            {
                ProblemId = CurrentProblem.ProblemId,
                UserCode = UserCode,
                IsCorrect = isCorrect,
                OutputResult = OutputResult,
                SubmittedAt = DateTime.Now
            };

            await _submissionRepo.SubmitAsync(submission);
            AnsweredCount++;
            LastSubmittedCode = UserCode;
            LastProblemDescription = CurrentProblem.Description;
            await SelectNextProblemAsync();
        }
        finally
        {
            IsJudging = false;
        }
    }

    /// <summary>跳過當前題目</summary>
    [RelayCommand]
    public async Task SkipProblem()
    {
        await SelectNextProblemAsync();
    }

    /// <summary>結束考試</summary>
    [RelayCommand]
    public void EndExam()
    {
        IsExamActive = false;
        IsExamFinished = true;
        _examTimer?.Stop();
        CurrentProblem = null;
        CurrentTestCases = new ObservableCollection<TestCase>();

        // TODO: 導向成績評測畫面
        Debug.WriteLine($"考試結束 答對: {AnsweredCount}/{TotalProblems}");
    }

    /// <summary>AI 問答</summary>
    [RelayCommand]
    public async Task AskAiAsync()
    {
        if (string.IsNullOrWhiteSpace(AiQuestion))
        {
            return;
        }

        IsAiBusy = true;
        try
        {
            AiAnswer = await _ollamaService.AskAsync(AiQuestion);
        }
        finally
        {
            IsAiBusy = false;
        }
    }

    /// <summary>AI 程式碼分析</summary>
    [RelayCommand]
    public async Task AnalyzeCodeAsync()
    {
        if (string.IsNullOrWhiteSpace(LastSubmittedCode))
        {
            AiAnalysis = "尚無可分析的程式碼";
            return;
        }

        IsAiBusy = true;
        try
        {
            AiAnalysis = await _ollamaService.AnalyzeCodeAsync(LastSubmittedCode, LastProblemDescription);
        }
        finally
        {
            IsAiBusy = false;
        }
    }

    /// <summary>更新剩餘時間文字</summary>
    partial void OnRemainingSecondsChanged(int value)
    {
        var minutes = value / 60;
        var seconds = value % 60;
        RemainingTimeText = $"{minutes:D2}:{seconds:D2}";
    }

    /// <summary>更新考試狀態旗標</summary>
    partial void OnIsExamActiveChanged(bool value)
    {
        IsExamInactive = !value;
    }

    /// <summary>更新判題狀態</summary>
    partial void OnIsJudgingChanged(bool value)
    {
        IsNotJudging = !value;
    }

    /// <summary>更新 AI 狀態</summary>
    partial void OnIsAiBusyChanged(bool value)
    {
        IsAiNotBusy = !value;
    }

    /// <summary>更新答對率</summary>
    partial void OnCorrectCountChanged(int value)
    {
        ScorePercent = TotalProblems == 0 ? 0 : (int)Math.Round((double)value * 100 / TotalProblems);
    }

    /// <summary>更新答對率</summary>
    partial void OnTotalProblemsChanged(int value)
    {
        ScorePercent = value == 0 ? 0 : (int)Math.Round((double)CorrectCount * 100 / value);
    }
}
