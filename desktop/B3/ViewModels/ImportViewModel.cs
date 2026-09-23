using B3.Models;
using B3.Services;
using B3.Data;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace B3.ViewModels;

/// <summary>
/// 匯入ViewModel - 題目單題輸入、批次表單預覽與匯入
/// 大量資料處理：預覽分頁顯示、驗證分塊進行並讓出 UI、匯入於背景小批次寫入，全程以進度條回報
/// </summary>
public partial class ImportViewModel : ViewModelBase
{
    /// <summary>預覽每頁列數 - 每列含 12 個輸入框，一次渲染過多會凍結畫面</summary>
    private const int PreviewPageSize = 50;

    /// <summary>每次在 UI 執行緒驗證的列數，驗證完一塊就讓出 UI 更新進度條</summary>
    private const int ValidationChunkSize = 500;

    /// <summary>驗證訊息區最多顯示的筆數</summary>
    private const int MaxDisplayedIssues = 100;

    private readonly ProblemImportService _importService = new();
    private readonly ProblemRepository _problemRepo;
    private ExamDbContext _dbContext = null!;

    /// <summary>表單匯入的全部列 (畫面只顯示目前頁)</summary>
    private readonly List<ProblemImportSpreadsheetRow> _allRows = new();
    private readonly DispatcherTimer _issueRefreshTimer;
    private CancellationTokenSource? _operationCts;
    private bool _suppressRowValidation;

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

    /// <summary>目前頁的預覽列</summary>
    [ObservableProperty]
    private ObservableCollection<ProblemImportSpreadsheetRow> previewRows = new();

    /// <summary>驗證訊息 (最多顯示 MaxDisplayedIssues 筆)</summary>
    [ObservableProperty]
    private ObservableCollection<ProblemImportValidationIssue> validationIssues = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool isBusy;

    public bool IsNotBusy => !IsBusy;

    /// <summary>進度條是否顯示</summary>
    [ObservableProperty]
    private bool isProgressVisible;

    /// <summary>進度百分比 (0~100)</summary>
    [ObservableProperty]
    private double progressValue;

    /// <summary>無法得知總量時 (例如解析檔案) 顯示不定進度</summary>
    [ObservableProperty]
    private bool isProgressIndeterminate;

    /// <summary>進度說明文字</summary>
    [ObservableProperty]
    private string progressText = string.Empty;

    /// <summary>目前作業是否可取消</summary>
    [ObservableProperty]
    private bool canCancelOperation;

    /// <summary>目前頁 (從 1 開始)</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PageInfoText))]
    private int currentPage = 1;

    /// <summary>總頁數</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PageInfoText))]
    private int pageCount = 1;

    /// <summary>只顯示未通過驗證的列</summary>
    [ObservableProperty]
    private bool showInvalidRowsOnly;

    /// <summary>列數統計文字</summary>
    [ObservableProperty]
    private string rowSummaryText = string.Empty;

    /// <summary>驗證訊息統計文字</summary>
    [ObservableProperty]
    private string issueSummaryText = string.Empty;

    public string PageInfoText => $"{CurrentPage} / {PageCount}";

