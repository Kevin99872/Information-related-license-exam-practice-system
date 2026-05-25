using B3.Models;
using B3.Services;
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
    }

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
        });
    }

    public async Task ImportSingleProblemAsync()
    {
        var form = BuildSingleForm();

        await RunBusyAsync(async () =>
        {
            var result = await _importService.ImportSingleProblemAsync(form);
            ValidationIssues = new ObservableCollection<ProblemImportValidationIssue>(result.Issues);
            ImportStatusMessage = result.Summary;
        });
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