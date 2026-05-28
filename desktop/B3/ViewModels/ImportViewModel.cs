using B3.Models;
using B3.Services;
using B3.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B3.ViewModels;

/// <summary>
/// 匯入ViewModel - 題目單題輸入、批次表單預覽與匯入
/// </summary>
public partial class ImportViewModel : ViewModelBase
{
    private readonly ProblemImportService _importService = new();
    private readonly ProblemRepository _problemRepo;
    private ExamDbContext _dbContext = null!;

    [ObservableProperty]
    private string supportedFormats = "CSV / XLS / XLSX / TXT";

    [ObservableProperty]
    private string importStatusMessage = "請先填寫單題表單或選擇 CSV/XLS/XLSX 檔案。";

    [ObservableProperty]
    private string spreadsheetFilePath = string.Empty;

    [ObservableProperty]
    private string templateSpecification = "欄位順序：ProblemCode, ExamType, Title, Description, Difficulty, Status, SolutionLanguage, SolutionCode, OrderIndex, IsExample, TestInput, ExpectedOutput";

    [ObservableProperty]
    private string templateNotes = "批次匯入時，每一列代表一筆測試資料；相同 ProblemCode 的列會合併成同一題。";

    [ObservableProperty]
    private string singleProblemCode = string.Empty;

    [ObservableProperty]
    private string singleExamType = "TQC";

    [ObservableProperty]
    private string singleTitle = string.Empty;

    [ObservableProperty]
    private string singleDescription = string.Empty;

    [ObservableProperty]
    private int singleDifficulty = 1;

    [ObservableProperty]
    private string singleStatus = "Draft";

    [ObservableProperty]
    private string singleSolutionLanguage = "Python";

    [ObservableProperty]
    private string singleSolutionCode = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ProblemImportTestCaseModel> singleTestCases = new();

    [ObservableProperty]
    private ObservableCollection<ProblemImportSpreadsheetRow> previewRows = new();

    [ObservableProperty]
    private ObservableCollection<ProblemImportValidationIssue> validationIssues = new();

    [ObservableProperty]
    private bool isBusy;

    public ImportViewModel()
    {
        ResetSingleForm();
        _dbContext = new ExamDbContext();
        _problemRepo = new ProblemRepository(_dbContext);
        _ = LoadLoadedProblemsAsync();
    }

    [ObservableProperty]
    private ObservableCollection<Problem> loadedProblems = new();

    [ObservableProperty]
    private Problem? selectedLoadedProblem;

    [ObservableProperty]
    private int editingProblemId;

    [RelayCommand]
    public void AddSingleTestCase()
    {
        SingleTestCases.Add(new ProblemImportTestCaseModel
        {
            OrderIndex = SingleTestCases.Count + 1,
            IsExample = true
        });
    }

    [RelayCommand]
    public void RemoveSingleTestCase(ProblemImportTestCaseModel? testCase)
    {
        if (testCase == null)
        {
            return;
        }

        SingleTestCases.Remove(testCase);
        ReindexSingleTestCases();
    }

    [RelayCommand]
    public void ResetSingleForm()
    {
        SingleProblemCode = string.Empty;
        SingleExamType = "TQC";
        SingleTitle = string.Empty;
        SingleDescription = string.Empty;
        SingleDifficulty = 1;
        SingleStatus = "Draft";
        SingleSolutionLanguage = "Python";
        SingleSolutionCode = string.Empty;

        SingleTestCases = new ObservableCollection<ProblemImportTestCaseModel>
        {
            new()
            {
                OrderIndex = 1,
                IsExample = true
            }
        };

        ImportStatusMessage = "已重設單題表單。";
    }

