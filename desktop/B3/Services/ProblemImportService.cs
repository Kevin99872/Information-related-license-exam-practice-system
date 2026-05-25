using B3.Data;
using B3.Models;
using Microsoft.EntityFrameworkCore;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace B3.Services;

/// <summary>
/// 題目導入Service - 支援TXT種子匯入、單題表單匯入、CSV/XLS/XLSX批次匯入
/// </summary>
public class ProblemImportService
{
    private static readonly string[] SpreadsheetHeaders =
    {
        "ProblemCode", "ExamType", "Title", "Description", "Difficulty", "Status",
        "SolutionLanguage", "SolutionCode", "OrderIndex", "IsExample", "TestInput", "ExpectedOutput"
    };

    private static readonly Dictionary<string, string[]> HeaderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ProblemCode"] = ["ProblemCode", "題目代碼", "題號", "題目編號"],
        ["ExamType"] = ["ExamType", "考照種類", "類型"],
        ["Title"] = ["Title", "題目標題", "題目名稱", "標題"],
        ["Description"] = ["Description", "題目敘述", "題目說明", "說明"],
        ["Difficulty"] = ["Difficulty", "難度"],
        ["Status"] = ["Status", "狀態"],
        ["SolutionLanguage"] = ["SolutionLanguage", "解法語言", "語言"],
        ["SolutionCode"] = ["SolutionCode", "解法程式碼", "程式碼"],
        ["OrderIndex"] = ["OrderIndex", "測試順序", "排序"],
        ["IsExample"] = ["IsExample", "是否範例", "範例"],
        ["TestInput"] = ["TestInput", "測試資料", "輸入"],
        ["ExpectedOutput"] = ["ExpectedOutput", "驗證資料", "預期輸出", "輸出"]
    };

    private readonly ProblemRepository _problemRepo;
    private readonly TestCaseRepository _testCaseRepo;
    private readonly ExamDbContext _dbContext = new();

    public ProblemImportService()
    {
        _problemRepo = new ProblemRepository(_dbContext);
        _testCaseRepo = new TestCaseRepository(_dbContext);
    }

    /// <summary>
    /// 從文件夾批量導入所有TQC題目
    /// </summary>
    public async Task<int> ImportFromFolderAsync(string folderPath)
    {
        var importCount = 0;

        try
        {
            var files = Directory.GetFiles(folderPath, "*.txt");

            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var problem = await ImportProblemFromFileAsync(file, fileName);
                if (problem != null)
                {
                    importCount++;
                }
            }
        }
        catch (Exception ex)
        {
            LoggerService.LogError("導入失敗", ex);
        }

        return importCount;
    }

    /// <summary>
    /// 若資料庫尚無題目則進行批量導入
    /// </summary>
    public async Task<int> ImportIfEmptyAsync(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            return 0;
        }

        var hasData = await _dbContext.Problems.AnyAsync();
        if (hasData)
        {
            return 0;
        }

        return await ImportFromFolderAsync(folderPath);
    }

    /// <summary>
    /// 單題表單匯入或更新
    /// </summary>
    public async Task<ProblemImportResult> ImportSingleProblemAsync(ProblemImportFormModel form)
    {
        var validationIssues = ValidateSingleProblem(form);
        if (validationIssues.Any(issue => issue.Severity == ProblemImportSeverity.Error))
        {
            return new ProblemImportResult
            {
                Issues = validationIssues,
                Summary = "表單驗證失敗"
            };
        }

        var saved = await UpsertProblemAsync(form);
        return new ProblemImportResult
        {
            ImportedProblems = 1,
            ImportedTestCases = saved.Item2,
            Issues = validationIssues,
            Summary = $"已儲存題目 {saved.Item1.ProblemCode}"
        };
    }

    /// <summary>
    /// 預覽 CSV/XLS/XLSX 內容，僅做驗證不寫入資料庫
    /// </summary>
    public async Task<ProblemImportPreviewResult> PreviewSpreadsheetAsync(string filePath)
    {
        var rows = await ParseSpreadsheetAsync(filePath);
        var issues = new List<ProblemImportValidationIssue>();

        foreach (var row in rows)
        {
            if (!row.IsValid)
            {
                issues.Add(new ProblemImportValidationIssue(row.RowNumber, "Sheet", row.ValidationMessage, ProblemImportSeverity.Error));
            }
        }

        return new ProblemImportPreviewResult
        {
            Rows = rows,
            Issues = issues
        };
    }

    /// <summary>
    /// 匯入 CSV/XLS/XLSX 題目表單
    /// </summary>
    public async Task<ProblemImportResult> ImportSpreadsheetAsync(string filePath)
    {
        var rows = await ParseSpreadsheetAsync(filePath);
        var issues = rows
            .Where(row => !row.IsValid)
            .Select(row => new ProblemImportValidationIssue(row.RowNumber, "Sheet", row.ValidationMessage, ProblemImportSeverity.Error))
            .ToList();

        var result = new ProblemImportResult
        {
            Issues = issues,
            SkippedRows = rows.Count(row => !row.IsValid)
        };

        if (issues.Any(issue => issue.Severity == ProblemImportSeverity.Error))
        {
            result.Summary = "匯入中止，因為表單驗證未通過";
            return result;
        }

        var groupedRows = rows.GroupBy(row => NormalizeProblemCode(row.ProblemCode));
        foreach (var group in groupedRows)
        {
            var groupList = group.ToList();
            var first = groupList[0];

            var problemForm = new ProblemImportFormModel
            {
                ProblemCode = first.ProblemCode,
                ExamType = first.ExamType,
                Title = first.Title,
                Description = first.Description,
                Difficulty = first.Difficulty,
                Status = first.Status,
                SolutionLanguage = first.SolutionLanguage,
                SolutionCode = first.SolutionCode,
                TestCases = groupList
                    .OrderBy(row => row.OrderIndex)
                    .Select(row => new ProblemImportTestCaseModel
                    {
                        OrderIndex = row.OrderIndex,
                        Input = row.TestInput,
                        ExpectedOutput = row.ExpectedOutput,
                        IsExample = row.IsExample
                    })
                    .ToList()
            };

            var saved = await UpsertProblemAsync(problemForm);
            result.ImportedProblems += 1;
            result.ImportedTestCases += saved.Item2;
        }

        result.Summary = $"已匯入 {result.ImportedProblems} 題，{result.ImportedTestCases} 筆測試資料";
        return result;
    }

    /// <summary>
    /// 產生 CSV 模板內容
    /// </summary>
    public string BuildTemplateCsv()
    {
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(",", SpreadsheetHeaders));
        builder.AppendLine(string.Join(",", new[]
        {
            "PYD101", "TQC", "範例題目", "請描述題目內容", "1", "Draft", "Python", "print(\"hello world\")", "1", "True", "1 2", "3"
        }.Select(EscapeCsv)));
        builder.AppendLine(string.Join(",", new[]
        {
            "PYD101", "TQC", "範例題目", "請描述題目內容", "1", "Draft", "Python", "print(\"hello world\")", "2", "False", "5 6", "11"
        }.Select(EscapeCsv)));
        return builder.ToString();
    }

    /// <summary>
    /// 解析TQC格式題目文件
    /// </summary>
    private async Task<Problem?> ImportProblemFromFileAsync(string filePath, string problemCode)
    {
        try
        {
            var existing = await _problemRepo.GetByCodeAsync(problemCode);
            var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
            var problem = ParseProblem(content, problemCode);
            if (problem == null)
            {
                return existing;
            }

            var testCases = ParseTestCases(content, existing?.ProblemId ?? 0);
            if (existing == null)
            {
                var savedProblem = await _problemRepo.AddAsync(problem);
                if (testCases.Count > 0)
                {
                    foreach (var testCase in testCases)
                    {
                        testCase.ProblemId = savedProblem.ProblemId;
                    }
                    await _testCaseRepo.AddMultipleAsync(testCases);
                }

                return savedProblem;
            }

            existing.Title = problem.Title;
            existing.Description = problem.Description;
            existing.ExamType = problem.ExamType;
            existing.Difficulty = problem.Difficulty;
            existing.Status = problem.Status;
            existing.SolutionLanguage = problem.SolutionLanguage;
            existing.SolutionCode = problem.SolutionCode;
            await _problemRepo.UpdateAsync(existing);

            if (testCases.Count > 0)
            {
                await _testCaseRepo.DeleteByProblemIdAsync(existing.ProblemId);
                foreach (var testCase in testCases)
                {
                    testCase.ProblemId = existing.ProblemId;
                }
                await _testCaseRepo.AddMultipleAsync(testCases);
            }

            return existing;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"導入 {problemCode} 失敗: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 解析TQC格式題目文件
    /// </summary>
    private Problem? ParseProblem(string content, string problemCode)
    {
        try
        {
            content = content.Replace("'''", string.Empty).Trim();

            var titleMatch = Regex.Match(content, @"TQC\+\s+程式語言Python\s+(\d+)\s+(.+?)$", RegexOptions.Multiline);
            if (!titleMatch.Success)
            {
                return null;
            }

            var difficulty = ExtractDifficulty(problemCode);
            return new Problem
            {
                ProblemCode = problemCode,
                Title = titleMatch.Groups[2].Value.Trim(),
                Description = ExtractDescription(content),
                ExamType = "TQC",
                Difficulty = difficulty,
                Status = "Active",
                SolutionLanguage = "Python",
                SolutionCode = string.Empty,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 根據題號提取難度等級
    /// </summary>
    private int ExtractDifficulty(string problemCode)
    {
        if (int.TryParse(problemCode.Substring(3), out var num))
        {
            if (num >= 400) return 3;
            if (num >= 200) return 2;
            return 1;
        }

        return 2;
    }

    /// <summary>
    /// 提取題目描述部分
    /// </summary>
    private string ExtractDescription(string content)
    {
        var match = Regex.Match(content, @"1\.\s*題目說明:(.*?)2\.\s*設計說明:", RegexOptions.Singleline);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        return content.Substring(0, Math.Min(500, content.Length));
    }

    /// <summary>
    /// 從TQC文件解析測試案例
    /// </summary>
    private List<TestCase> ParseTestCases(string content, int problemId)
    {
        var testCases = new List<TestCase>();

        try
        {
            var exampleMatch = Regex.Match(content, @"範例輸入(.*?)範例輸出(.*?)($|''')", RegexOptions.Singleline);
            if (!exampleMatch.Success)
            {
                return testCases;
            }

            var inputText = exampleMatch.Groups[1].Value.Trim();
            var outputText = exampleMatch.Groups[2].Value.Trim();

            var inputs = inputText.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.None);
            var outputs = outputText.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.None);

            for (var i = 0; i < Math.Min(inputs.Length, outputs.Length); i++)
            {
                testCases.Add(new TestCase
                {
                    ProblemId = problemId,
                    Input = inputs[i].Trim(),
                    ExpectedOutput = outputs[i].Trim(),
                    IsExample = true,
                    OrderIndex = i
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"解析測試案例失敗: {ex.Message}");
        }

        return testCases;
    }

    /// <summary>
    /// 批量標記題目為Active狀態
    /// </summary>
    public async Task<int> ActivateProblemsAsync(string[] problemCodes)
    {
        var count = 0;
        foreach (var code in problemCodes)
        {
            var problem = await _problemRepo.GetByCodeAsync(code);
            if (problem != null && problem.Status != "Active")
            {
                problem.Status = "Active";
                await _problemRepo.UpdateAsync(problem);
                count++;
            }
        }

        return count;
    }

    private async Task<List<ProblemImportSpreadsheetRow>> ParseSpreadsheetAsync(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".csv" => ParseCsvRows(filePath),
            ".xls" or ".xlsx" => ParseWorkbookRows(filePath),
            _ => throw new NotSupportedException("僅支援 CSV、XLS、XLSX 格式")
        };
    }

    private List<ProblemImportSpreadsheetRow> ParseCsvRows(string filePath)
    {
        using var parser = new TextFieldParser(filePath, Encoding.UTF8)
        {
            TextFieldType = FieldType.Delimited,
            HasFieldsEnclosedInQuotes = true,
            TrimWhiteSpace = false
        };

        parser.SetDelimiters(",");

        if (parser.EndOfData)
        {
            return new List<ProblemImportSpreadsheetRow>();
        }

        var headers = parser.ReadFields() ?? Array.Empty<string>();
        var headerMap = BuildHeaderMap(headers);
        var rows = new List<ProblemImportSpreadsheetRow>();
        var rowNumber = 2;

        while (!parser.EndOfData)
        {
            var fields = parser.ReadFields() ?? Array.Empty<string>();
            rows.Add(BuildRow(fields, headerMap, rowNumber));
            rowNumber++;
        }

        return rows;
    }

    private List<ProblemImportSpreadsheetRow> ParseWorkbookRows(string filePath)
    {
        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        IWorkbook workbook = Path.GetExtension(filePath).Equals(".xls", StringComparison.OrdinalIgnoreCase)
            ? new HSSFWorkbook(stream)
            : new XSSFWorkbook(stream);

        var sheet = workbook.GetSheetAt(0);
        if (sheet == null)
        {
            return new List<ProblemImportSpreadsheetRow>();
        }

        var headerRow = sheet.GetRow(sheet.FirstRowNum);
        if (headerRow == null)
        {
            return new List<ProblemImportSpreadsheetRow>();
        }

        var headers = new List<string>();
        for (var cellIndex = headerRow.FirstCellNum; cellIndex < headerRow.LastCellNum; cellIndex++)
        {
            headers.Add(GetCellValue(headerRow.GetCell(cellIndex)));
        }

        var headerMap = BuildHeaderMap(headers.ToArray());
        var rows = new List<ProblemImportSpreadsheetRow>();

        for (var rowIndex = sheet.FirstRowNum + 1; rowIndex <= sheet.LastRowNum; rowIndex++)
        {
            var row = sheet.GetRow(rowIndex);
            if (row == null)
            {
                continue;
            }

            var fields = new List<string>();
            for (var cellIndex = row.FirstCellNum; cellIndex < headerRow.LastCellNum; cellIndex++)
            {
                fields.Add(GetCellValue(row.GetCell(cellIndex)));
            }

            rows.Add(BuildRow(fields.ToArray(), headerMap, rowIndex + 1));
        }

        return rows;
    }

    private Dictionary<string, int> BuildHeaderMap(string[] headers)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < headers.Length; index++)
        {
            var header = NormalizeText(headers[index]);
            if (string.IsNullOrWhiteSpace(header))
            {
                continue;
            }

            var canonical = ResolveCanonicalHeader(header);
            if (!map.ContainsKey(canonical))
            {
                map[canonical] = index;
            }
        }

        return map;
    }

    private string ResolveCanonicalHeader(string header)
    {
        foreach (var pair in HeaderAliases)
        {
            if (pair.Value.Any(alias => string.Equals(NormalizeText(alias), header, StringComparison.OrdinalIgnoreCase)))
            {
                return pair.Key;
            }
        }

        return header;
    }

    private ProblemImportSpreadsheetRow BuildRow(string[] fields, IReadOnlyDictionary<string, int> headerMap, int rowNumber)
    {
        string GetValue(string header)
        {
            if (!headerMap.TryGetValue(header, out var index) || index >= fields.Length)
            {
                return string.Empty;
            }

            return fields[index].Trim();
        }

        var row = new ProblemImportSpreadsheetRow
        {
            RowNumber = rowNumber,
            ProblemCode = GetValue("ProblemCode"),
            ExamType = NormalizeRequired(GetValue("ExamType"), "TQC"),
            Title = GetValue("Title"),
            Description = GetValue("Description"),
            Difficulty = ParseDifficulty(GetValue("Difficulty")),
            Status = NormalizeRequired(GetValue("Status"), "Draft"),
            SolutionLanguage = NormalizeRequired(GetValue("SolutionLanguage"), "Python"),
            SolutionCode = GetValue("SolutionCode"),
            OrderIndex = ParseInteger(GetValue("OrderIndex"), rowNumber - 1),
            IsExample = ParseBoolean(GetValue("IsExample"), true),
            TestInput = GetValue("TestInput"),
            ExpectedOutput = GetValue("ExpectedOutput")
        };

        var issues = ValidateSpreadsheetRow(row);
        row.IsValid = !issues.Any(issue => issue.Severity == ProblemImportSeverity.Error);
        row.ValidationMessage = issues.Count == 0
            ? ""
            : string.Join("；", issues.Select(issue => issue.Message));
        return row;
    }

    private List<ProblemImportValidationIssue> ValidateSpreadsheetRow(ProblemImportSpreadsheetRow row)
    {
        var issues = new List<ProblemImportValidationIssue>();

        if (string.IsNullOrWhiteSpace(row.ProblemCode))
        {
            issues.Add(new ProblemImportValidationIssue(row.RowNumber, "ProblemCode", "題目代碼不可為空", ProblemImportSeverity.Error));
        }

        if (string.IsNullOrWhiteSpace(row.ExamType))
        {
            issues.Add(new ProblemImportValidationIssue(row.RowNumber, "ExamType", "考照種類不可為空", ProblemImportSeverity.Error));
        }

        if (string.IsNullOrWhiteSpace(row.Title))
        {
            issues.Add(new ProblemImportValidationIssue(row.RowNumber, "Title", "題目標題不可為空", ProblemImportSeverity.Error));
        }

        if (string.IsNullOrWhiteSpace(row.Description))
        {
            issues.Add(new ProblemImportValidationIssue(row.RowNumber, "Description", "題目敘述不可為空", ProblemImportSeverity.Error));
        }

        if (string.IsNullOrWhiteSpace(row.TestInput))
        {
            issues.Add(new ProblemImportValidationIssue(row.RowNumber, "TestInput", "測試資料不可為空", ProblemImportSeverity.Error));
        }

        if (string.IsNullOrWhiteSpace(row.ExpectedOutput))
        {
            issues.Add(new ProblemImportValidationIssue(row.RowNumber, "ExpectedOutput", "驗證資料不可為空", ProblemImportSeverity.Error));
        }

        if (row.Difficulty < 1 || row.Difficulty > 3)
        {
            issues.Add(new ProblemImportValidationIssue(row.RowNumber, "Difficulty", "難度僅能為 1、2、3，或對應的中文標籤", ProblemImportSeverity.Error));
        }

        return issues;
    }

    private async Task<(Problem, int)> UpsertProblemAsync(ProblemImportFormModel form)
    {
        var existing = await _problemRepo.GetByCodeAsync(form.ProblemCode);
        var testCaseCount = form.TestCases?.Count ?? 0;

        if (existing == null)
        {
            var problem = new Problem
            {
                ProblemCode = form.ProblemCode,
                ExamType = form.ExamType,
                Title = form.Title,
                Description = form.Description,
                Difficulty = form.Difficulty,
                Status = string.IsNullOrWhiteSpace(form.Status) ? "Draft" : form.Status,
                SolutionLanguage = string.IsNullOrWhiteSpace(form.SolutionLanguage) ? "Python" : form.SolutionLanguage,
                SolutionCode = form.SolutionCode ?? string.Empty,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var saved = await _problemRepo.AddAsync(problem);
            await InsertTestCasesAsync(saved.ProblemId, form.TestCases);
            return (saved, testCaseCount);
        }

        existing.ExamType = form.ExamType;
        existing.Title = form.Title;
        existing.Description = form.Description;
        existing.Difficulty = form.Difficulty;
        existing.Status = string.IsNullOrWhiteSpace(form.Status) ? existing.Status : form.Status;
        existing.SolutionLanguage = string.IsNullOrWhiteSpace(form.SolutionLanguage) ? existing.SolutionLanguage : form.SolutionLanguage;
        existing.SolutionCode = form.SolutionCode ?? string.Empty;
        await _problemRepo.UpdateAsync(existing);

        await _testCaseRepo.DeleteByProblemIdAsync(existing.ProblemId);
        await InsertTestCasesAsync(existing.ProblemId, form.TestCases);
        return (existing, testCaseCount);
    }

    private async Task InsertTestCasesAsync(int problemId, IEnumerable<ProblemImportTestCaseModel>? testCases)
    {
        var entities = new List<TestCase>();
        if (testCases != null)
        {
            entities.AddRange(testCases.Select(testCase => new TestCase
            {
                ProblemId = problemId,
                Input = testCase.Input,
                ExpectedOutput = testCase.ExpectedOutput,
                IsExample = testCase.IsExample,
                OrderIndex = testCase.OrderIndex
            }));
        }

        if (entities.Count > 0)
        {
            await _testCaseRepo.AddMultipleAsync(entities);
        }
    }

    private List<ProblemImportValidationIssue> ValidateSingleProblem(ProblemImportFormModel form)
    {
        var issues = new List<ProblemImportValidationIssue>();

        if (string.IsNullOrWhiteSpace(form.ProblemCode))
        {
            issues.Add(new ProblemImportValidationIssue(1, "ProblemCode", "題目代碼不可為空", ProblemImportSeverity.Error));
        }

        if (string.IsNullOrWhiteSpace(form.ExamType))
        {
            issues.Add(new ProblemImportValidationIssue(1, "ExamType", "考照種類不可為空", ProblemImportSeverity.Error));
        }

        if (string.IsNullOrWhiteSpace(form.Title))
        {
            issues.Add(new ProblemImportValidationIssue(1, "Title", "題目標題不可為空", ProblemImportSeverity.Error));
        }

        if (string.IsNullOrWhiteSpace(form.Description))
        {
            issues.Add(new ProblemImportValidationIssue(1, "Description", "題目敘述不可為空", ProblemImportSeverity.Error));
        }

        if (form.Difficulty < 1 || form.Difficulty > 3)
        {
            issues.Add(new ProblemImportValidationIssue(1, "Difficulty", "難度僅能為 1、2、3", ProblemImportSeverity.Error));
        }

        if (form.TestCases == null || form.TestCases.Count == 0)
        {
            issues.Add(new ProblemImportValidationIssue(1, "TestCases", "至少需要一組測試資料與驗證資料", ProblemImportSeverity.Error));
        }
        else
        {
            foreach (var testCase in form.TestCases.Select((value, index) => (value, index)))
            {
                var rowNumber = testCase.index + 1;
                if (string.IsNullOrWhiteSpace(testCase.value.Input))
                {
                    issues.Add(new ProblemImportValidationIssue(rowNumber, "TestInput", "測試資料不可為空", ProblemImportSeverity.Error));
                }

                if (string.IsNullOrWhiteSpace(testCase.value.ExpectedOutput))
                {
                    issues.Add(new ProblemImportValidationIssue(rowNumber, "ExpectedOutput", "驗證資料不可為空", ProblemImportSeverity.Error));
                }
            }
        }

        return issues;
    }

    private int ParseDifficulty(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 1;
        }

        var normalized = NormalizeText(value);
        return normalized switch
        {
            "1" or "簡單" => 1,
            "2" or "中等" or "中級" => 2,
            "3" or "困難" => 3,
            _ when int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) => Math.Clamp(parsed, 1, 3),
            _ => 1
        };
    }

    private bool ParseBoolean(string value, bool defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        var normalized = NormalizeText(value);
        return normalized switch
        {
            "true" or "1" or "yes" or "y" or "是" or "對" => true,
            "false" or "0" or "no" or "n" or "否" or "不是" => false,
            _ when bool.TryParse(value, out var parsed) => parsed,
            _ => defaultValue
        };
    }

    private int ParseInteger(string value, int defaultValue)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : defaultValue;
    }

    private string NormalizeRequired(string value, string defaultValue)
    {
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
    }

    private string NormalizeText(string value)
    {
        return value.Trim().Replace(" ", string.Empty).Replace("\t", string.Empty);
    }

    private string GetCellValue(ICell? cell)
    {
        if (cell == null)
        {
            return string.Empty;
        }

        return cell.CellType switch
        {
            CellType.Boolean => cell.BooleanCellValue ? "True" : "False",
            CellType.Numeric => DateUtil.IsCellDateFormatted(cell)
                ? $"{cell.DateCellValue:yyyy-MM-dd HH:mm:ss}"
                : cell.NumericCellValue.ToString(CultureInfo.InvariantCulture),
            CellType.Formula => cell.ToString(),
            CellType.String => cell.StringCellValue,
            CellType.Blank => string.Empty,
            _ => cell.ToString()
        };
    }

    private string EscapeCsv(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    private string NormalizeProblemCode(string problemCode)
    {
        return problemCode.Trim().ToUpperInvariant();
    }
}