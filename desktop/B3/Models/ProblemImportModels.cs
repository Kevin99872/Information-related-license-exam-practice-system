using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace B3.Models;

public sealed class ProblemImportFormModel
{
    public string ProblemCode { get; set; } = string.Empty;
    public string ExamType { get; set; } = "TQC";
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Difficulty { get; set; } = 1;
    public string Status { get; set; } = "Draft";
    public string SolutionLanguage { get; set; } = "Python";
    public string SolutionCode { get; set; } = string.Empty;
    public List<ProblemImportTestCaseModel> TestCases { get; set; } = new();
}

public partial class ProblemImportTestCaseModel : ObservableObject
{
    [ObservableProperty]
    private int orderIndex;

    [ObservableProperty]
    private string input = string.Empty;

    [ObservableProperty]
    private string expectedOutput = string.Empty;

    [ObservableProperty]
    private bool isExample = true;
}

/// <summary>
/// 表單匯入的一列 - 可觀察屬性讓欄位編輯後即時驗證、驗證結果即時反映到畫面
/// </summary>
public partial class ProblemImportSpreadsheetRow : ObservableObject
{
    [ObservableProperty] private int rowNumber;
    [ObservableProperty] private string problemCode = string.Empty;
    [ObservableProperty] private string examType = string.Empty;
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private int difficulty;
    [ObservableProperty] private string status = string.Empty;
    [ObservableProperty] private string solutionLanguage = string.Empty;
    [ObservableProperty] private string solutionCode = string.Empty;
    [ObservableProperty] private int orderIndex;
    [ObservableProperty] private bool isExample;
    [ObservableProperty] private string testInput = string.Empty;
    [ObservableProperty] private string expectedOutput = string.Empty;
    [ObservableProperty] private bool isValid;
    [ObservableProperty] private string validationMessage = string.Empty;

    /// <summary>會影響驗證結果的欄位 (編輯後需重新驗證)</summary>
    public static bool IsDataProperty(string? propertyName) =>
        propertyName is not (nameof(IsValid) or nameof(ValidationMessage) or nameof(RowNumber));
}

public sealed record ProblemImportValidationIssue(
    int RowNumber,
    string Field,
    string Message,
    ProblemImportSeverity Severity);

public enum ProblemImportSeverity
{
    Info,
    Warning,
    Error
}

public sealed class ProblemImportPreviewResult
{
    public List<ProblemImportSpreadsheetRow> Rows { get; set; } = new();
    public List<ProblemImportValidationIssue> Issues { get; set; } = new();
    public int RowCount => Rows.Count;
    public int ValidRowCount => Rows.Where(row => row.IsValid).Count();
}

public sealed class ProblemImportResult
{
    public int ImportedProblems { get; set; }
    public int ImportedTestCases { get; set; }
    public int SkippedRows { get; set; }
    public List<ProblemImportValidationIssue> Issues { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public bool Cancelled { get; set; }
}

/// <summary>批次作業進度 (階段、已完成數、總數)</summary>
public sealed record ImportProgressInfo(string Stage, int Completed, int Total)
{
    public double Percent => Total <= 0 ? 0 : Completed * 100.0 / Total;
}