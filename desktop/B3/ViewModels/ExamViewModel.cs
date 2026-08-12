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
    
    /// <summary>本次考試的題目清單（用於結算畫面顯示）</summary>
    [ObservableProperty]
    private ObservableCollection<Problem> examProblems = new();

    /// <summary>本次考試的提交記錄（用於結算畫面顯示）</summary>
    [ObservableProperty]
    private ObservableCollection<UserSubmission> examSubmissions = new();

    [ObservableProperty]
    private Problem? selectedExamProblem;

    [ObservableProperty]
    private UserSubmission? selectedSubmission;
    private readonly Random _random = new();
    private bool _suppressEditorSync;

    /// <summary>取消考試設定時的回呼</summary>
    public Action? RequestHomeNavigation { get; set; }

    /// <summary>考試結束時的回呼 - 切換到結算畫面</summary>
    public Action<ExamResultViewModel>? RequestResultNavigation { get; set; }

    /// <summary>當前考題</summary>
    [ObservableProperty]
    private Problem? currentProblem;

    /// <summary>當前考題的測試案例</summary>
    [ObservableProperty]
    private ObservableCollection<TestCase> currentTestCases = new();

    /// <summary>使用者提交的程式碼</summary>
    [ObservableProperty]
    private string userCode = string.Empty;

    /// <summary>IDE 輸入內容</summary>
    [ObservableProperty]
    private string input = string.Empty;

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

    /// <summary>是否顯示考前卡片</summary>
    [ObservableProperty]
    private bool isPreExamVisible = true;

    /// <summary>是否顯示考前準備倒數</summary>
    [ObservableProperty]
    private bool isPreparingExam = false;

    /// <summary>考前倒數秒數</summary>
    [ObservableProperty]
    private int preparationSeconds = 3;

    /// <summary>當前可用題庫數量</summary>
    [ObservableProperty]
    private int availableQuestionCount = 0;

    /// <summary>可設定的作答題數</summary>
    [ObservableProperty]
    private int editableQuestionCount = 20;

    /// <summary>考試計時預覽文字</summary>
    [ObservableProperty]
    private string examTimerPreviewText = "60:00";

    /// <summary>當前題目编號</summary>
    [ObservableProperty]
    private int currentProblemNumber = 0;

    /// <summary>格式化題目統計文字</summary>
    [ObservableProperty]
    private string problemCountText = "0/0";

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

    /// <summary>IDE 檔案清單</summary>
    [ObservableProperty]
    private ObservableCollection<EditorFileItem> editorFiles = new();

    /// <summary>目前編輯的檔案</summary>
    [ObservableProperty]
    private EditorFileItem? selectedEditorFile;

    /// <summary>編輯器內容緩衝</summary>
    [ObservableProperty]
    private string editorBuffer = string.Empty;

    /// <summary>編輯器行號</summary>
    [ObservableProperty]
    private string editorLineNumbers = "1";

    /// <summary>編輯器狀態文字</summary>
    [ObservableProperty]
    private string editorStatusText = string.Empty;

    /// <summary>目前檔名</summary>
    [ObservableProperty]
    private string activeEditorFileName = string.Empty;

    /// <summary>目前檔案說明</summary>
    [ObservableProperty]
    private string activeEditorFileDescription = string.Empty;

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

    /// <summary>考試標題</summary>
    [ObservableProperty]
    private string examTitle = "新考試";

    /// <summary>可修改的考試類型（考前卡片用）</summary>
    [ObservableProperty]
    private string editableExamType = "TQC";

    /// <summary>可修改的考試時間分鐘數（考前卡片用）</summary>
    [ObservableProperty]
    private int editableExamDurationMinutes = 60;

    /// <summary>考試卡片資訊</summary>
    [ObservableProperty]
    private ExamCard? selectedExamCard;

    /// <summary>目前考試種類的介紹說明 (取自 catalog.db)</summary>
    [ObservableProperty]
    private string examDescription = string.Empty;

    /// <summary>考試開始時間</summary>
    private DateTime _examStartTime;

    /// <summary>開始時剩餘秒數</summary>
    private int _examStartSeconds = 0;

    /// <summary>花費時間文字</summary>
    [ObservableProperty]
    private string elapsedTimeText = "0分0秒";

    /// <summary>分數等級</summary>
    [ObservableProperty]
    private string scoreGrade = "未開始";

    /// <summary>最近一次提交的判題狀態</summary>
    [ObservableProperty]
    private string lastSubmissionStatusText = "尚未提交";

    /// <summary>最近一次提交的判題細節</summary>
    [ObservableProperty]
    private string lastSubmissionDetailText = string.Empty;

    /// <summary>ExamViewModel 建構子</summary>
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
        UserCode = GetDefaultSourceTemplate(SelectedLanguage);
        Input = string.Empty;
        EditableQuestionCount = settings.QuestionsPerExam;
        InitializeEditorWorkspace();
    }

    /// <summary>開始考試命令</summary>
    [RelayCommand]
    public async Task StartExam()
    {
        if (EditableQuestionCount <= 0)
        {
            OutputResult = "請先選擇題數後再開始考試。";
            return;
        }

        await PrepareAndStartExamAsync(EditableExamDurationMinutes, EditableExamType, EditableQuestionCount);
    }

    /// <summary>從卡片啟動考試 - 由 HomeView 佥單</summary>
    public async Task StartExamFromCardAsync(ExamCard card)
    {
        SelectedExamCard = card;
        ExamTitle = card.Title;
        ExamType = card.ExamType;
        ExamDurationMinutes = card.DurationMinutes;
        IsExamActive = false;
        IsExamFinished = false;
        IsPreparingExam = false;
        OutputResult = string.Empty;
        LastSubmittedCode = string.Empty;
        LastProblemDescription = string.Empty;
        LastSubmissionStatusText = "尚未提交";
        LastSubmissionDetailText = string.Empty;
        CurrentProblem = null;
        CurrentTestCases = new ObservableCollection<TestCase>();

        // 設定可修改屬性爲預設值
        EditableExamType = card.ExamType;
        EditableExamDurationMinutes = card.DurationMinutes;
        EditableQuestionCount = Math.Max(1, card.QuestionCount);

        await LoadQuestionBankInfoAsync(card.ExamType);
        await LoadExamDescriptionAsync(card.ExamType);
        if (AvailableQuestionCount > 0)
        {
            EditableQuestionCount = Math.Min(EditableQuestionCount, AvailableQuestionCount);
        }
        RefreshPanelVisibility();
    }

    /// <summary>程式碼試跑 - 使用示例測資立即執行</summary>
    [RelayCommand]
    public async Task RunCodeAsync()
    {
        if (!IsExamActive || CurrentProblem == null)
        {
            OutputResult = "考試尚未開始。";
            return;
        }

        if (string.IsNullOrWhiteSpace(UserCode))
        {
            OutputResult = "請先輸入程式碼後再執行。";
            return;
        }

        IsJudging = true;
        try
        {
            var sampleCase = CurrentTestCases.FirstOrDefault();
            var sampleInput = sampleCase?.Input ?? string.Empty;
            var actual = await _judgeService.ExecuteAsync(SelectedLanguage, UserCode, sampleInput);

            var output = new System.Text.StringBuilder();
            output.AppendLine("Run Result");
            output.AppendLine($"Language: {SelectedLanguage}");
            output.AppendLine($"Input: {(string.IsNullOrEmpty(sampleInput) ? "(empty)" : sampleInput)}");
            output.AppendLine($"Actual: {actual}");

            if (sampleCase != null)
            {
                var passed = _judgeService.CompareOutput(actual, sampleCase.ExpectedOutput);
                output.AppendLine($"Expected: {sampleCase.ExpectedOutput}");
                output.AppendLine(passed ? "Status: OK" : "Status: WA");
            }

            OutputResult = output.ToString();
        }
        finally
        {
            IsJudging = false;
        }
    }

    /// <summary>考前準備流程 - 顯示倒數後開始考試</summary>
    [RelayCommand]
    public void CancelExamSetup()
    {
        IsPreparingExam = false;
        IsExamFinished = false;
        SelectedExamCard = null;
        OutputResult = string.Empty;
        RequestHomeNavigation?.Invoke();
    }

    /// <summary>考前準備流程 - 顯示倒數後開始考試</summary>
    private async Task PrepareAndStartExamAsync(int examDurationMinutes, string examType, int requestedQuestionCount)
    {
        if (IsPreparingExam || IsExamActive)
        {
            return;
        }

        if (AvailableQuestionCount == 0)
        {
            await LoadQuestionBankInfoAsync(examType);
        }

        if (AvailableQuestionCount == 0)
        {
            OutputResult = "尚未找到可用題庫，請先匯入題庫。";
            return;
        }

        EditableQuestionCount = Math.Max(1, Math.Min(requestedQuestionCount, AvailableQuestionCount));

        IsPreparingExam = true;
        for (var i = 3; i >= 1; i--)
        {
            PreparationSeconds = i;
            await Task.Delay(1000);
        }

        IsPreparingExam = false;
        await StartExamAsync(examDurationMinutes, examType, EditableQuestionCount);
    }

    /// <summary>開始考試 初始化計時器和題目</summary>
    public async Task StartExamAsync(int examDurationMinutes, string examType, int questionCount)
    {
        if (IsExamActive)
        {
            return;
        }

        // 載入該考照類型的所有題目
        var problems = await _problemRepo.GetByExamTypeAsync(examType);
        if (problems.Count == 0)
        {
            OutputResult = "尚未找到可用題庫，請先匯入題庫。";
            IsExamActive = false;
            return;
        }

        IsExamActive = true;
        IsExamFinished = false;
        RemainingSeconds = examDurationMinutes * 60;
        _examStartSeconds = RemainingSeconds; // 記錄開始時的剩餘秒數
        _examStartTime = DateTime.Now; // 記錄開始時間
        AnsweredCount = 0;
        CorrectCount = 0;
        CurrentProblemNumber = 0;
        ProblemCountText = "0/0";
        AiQuestion = string.Empty;
        AiAnswer = string.Empty;
        AiAnalysis = string.Empty;
        LastSubmittedCode = string.Empty;
        LastProblemDescription = string.Empty;
        LastSubmissionStatusText = "考試進行中";
        LastSubmissionDetailText = string.Empty;

        // 根據設定限制題數
        TotalProblems = Math.Max(1, Math.Min(questionCount, problems.Count));

        // 隨機選取指定數量的題目
        var random = new Random();
        _remainingProblems = problems.OrderBy(_ => random.Next()).Take(TotalProblems).ToList();

        // 設定本次考試題目集合供結算畫面顯示
        ExamProblems = new ObservableCollection<Problem>(_remainingProblems);

        // 初始化提交記錄集合
        ExamSubmissions = new ObservableCollection<UserSubmission>();

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

        // 更新當前題數與格式化文字
        CurrentProblemNumber = AnsweredCount + 1;
        ProblemCountText = $"{CurrentProblemNumber}/{TotalProblems}";

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

            bool isCorrect;
            var output = new System.Text.StringBuilder();

            if (testCases.Count == 0)
            {
                // 題目沒有任何測試資料時，無從驗證程式碼是否正確，
                // 不可視為「通過」，避免無測資的題目被誤判為正確。
                isCorrect = false;
                LastSubmissionStatusText = "判題結果：無法判題";
                LastSubmissionDetailText = $"{CurrentProblem.ProblemCode} 沒有任何測試資料，無法自動判題。請聯絡題庫維護者補上測試資料。";
                output.AppendLine(LastSubmissionStatusText);
                output.AppendLine(LastSubmissionDetailText);
            }
            else
            {
                string? firstFailureDetail = null;
                var caseIndex = 0;
                isCorrect = true;

                foreach (var testCase in testCases)
                {
                    caseIndex++;
                    var actual = await _judgeService.ExecuteAsync(SelectedLanguage, UserCode, testCase.Input);
                    var passed = _judgeService.CompareOutput(actual, testCase.ExpectedOutput);
                    if (!passed)
                    {
                        isCorrect = false;
                        if (firstFailureDetail == null)
                        {
                            firstFailureDetail = $"第 {caseIndex} 筆測資失敗\nInput: {testCase.Input}\nExpected: {testCase.ExpectedOutput}\nActual: {actual}";
                        }
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

                LastSubmissionStatusText = isCorrect ? "判題結果：正確" : "判題結果：錯誤";
                LastSubmissionDetailText = isCorrect
                    ? $"{CurrentProblem.ProblemCode} 已通過全部測資，共 {testCases.Count} 筆。"
                    : firstFailureDetail ?? $"{CurrentProblem.ProblemCode} 判定失敗，但未取得具體錯誤細節。";

                output.Insert(0, $"{LastSubmissionStatusText}\n");
                if (!isCorrect)
                {
                    output.AppendLine();
                    output.AppendLine(LastSubmissionDetailText);
                }
            }

            OutputResult = output.ToString().Trim();
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

            // Attach Problem navigation for later display in results
            submission.Problem = CurrentProblem;

            await _submissionRepo.SubmitAsync(submission);
            // 將提交加入本次考試的提交清單（放在最前面）
            ExamSubmissions.Insert(0, submission);
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

        // 計算花費時間
        var elapsedSeconds = _examStartSeconds - RemainingSeconds;
        var minutes = elapsedSeconds / 60;
        var seconds = elapsedSeconds % 60;
        ElapsedTimeText = $"{minutes}分{seconds}秒";

        // 計算分數等級
        CalculateScoreGrade();

        var resultViewModel = new ExamResultViewModel();
        resultViewModel.LoadFromExam(this);
        RequestResultNavigation?.Invoke(resultViewModel);

        // TODO: 導向成績評測畫面
        Debug.WriteLine($"考試結束 答對: {AnsweredCount}/{TotalProblems}");
    }

    /// <summary>計算分數等級</summary>
    private void CalculateScoreGrade()
    {
        if (TotalProblems == 0)
        {
            ScoreGrade = "未開始";
            return;
        }

        int percent = ScorePercent;
        ScoreGrade = percent switch
        {
            >= 90 => "優秀 ★★★",
            >= 80 => "良好 ★★",
            >= 70 => "及格 ★",
            >= 60 => "待加強",
            _ => "需努力"
        };
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
        RefreshPanelVisibility();
    }

    /// <summary>更新準備倒數狀態</summary>
    partial void OnIsPreparingExamChanged(bool value)
    {
        RefreshPanelVisibility();
    }

    /// <summary>更新考試完成狀態</summary>
    partial void OnIsExamFinishedChanged(bool value)
    {
        RefreshPanelVisibility();
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

    /// <summary>更新已答題數時購新題目數量文字</summary>
    partial void OnAnsweredCountChanged(int value)
    {
        if (TotalProblems > 0)
        {
            ProblemCountText = $"{value + 1}/{TotalProblems}";
        }
    }

    /// <summary>更新考試時間預覽</summary>
    partial void OnExamDurationMinutesChanged(int value)
    {
        ExamTimerPreviewText = $"{Math.Max(0, value):D2}:00";
    }

    /// <summary>語言改變時更新 IDE 檔案</summary>
    partial void OnSelectedLanguageChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(UserCode))
        {
            UserCode = GetDefaultSourceTemplate(value);
        }

        InitializeEditorWorkspace();
    }

    /// <summary>更新編輯器內容時同步目前檔案</summary>
    partial void OnEditorBufferChanged(string value)
    {
        if (_suppressEditorSync)
        {
            return;
        }

        if (SelectedEditorFile?.Kind == EditorFileKind.Source)
        {
            UserCode = value;
        }
        else if (SelectedEditorFile?.Kind == EditorFileKind.Input)
        {
            Input = value;
        }

        UpdateEditorLineNumbers(value);
        UpdateEditorStatus();
    }

    /// <summary>切換檔案時同步編輯器內容</summary>
    partial void OnSelectedEditorFileChanged(EditorFileItem? value)
    {
        SyncEditorBufferFromSelection();
    }

    /// <summary>更新考試類型時同步題庫資訊與不供選擇的考試類型</summary>
    partial void OnEditableExamTypeChanged(string value)
    {
        ExamType = value;
        _ = LoadQuestionBankInfoAsync(value);
        _ = LoadExamDescriptionAsync(value);
    }

    /// <summary>載入指定考試種類的介紹說明，若 catalog.db 已有相關說明則直接顯示</summary>
    public async Task LoadExamDescriptionAsync(string examType)
    {
        try
        {
            using var catalogContext = new ExamCatalogDbContext();
            var categoryRepo = new ExamCategoryRepository(catalogContext);
            var category = await categoryRepo.GetByExamTypeAsync(examType);

            if (category != null && !string.IsNullOrWhiteSpace(category.Description))
            {
                ExamDescription = category.Description;
            }
            else if (!string.IsNullOrWhiteSpace(SelectedExamCard?.Description))
            {
                // catalog 無資料時，沿用點擊卡片帶入的說明
                ExamDescription = SelectedExamCard!.Description;
            }
            else
            {
                ExamDescription = string.Empty;
            }
        }
        catch (Exception ex)
        {
            LoggerService.LogError("載入考試種類說明失敗", ex);
        }
    }

    /// <summary>更新可修改考試時間時同步預覽文字</summary>
    partial void OnEditableExamDurationMinutesChanged(int value)
    {
        ExamDurationMinutes = value;
        ExamTimerPreviewText = $"{Math.Max(0, value):D2}:00";
    }

    /// <summary>載入指定考試類型的題庫數量</summary>
    public async Task LoadQuestionBankInfoAsync(string examType)
    {
        var problems = await _problemRepo.GetByExamTypeAsync(examType);
        AvailableQuestionCount = problems.Count;
    }

    /// <summary>題庫數量變動時限制可選題數</summary>
    partial void OnAvailableQuestionCountChanged(int value)
    {
        if (value <= 0)
        {
            EditableQuestionCount = 0;
            return;
        }

        if (EditableQuestionCount <= 0 || EditableQuestionCount > value)
        {
            EditableQuestionCount = value;
        }
    }

    /// <summary>同步面板可見狀態</summary>
    private void RefreshPanelVisibility()
    {
        IsPreExamVisible = !IsExamActive && !IsExamFinished && !IsPreparingExam;
    }

    /// <summary>初始化 IDE 檔案與狀態</summary>
    private void InitializeEditorWorkspace()
    {
        var sourceFileName = GetSourceFileName(SelectedLanguage);
        EditorFiles = new ObservableCollection<EditorFileItem>
        {
            new(EditorFileKind.Source, sourceFileName, "主要程式檔", true),
            new(EditorFileKind.Input, "input.txt", "測試輸入資料", true)
        };

        var selectedKind = SelectedEditorFile?.Kind ?? EditorFileKind.Source;
        SelectedEditorFile = EditorFiles.FirstOrDefault(file => file.Kind == selectedKind) ?? EditorFiles.FirstOrDefault();

        SyncEditorBufferFromSelection();
    }

    /// <summary>將編輯器內容同步到目前檔案</summary>
    private void SyncEditorBufferFromSelection()
    {
        _suppressEditorSync = true;

        if (SelectedEditorFile?.Kind == EditorFileKind.Input)
        {
            EditorBuffer = Input;
        }
        else
        {
            EditorBuffer = UserCode;
        }

        ActiveEditorFileName = SelectedEditorFile?.Name ?? GetSourceFileName(SelectedLanguage);
        ActiveEditorFileDescription = SelectedEditorFile?.Description ?? "主要程式檔";
        UpdateEditorStatus();
        UpdateEditorLineNumbers(EditorBuffer);

        _suppressEditorSync = false;
    }

    /// <summary>更新編輯器狀態列</summary>
    private void UpdateEditorStatus()
    {
        var lineCount = GetLineCount(EditorBuffer);
        EditorStatusText = $"{SelectedLanguage} | {ActiveEditorFileName} | {lineCount} 行";
    }

    /// <summary>更新編輯器行號文字</summary>
    private void UpdateEditorLineNumbers(string text)
    {
        var lineCount = GetLineCount(text);
        var lines = Enumerable.Range(1, Math.Max(1, lineCount)).Select(number => number.ToString());
        EditorLineNumbers = string.Join(Environment.NewLine, lines);
    }

    /// <summary>取得預設範例程式</summary>
    private static string GetDefaultSourceTemplate(string language)
    {
        return language switch
        {
            "C#" => "using System;\n\nConsole.WriteLine(\"Hello World\");",
            "C++" => "#include <bits/stdc++.h>\nusing namespace std;\n\nint main() {\n    ios::sync_with_stdio(false);\n    cin.tie(nullptr);\n\n    cout << \"Hello World\" << endl;\n    return 0;\n}",
            _ => "print('Hello World')"
        };
    }

    /// <summary>取得對應語言的檔名</summary>
    private static string GetSourceFileName(string language)
    {
        return language switch
        {
            "C#" => "Program.cs",
            "C++" => "main.cpp",
            _ => "main.py"
        };
    }

    /// <summary>計算行數</summary>
    private static int GetLineCount(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 1;
        }

        return text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).Length;
    }
}

/// <summary>編輯器檔案類型</summary>
public enum EditorFileKind
{
    Source,
    Input
}

/// <summary>編輯器檔案項目</summary>
public class EditorFileItem
{
    public EditorFileItem(EditorFileKind kind, string name, string description, bool isEditable)
    {
        Kind = kind;
        Name = name;
        Description = description;
        IsEditable = isEditable;
    }

    public EditorFileKind Kind { get; }
    public string Name { get; }
    public string Description { get; }
    public bool IsEditable { get; }
}
