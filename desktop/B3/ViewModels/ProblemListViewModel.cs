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

    /// <summary>程式編輯器內容（右下）</summary>
    [ObservableProperty]
    private ObservableCollection<EditorFileItem> editorFiles = new();

    /// <summary>目前編輯的檔案</summary>
    [ObservableProperty]
    private EditorFileItem? selectedEditorFile;

    /// <summary>程式編輯器內容（右下）</summary>
    [ObservableProperty]
    private string editorBuffer = string.Empty;

    /// <summary>編輯器行號</summary>
    [ObservableProperty]
    private string editorLineNumbers = "1";

    /// <summary>編輯器狀態文字</summary>
    [ObservableProperty]
    private string editorStatusText = "請選擇題目後開始編輯";

    /// <summary>相容舊欄位：程式編輯器內容</summary>
    [ObservableProperty]
    private string codeText = string.Empty;

    /// <summary>編譯器輸出文字（右下輸出）</summary>
    [ObservableProperty]
    private string compilerOutput = string.Empty;

    /// <summary>可選的解法語言</summary>
    [ObservableProperty]
    private ObservableCollection<string> availableLanguages = new();

    /// <summary>選擇的語言</summary>
    [ObservableProperty]
    private string selectedLanguage = "Python";

    /// <summary>編輯器提示文字</summary>
    [ObservableProperty]
    private string selectedEditorHint = "請選擇題目";

    /// <summary>篩選考照類型 TQC/CPE/Leetcode</summary>
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
        InitializeEditorWorkspace();
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
        // 當選取題目改變時，載入其解法程式碼到編輯器（如果有）
        EditorBuffer = value?.SolutionCode ?? GetDefaultSourceTemplate(SelectedLanguage);
        CodeText = EditorBuffer;
        CompilerOutput = string.Empty;
        SelectedEditorHint = value == null ? "請選擇題目" : $"目前題目：{value.ProblemCode}";
        EditorStatusText = value == null ? "請選擇題目後開始編輯" : $"{SelectedEditorFile?.Name ?? "main.py"} · {SelectedLanguage}";
    }

    partial void OnEditorBufferChanged(string value)
    {
        CodeText = value;
        EditorLineNumbers = BuildLineNumbers(value);
    }

    partial void OnSelectedEditorFileChanged(EditorFileItem? value)
    {
        EditorStatusText = value == null
            ? "請選擇題目後開始編輯"
            : $"{value.Name} · {SelectedLanguage}";
    }

    partial void OnSelectedLanguageChanged(string value)
    {
        EditorStatusText = SelectedEditorFile == null
            ? $"{value} 編輯器"
            : $"{SelectedEditorFile.Name} · {value}";
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

    /// <summary>模擬編譯命令 - 實際可接入編譯服務</summary>
    [RelayCommand]
    public async Task CompileAsync()
    {
        var source = EditorBuffer;
        if (string.IsNullOrWhiteSpace(source))
        {
            CompilerOutput = "錯誤: 程式碼為空";
            EditorStatusText = "編譯失敗";
            return;
        }

        CompilerOutput = "執行中...";
        EditorStatusText = $"{SelectedLanguage} 執行中";

        try
        {
            var output = await _codeJudgeService.ExecuteAsync(SelectedLanguage, source, string.Empty);

            if (string.IsNullOrWhiteSpace(output) && SelectedLanguage == "Python")
            {
                output = EvaluateSimplePythonPrint(source);
            }

            CompilerOutput = string.IsNullOrWhiteSpace(output)
                ? "（無輸出）"
                : output.Trim();

            EditorStatusText = $"{SelectedLanguage} 執行完成";
        }
        catch (Exception ex)
        {
            var fallback = SelectedLanguage == "Python" ? EvaluateSimplePythonPrint(source) : string.Empty;
            CompilerOutput = !string.IsNullOrWhiteSpace(fallback)
                ? fallback.Trim()
                : $"執行失敗：{ex.Message}";
            EditorStatusText = "編譯失敗";
        }
    }

    private void InitializeEditorWorkspace()
    {
        EditorFiles = new ObservableCollection<EditorFileItem>
        {
            new(EditorFileKind.Source, "main.py", "主要程式碼", true)
        };

        SelectedEditorFile = EditorFiles.FirstOrDefault();
        EditorBuffer = GetDefaultSourceTemplate(SelectedLanguage);
        EditorLineNumbers = BuildLineNumbers(EditorBuffer);
        EditorStatusText = SelectedEditorFile == null
            ? "請選擇題目後開始編輯"
            : $"{SelectedEditorFile.Name} · {SelectedLanguage}";
    }

    private string BuildLineNumbers(string text)
    {
        var count = Math.Max(1, text.Replace("\r\n", "\n").Split('\n').Length);
        var builder = new StringBuilder();

        for (var i = 1; i <= count; i++)
        {
            builder.AppendLine(i.ToString());
        }

        return builder.ToString().TrimEnd();
    }

    private string GetDefaultSourceTemplate(string language)
    {
        return language switch
        {
            "Python" => "print(\"hello world\")",
            "C#" => "using System;\n\nConsole.WriteLine(\"hello world\");",
            "C++" => "#include <iostream>\nusing namespace std;\n\nint main() {\n    cout << \"hello world\" << endl;\n    return 0;\n}",
            _ => string.Empty
        };
    }

    private string EvaluateSimplePythonPrint(string source)
    {
        var lines = source.Replace("\r\n", "\n").Split('\n');
        var output = new StringBuilder();

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (!line.StartsWith("print(", StringComparison.Ordinal))
            {
                continue;
            }

            var match = Regex.Match(line, @"^print\s*\((?<expr>.*)\)\s*;?$", RegexOptions.Singleline);
            if (!match.Success)
            {
                continue;
            }

            var expr = match.Groups["expr"].Value.Trim();
            output.AppendLine(UnquotePythonString(expr));
        }

        return output.ToString().TrimEnd();
    }

    private string UnquotePythonString(string expr)
    {
        if (expr.Length >= 2 && ((expr.StartsWith('"') && expr.EndsWith('"')) || (expr.StartsWith('\'') && expr.EndsWith('\''))))
        {
            var content = expr[1..^1]
                .Replace("\\n", "\n")
                .Replace("\\t", "\t")
                .Replace("\\r", "\r")
                .Replace("\\\"", "\"")
                .Replace("\\'", "'")
                .Replace("\\\\", "\\");
            return content;
        }

        return expr;
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
