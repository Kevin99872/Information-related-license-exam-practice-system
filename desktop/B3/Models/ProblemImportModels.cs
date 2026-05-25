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

public sealed class ProblemImportSpreadsheetRow
{
    public int RowNumber { get; set; }
    public string ProblemCode { get; set; } = string.Empty;
    public string ExamType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public string Status { get; set; } = string.Empty;
    public string SolutionLanguage { get; set; } = string.Empty;
    public string SolutionCode { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public bool IsExample { get; set; }
    public string TestInput { get; set; } = string.Empty;
    public string ExpectedOutput { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string ValidationMessage { get; set; } = string.Empty;
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
}