    public ImportViewModel()
    {
        ResetSingleForm();
        _dbContext = new ExamDbContext();
        _problemRepo = new ProblemRepository(_dbContext);

        // 使用者編輯時會連續觸發，合併成一次彙整驗證訊息
        _issueRefreshTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _issueRefreshTimer.Tick += (_, _) =>
        {
            _issueRefreshTimer.Stop();
            RefreshValidationSummary();
        };

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
            await LoadSpreadsheetCoreAsync(filePath);
        });
    }

    public async Task ImportSpreadsheetAsync()
    {
        if (_allRows.Count == 0 && (string.IsNullOrWhiteSpace(SpreadsheetFilePath) || !File.Exists(SpreadsheetFilePath)))
        {
            ImportStatusMessage = "請先選擇有效的 CSV/XLS/XLSX 檔案。";
            return;
        }

        await RunBusyAsync(async () =>
        {
            _operationCts = new CancellationTokenSource();
            CanCancelOperation = true;
            var token = _operationCts.Token;

            try
            {
                if (_allRows.Count == 0)
                {
                    await LoadSpreadsheetCoreAsync(SpreadsheetFilePath);
                }

                // 1. 分塊驗證全部列
                if (!await ValidateAllRowsInChunksAsync(token))
                {
                    ImportStatusMessage = $"已取消驗證。{RowSummaryText}";
                    return;
                }

                var invalidCount = _allRows.Count(row => !row.IsValid);
                if (invalidCount > 0)
                {
                    ShowInvalidRowsOnly = true;
                    ImportStatusMessage = $"匯入中止：有 {invalidCount} 列未通過驗證，已切換為只顯示錯誤列，修正後再匯入。";
                    return;
                }

                // 2. 合併題目 (背景執行緒)
                SetProgress("整理題目資料...", indeterminate: true);
                var snapshot = _allRows.ToList();
                var forms = await Task.Run(() => _importService.BuildProblemForms(snapshot), token);

                // 3. 小批次寫入資料庫
                var progress = new Progress<ImportProgressInfo>(info =>
                    SetProgress($"{info.Stage}：{info.Completed} / {info.Total} 題", info.Percent));
                var result = await _importService.ImportProblemFormsAsync(forms, progress, token);

                ImportStatusMessage = result.Summary;
                await LoadLoadedProblemsAsync();
            }
            catch (OperationCanceledException)
            {
                ImportStatusMessage = "已取消作業。";
            }
            catch (Exception ex)
            {
                LoggerService.LogError("表單匯入失敗", ex);
                ImportStatusMessage = $"匯入失敗：{ex.Message}";
            }
            finally
            {
                CanCancelOperation = false;
                _operationCts.Dispose();
                _operationCts = null;
            }
        });
    }

    /// <summary>取消進行中的驗證或匯入</summary>
    [RelayCommand]
    public void CancelOperation()
    {
        _operationCts?.Cancel();
        ProgressText = "正在取消...";
    }

    public async Task LoadLoadedProblemsAsync()
    {
        // 不載入測試資料，編輯時再個別讀取，避免題庫很大時清單載入卡頓
        var list = await _problemRepo.GetAllSummaryAsync();
        LoadedProblems = new ObservableCollection<Problem>(list);
    }

    public void LoadProblemForEdit(Problem problem)
    {
        if (problem == null)
        {
            return;
        }

        _ = LoadProblemIntoFormAsync(problem, $"正在編輯題目 {problem.ProblemCode}");
    }

    [RelayCommand]
    public void AddSpreadsheetRow()
    {
        var row = new ProblemImportSpreadsheetRow
        {
            RowNumber = _allRows.Count + 2,
            ExamType = "TQC",
            Difficulty = 1,
            Status = "Draft",
            SolutionLanguage = "Python",
            OrderIndex = _allRows.Count + 1,
            IsExample = true
        };
        _importService.ValidateRow(row);
        _allRows.Add(row);

        ShowInvalidRowsOnly = false;
        ShowPage(int.MaxValue);
        RefreshValidationSummary();
        ImportStatusMessage = "已新增一列，可直接編輯欄位內容，驗證結果會即時更新。";
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
        if (row == null || !_allRows.Remove(row))
        {
            return;
        }

        _suppressRowValidation = true;
        for (var index = 0; index < _allRows.Count; index++)
        {
            _allRows[index].RowNumber = index + 2;
            _allRows[index].OrderIndex = _allRows[index].OrderIndex <= 0 ? index + 1 : _allRows[index].OrderIndex;
        }
        _suppressRowValidation = false;

        ShowPage(CurrentPage);
        RefreshValidationSummary();
        ImportStatusMessage = "已刪除一列。";
    }

    [RelayCommand]
    public async Task ValidateSpreadsheetRows()
    {
        if (_allRows.Count == 0)
        {
            ImportStatusMessage = "目前沒有可驗證的列資料。";
            ValidationIssues = new ObservableCollection<ProblemImportValidationIssue>();
            return;
        }

        await RunBusyAsync(async () =>
        {
            _operationCts = new CancellationTokenSource();
            CanCancelOperation = true;
            try
            {
                var completed = await ValidateAllRowsInChunksAsync(_operationCts.Token);
                ImportStatusMessage = completed ? $"驗證完成。{RowSummaryText}" : $"已取消驗證。{RowSummaryText}";
            }
            finally
            {
                CanCancelOperation = false;
                _operationCts.Dispose();
                _operationCts = null;
            }
        });
    }

    [RelayCommand]
    public void NextPage() => ShowPage(CurrentPage + 1);

    [RelayCommand]
    public void PreviousPage() => ShowPage(CurrentPage - 1);

    partial void OnShowInvalidRowsOnlyChanged(bool value) => ShowPage(1);

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
    public async Task EditLoadedProblem(Problem? problem)
    {
        if (problem == null)
        {
            return;
        }

        await LoadProblemIntoFormAsync(problem, $"正在編輯題目 {problem.ProblemCode}（按「匯入 / 更新」以儲存變更）");
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

    /// <summary>解析檔案 (背景執行緒) 並顯示第一頁</summary>
    private async Task LoadSpreadsheetCoreAsync(string filePath)
    {
        SetProgress("解析檔案中...", indeterminate: true);
        try
        {
            var preview = await _importService.PreviewSpreadsheetAsync(filePath);

            DetachPageRows();
            _allRows.Clear();
            _allRows.AddRange(preview.Rows);

            ShowInvalidRowsOnly = false;
            ShowPage(1);
            RefreshValidationSummary();
            ImportStatusMessage = $"已載入檔案。{RowSummaryText}";
        }
        catch (Exception ex)
        {
            LoggerService.LogError("解析匯入檔案失敗", ex);
            ImportStatusMessage = $"讀取檔案失敗：{ex.Message}";
        }
    }

    /// <summary>
    /// 分塊驗證全部列：每塊在 UI 執行緒處理後讓出，讓進度條與畫面得以更新
    /// (列物件已綁定到畫面，不能在背景執行緒修改)
    /// </summary>
    /// <returns>是否完整跑完 (取消時回傳 false)</returns>
    private async Task<bool> ValidateAllRowsInChunksAsync(CancellationToken token)
    {
        var total = _allRows.Count;
        _suppressRowValidation = true;
        try
        {
            for (var start = 0; start < total; start += ValidationChunkSize)
            {
                if (token.IsCancellationRequested)
                {
                    return false;
                }

                var end = Math.Min(start + ValidationChunkSize, total);
                for (var index = start; index < end; index++)
                {
                    _importService.NormalizeRow(_allRows[index], index);
                }

                SetProgress($"驗證中：{end} / {total} 列", end * 100.0 / total);
                await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
            }

            return true;
        }
        finally
        {
            _suppressRowValidation = false;
            if (ShowInvalidRowsOnly)
            {
                ShowPage(1);
            }
            RefreshValidationSummary();
        }
    }

    /// <summary>切換顯示頁，只有目前頁的列會綁定到畫面並監聽編輯</summary>
    private void ShowPage(int page)
    {
        var source = ShowInvalidRowsOnly ? _allRows.Where(row => !row.IsValid).ToList() : _allRows;

        PageCount = Math.Max(1, (int)Math.Ceiling(source.Count / (double)PreviewPageSize));
        CurrentPage = Math.Clamp(page, 1, PageCount);

        DetachPageRows();
        var pageRows = source.Skip((CurrentPage - 1) * PreviewPageSize).Take(PreviewPageSize).ToList();
        foreach (var row in pageRows)
        {
            row.PropertyChanged += OnPreviewRowPropertyChanged;
        }

        PreviewRows = new ObservableCollection<ProblemImportSpreadsheetRow>(pageRows);
    }

    private void DetachPageRows()
    {
        foreach (var row in PreviewRows)
        {
            row.PropertyChanged -= OnPreviewRowPropertyChanged;
        }
    }

    /// <summary>動態驗證：使用者編輯某列欄位時立即重新驗證該列</summary>
    private void OnPreviewRowPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_suppressRowValidation || sender is not ProblemImportSpreadsheetRow row || !ProblemImportSpreadsheetRow.IsDataProperty(e.PropertyName))
        {
            return;
        }

        _importService.ValidateRow(row);
        _issueRefreshTimer.Stop();
        _issueRefreshTimer.Start();
    }

    /// <summary>重新統計列數與驗證訊息 (僅顯示前 MaxDisplayedIssues 筆)</summary>
    private void RefreshValidationSummary()
    {
        var total = _allRows.Count;
        var invalid = _allRows.Count(row => !row.IsValid);
        var problemCount = _allRows.Select(row => row.ProblemCode.Trim().ToUpperInvariant()).Distinct().Count();
        RowSummaryText = total == 0
            ? string.Empty
            : $"共 {total} 列（{problemCount} 題），通過 {total - invalid} 列，錯誤 {invalid} 列";

        var issues = _importService.BuildIssues(_allRows);
        ValidationIssues = new ObservableCollection<ProblemImportValidationIssue>(issues.Take(MaxDisplayedIssues));
        IssueSummaryText = issues.Count > MaxDisplayedIssues
            ? $"共 {issues.Count} 項問題，僅顯示前 {MaxDisplayedIssues} 項（可勾選「只顯示錯誤列」逐頁修正）"
            : issues.Count == 0 ? string.Empty : $"共 {issues.Count} 項問題";
    }

    private void SetProgress(string text, double percent = 0, bool indeterminate = false)
    {
        IsProgressVisible = true;
        IsProgressIndeterminate = indeterminate;
        ProgressValue = percent;
        ProgressText = text;
    }

    private async Task LoadProblemIntoFormAsync(Problem problem, string statusMessage)
    {
        EditingProblemId = problem.ProblemId;
        SingleProblemCode = problem.ProblemCode;
        SingleExamType = problem.ExamType;
        SingleTitle = problem.Title;
        SingleDescription = problem.Description;
        SingleDifficulty = problem.Difficulty;
        SingleStatus = problem.Status;
        SingleSolutionLanguage = problem.SolutionLanguage;
        SingleSolutionCode = problem.SolutionCode;

        var testCases = problem.TestCases.Count > 0
            ? problem.TestCases.OrderBy(testCase => testCase.OrderIndex).ToList()
            : await _problemRepo.GetTestCasesAsync(problem.ProblemId);

        SingleTestCases = new ObservableCollection<ProblemImportTestCaseModel>(
            testCases.Select(testCase => new ProblemImportTestCaseModel
            {
                OrderIndex = testCase.OrderIndex,
                Input = testCase.Input,
                ExpectedOutput = testCase.ExpectedOutput,
                IsExample = testCase.IsExample
            }));

        ImportStatusMessage = statusMessage;
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

    private async Task RunBusyAsync(Func<Task> action)
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
            IsProgressVisible = false;
            IsProgressIndeterminate = false;
            ProgressValue = 0;
        }
    }
}