    public async Task PreviewSpreadsheetAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            ImportStatusMessage = "請先選擇有效的 CSV/XLS/XLSX 檔案。";
            return;
        }

        await RunBusyAsync(async () =>
        {
            SpreadsheetFilePath = filePath;
            var preview = await _importService.PreviewSpreadsheetAsync(filePath);

            PreviewRows = new ObservableCollection<ProblemImportSpreadsheetRow>(preview.Rows);
            ValidationIssues = new ObservableCollection<ProblemImportValidationIssue>(preview.Issues);
            ImportStatusMessage = $"已載入 {preview.RowCount} 列，通過驗證 {preview.ValidRowCount} 列。";
        });
    }

    public async Task ImportSpreadsheetAsync()
    {
        if (PreviewRows.Count > 0)
        {
            await RunBusyAsync(async () =>
            {
                var result = await _importService.ImportSpreadsheetRowsAsync(PreviewRows.ToList());
                ValidationIssues = new ObservableCollection<ProblemImportValidationIssue>(result.Issues);
                ImportStatusMessage = result.Summary;
            });
            return;
        }

        if (string.IsNullOrWhiteSpace(SpreadsheetFilePath) || !File.Exists(SpreadsheetFilePath))
        {
            ImportStatusMessage = "請先選擇有效的 CSV/XLS/XLSX 檔案。";
            return;
        }

        await RunBusyAsync(async () =>
        {
            var result = await _importService.ImportSpreadsheetAsync(SpreadsheetFilePath);
            ValidationIssues = new ObservableCollection<ProblemImportValidationIssue>(result.Issues);
            ImportStatusMessage = result.Summary;
            await LoadLoadedProblemsAsync();
        });
    }

    public async Task LoadLoadedProblemsAsync()
    {
        var list = await _problemRepo.GetAllAsync();
        LoadedProblems = new ObservableCollection<Problem>(list);
    }

    public void LoadProblemForEdit(Problem problem)
    {
        if (problem == null)
        {
            return;
        }

        EditingProblemId = problem.ProblemId;
        SingleProblemCode = problem.ProblemCode;
        SingleExamType = problem.ExamType;
        SingleTitle = problem.Title;
        SingleDescription = problem.Description;
        SingleDifficulty = problem.Difficulty;
        SingleStatus = problem.Status;
        SingleSolutionLanguage = problem.SolutionLanguage;
        SingleSolutionCode = problem.SolutionCode;
        SingleTestCases = new ObservableCollection<ProblemImportTestCaseModel>(
            problem.TestCases.Select(testCase => new ProblemImportTestCaseModel
            {
                OrderIndex = testCase.OrderIndex,
                Input = testCase.Input,
                ExpectedOutput = testCase.ExpectedOutput,
                IsExample = testCase.IsExample
            }));

        ImportStatusMessage = $"正在編輯題目 {problem.ProblemCode}";
    }

    [RelayCommand]
    public void AddSpreadsheetRow()
    {
        PreviewRows.Add(new ProblemImportSpreadsheetRow
        {
            RowNumber = PreviewRows.Count + 2,
            ExamType = "TQC",
            Difficulty = 1,
            Status = "Draft",
            SolutionLanguage = "Python",
            OrderIndex = PreviewRows.Count + 1,
            IsExample = true
        });

        ImportStatusMessage = "已新增一列，可直接編輯欄位內容後進行驗證或匯入。";
    }

    [RelayCommand]
    public async Task AddSingleProblem()
    {
        await ImportSingleProblemAsync();
        ResetSingleForm();
    }

    [RelayCommand]
    public void RemoveSpreadsheetRow(ProblemImportSpreadsheetRow? row)
    {
        if (row == null)
        {
            return;
        }

        PreviewRows.Remove(row);
        for (var index = 0; index < PreviewRows.Count; index++)
        {
            PreviewRows[index].RowNumber = index + 2;
            PreviewRows[index].OrderIndex = PreviewRows[index].OrderIndex <= 0 ? index + 1 : PreviewRows[index].OrderIndex;
        }

        ImportStatusMessage = "已刪除一列。";
    }

    [RelayCommand]
    public void ValidateSpreadsheetRows()
    {
        if (PreviewRows.Count == 0)
        {
            ImportStatusMessage = "目前沒有可驗證的列資料。";
            ValidationIssues = new ObservableCollection<ProblemImportValidationIssue>();
            return;
        }

        var preview = _importService.ValidateSpreadsheetRows(PreviewRows.ToList());
        PreviewRows = new ObservableCollection<ProblemImportSpreadsheetRow>(preview.Rows);
        ValidationIssues = new ObservableCollection<ProblemImportValidationIssue>(preview.Issues);
        ImportStatusMessage = $"已驗證 {preview.RowCount} 列，通過 {preview.ValidRowCount} 列。";
    }

    public async Task ImportSingleProblemAsync()
    {
        var form = BuildSingleForm();

        await RunBusyAsync(async () =>
        {
            var result = await _importService.ImportSingleProblemAsync(form);
            ValidationIssues = new ObservableCollection<ProblemImportValidationIssue>(result.Issues);
            ImportStatusMessage = result.Summary;
            await LoadLoadedProblemsAsync();
        });
    }

    [RelayCommand]
    public async Task DeleteLoadedProblem(Problem? problem)
    {
        if (problem == null)
        {
            return;
        }

        await _problemRepo.DeleteAsync(problem.ProblemId);
        await LoadLoadedProblemsAsync();
        ImportStatusMessage = $"已刪除題目 {problem.ProblemCode}";
    }

    [RelayCommand]
    public void EditLoadedProblem(Problem? problem)
    {
        if (problem == null)
        {
            return;
        }

        EditingProblemId = problem.ProblemId;
        SingleProblemCode = problem.ProblemCode;
        SingleExamType = problem.ExamType;
        SingleTitle = problem.Title;
        SingleDescription = problem.Description;
        SingleDifficulty = problem.Difficulty;
        SingleStatus = problem.Status;
        SingleSolutionLanguage = problem.SolutionLanguage;
        SingleSolutionCode = problem.SolutionCode;
        SingleTestCases = new ObservableCollection<ProblemImportTestCaseModel>(
            problem.TestCases.Select(tc => new ProblemImportTestCaseModel
            {
                OrderIndex = tc.OrderIndex,
                Input = tc.Input,
                ExpectedOutput = tc.ExpectedOutput,
                IsExample = tc.IsExample
            }));

        ImportStatusMessage = $"正在編輯題目 {problem.ProblemCode}（按「匯入 / 更新」以儲存變更）";
    }

    [RelayCommand]
    public async Task ImportSingleProblem()
    {
        await ImportSingleProblemAsync();
    }

    public async Task DownloadTemplateAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            ImportStatusMessage = "請指定模板儲存位置。";
            return;
        }

        await File.WriteAllTextAsync(filePath, _importService.BuildTemplateCsv(), Encoding.UTF8);
        ImportStatusMessage = $"模板已輸出到 {filePath}";
    }

    private ProblemImportFormModel BuildSingleForm()
    {
        return new ProblemImportFormModel
        {
            ProblemCode = SingleProblemCode.Trim(),
            ExamType = string.IsNullOrWhiteSpace(SingleExamType) ? "TQC" : SingleExamType.Trim(),
            Title = SingleTitle.Trim(),
            Description = SingleDescription.Trim(),
            Difficulty = SingleDifficulty,
            Status = string.IsNullOrWhiteSpace(SingleStatus) ? "Draft" : SingleStatus.Trim(),
            SolutionLanguage = string.IsNullOrWhiteSpace(SingleSolutionLanguage) ? "Python" : SingleSolutionLanguage.Trim(),
            SolutionCode = SingleSolutionCode,
            TestCases = SingleTestCases
                .Select((testCase, index) => new ProblemImportTestCaseModel
                {
                    OrderIndex = testCase.OrderIndex <= 0 ? index + 1 : testCase.OrderIndex,
                    Input = testCase.Input?.Trim() ?? string.Empty,
                    ExpectedOutput = testCase.ExpectedOutput?.Trim() ?? string.Empty,
                    IsExample = testCase.IsExample
                })
                .ToList()
        };
    }

    private void ReindexSingleTestCases()
    {
        for (var index = 0; index < SingleTestCases.Count; index++)
        {
            SingleTestCases[index].OrderIndex = index + 1;
        }
    }

    private async Task RunBusyAsync(System.Func<Task> action)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            await action();
        }
        finally
        {
            IsBusy = false;
        }
    }
}