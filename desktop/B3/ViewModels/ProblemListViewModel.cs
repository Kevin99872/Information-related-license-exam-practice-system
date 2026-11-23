using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using B3.Models;
using B3.Data;
using B3.Services;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    private readonly CodeJudgeService _codeJudgeService = new();
    private ExamDbContext _dbContext = null!;

    private MainWindowViewModel? _mainVM;

    /// <summary>題目列表集合 與View綁定</summary>
    [ObservableProperty]
    private ObservableCollection<Problem> problems = new();

    /// <summary>選取的題目 用於顯示詳細</summary>
    [ObservableProperty]
    private Problem? selectedProblem;

    /// <summary>Code Space 工作區 (檔案清單 / 檔案管理 / 編輯內容) - 連結: CodeWorkspaceView</summary>
    public EditorWorkspace Workspace { get; } = new();

    /// <summary>執行狀態文字 (顯示於輸出區標題)</summary>
    [ObservableProperty]
    private string editorStatusText = string.Empty;

    /// <summary>是否正在執行程式 (執行中停用執行按鈕)</summary>
    [ObservableProperty]
    private bool isRunning;

    /// <summary>編譯器輸出文字（右下輸出）</summary>
    [ObservableProperty]
    private string compilerOutput = string.Empty;

    /// <summary>可選的解法語言</summary>
    [ObservableProperty]
    private ObservableCollection<string> availableLanguages = new();

    /// <summary>選擇的語言</summary>
    [ObservableProperty]
    private string selectedLanguage = "Python";

    /// <summary>考照類型篩選選項 - "All" 加上題庫中實際存在的類型 (動態產生，支援自行匯入的題庫)</summary>
    [ObservableProperty]
    private ObservableCollection<string> examTypeOptions = new() { "All" };

    /// <summary>篩選考照類型 All 或題庫中的類型</summary>
    [ObservableProperty]
    private string selectedExamType = "TQC";

    /// <summary>篩選難度 All/簡單/中等/困難</summary>
    [ObservableProperty]
    private string selectedDifficulty = "All";

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
        AvailableLanguages = new ObservableCollection<string> { "Python", "C#", "C++", "Java" };
        Workspace.Reset(SelectedLanguage);
        _ = LoadProblemsAsync();
    }

    /// <summary>設定父視窗 ViewModel 以便導航</summary>
    public void SetMainViewModel(MainWindowViewModel mainVM)
    {
        _mainVM = mainVM;
    }

    /// <summary>開始練習所選題目</summary>
    [RelayCommand]
    public async System.Threading.Tasks.Task StartPracticeAsync()
    {
        if (SelectedProblem == null)
        {
            LoggerService.LogWarning("未選取題目，無法開始練習");
            return;
        }

        if (_mainVM == null)
        {
            LoggerService.LogWarning("MainWindowViewModel 未設定，無法導航到考試");
            return;
        }

        var examVM = new ExamViewModel();
        examVM.CurrentProblem = SelectedProblem;
        _mainVM.CurrentView = examVM;
        await System.Threading.Tasks.Task.CompletedTask;
    }

    /// <summary>
    /// 直接對某題開始練習（供清單按鈕使用）
    /// </summary>
    [RelayCommand]
    public async System.Threading.Tasks.Task StartPracticeProblemAsync(Problem problem)
    {
        if (problem == null)
        {
            LoggerService.LogWarning("未提供題目參數，無法開始練習");
            return;
        }

        SelectedProblem = problem;
        await StartPracticeAsync();
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

            // 依題庫內容補齊考照類型選項 (只新增不移除，避免打斷目前的選取)
            foreach (var type in allProblems.Select(p => p.ExamType).Distinct().OrderBy(t => t))
            {
                if (!string.IsNullOrWhiteSpace(type) && !ExamTypeOptions.Contains(type))
                {
                    ExamTypeOptions.Add(type);
                }
            }

            // 預設的 TQC 不存在時改顯示全部，避免一進頁面就是空清單
            if (SelectedExamType == "TQC" && allProblems.Count > 0 && allProblems.All(p => p.ExamType != "TQC"))
            {
                SelectedExamType = "All";
                return;
            }

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

            // 按難度篩選 (All, 簡單=1, 中等=2, 困難=3)
            if (!string.IsNullOrEmpty(SelectedDifficulty) && SelectedDifficulty != "All")
            {
                int diff = SelectedDifficulty switch
                {
                    "簡單" => 1,
                    "中等" => 2,
                    "困難" => 3,
                    _ => -1
                };

                if (diff > 0)
                {
                    allProblems = allProblems
                        .Where(p => p.Difficulty == diff)
                        .ToList();
                }
            }

            // 按關鍵字搜尋
            if (!string.IsNullOrEmpty(SearchKeyword))
            {
                allProblems = allProblems
                    .Where(p => p.Title.Contains(SearchKeyword, StringComparison.OrdinalIgnoreCase)
                             || p.ProblemCode.Contains(SearchKeyword, StringComparison.OrdinalIgnoreCase))
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
        // 下拉選單重建項目時可能短暫寫回 null，忽略之
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        _ = LoadProblemsAsync();
    }

    partial void OnSelectedDifficultyChanged(string value)
    {
        _ = LoadProblemsAsync();
    }

    partial void OnSelectedStatusChanged(string value)
    {
        _ = LoadProblemsAsync();
    }

    partial void OnSelectedProblemChanged(Problem? value)
    {
        // 當選取題目改變時，載入其解法程式碼到編輯器（沒有則用範例程式），並清掉上一題的自訂檔案
        Workspace.Reset(SelectedLanguage, value?.SolutionCode);
        CompilerOutput = string.Empty;
        EditorStatusText = string.Empty;
    }

    /// <summary>語言改變時更新主要程式檔 (檔名 / 範例程式 / 語法著色)</summary>
    partial void OnSelectedLanguageChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            Workspace.SetLanguage(value);
        }
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

    /// <summary>執行程式 - 以 input.txt 內容作為標準輸入，自訂檔案一併寫入工作目錄</summary>
    [RelayCommand]
    public async Task CompileAsync()
    {
        if (IsRunning)
        {
            return;
        }

        var source = Workspace.SourceCode;
        if (string.IsNullOrWhiteSpace(source))
        {
            CompilerOutput = "錯誤: 程式碼為空";
            EditorStatusText = "執行失敗";
            return;
        }

        IsRunning = true;
        CompilerOutput = "執行中...";
        EditorStatusText = $"{SelectedLanguage} 執行中";

        try
        {
            var output = await _codeJudgeService.ExecuteAsync(SelectedLanguage, source, Workspace.InputText, Workspace.ExtraFiles);

            CompilerOutput = string.IsNullOrWhiteSpace(output)
                ? "（無輸出）"
                : output.Trim();

            EditorStatusText = $"{SelectedLanguage} 執行完成";
        }
        catch (Exception ex)
        {
            CompilerOutput = $"執行失敗：{ex.Message}";
            EditorStatusText = "執行失敗";
        }
        finally
        {
            IsRunning = false;
        }
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
