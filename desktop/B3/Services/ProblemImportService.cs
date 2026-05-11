using B3.Models;
using B3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace B3.Services;

/// <summary>
/// 題目導入Service - 從文本文件解析並匯入題目到數據庫
/// 職責: 讀取TQC格式題目文件 解析題目結構 保存到SQLite
/// </summary>
public class ProblemImportService
{
    private readonly ProblemRepository _problemRepo;
    private readonly TestCaseRepository _testCaseRepo;
    private ExamDbContext _dbContext = null!;

    public ProblemImportService()
    {
        _dbContext = new ExamDbContext();
        _problemRepo = new ProblemRepository(_dbContext);
        _testCaseRepo = new TestCaseRepository(_dbContext);
    }

    /// <summary>
    /// 從文件夾批量導入所有TQC題目
    /// </summary>
    public async Task<int> ImportFromFolderAsync(string folderPath)
    {
        int importCount = 0;

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
            // TODO: 記錄錯誤 日誌記錄
            System.Diagnostics.Debug.WriteLine($"導入失敗: {ex.Message}");
        }

        return importCount;
    }

    /// <summary>
    /// 從單個文件導入題目
    /// </summary>
    private async Task<Problem?> ImportProblemFromFileAsync(string filePath, string problemCode)
    {
        try
        {
            // 檢查題目是否已存在
            var existing = await _problemRepo.GetByCodeAsync(problemCode);
            if (existing != null)
            {
                return existing; // 已存在則跳過
            }

            var content = File.ReadAllText(filePath, System.Text.Encoding.UTF8);

            // 解析題目內容 - 按照TQC格式
            var problem = ParseProblem(content, problemCode);
            if (problem == null) return null;

            // 新增題目
            var savedProblem = await _problemRepo.AddAsync(problem);

            // 解析並新增測試案例
            var testCases = ParseTestCases(content, savedProblem.ProblemId);
            if (testCases.Count > 0)
            {
                await _testCaseRepo.AddMultipleAsync(testCases);
            }

            return savedProblem;
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
            // 移除Python三引號標記
            content = content.Replace("'''", "").Trim();

            // 提取題名
            var titleMatch = Regex.Match(content, @"TQC\+\s+程式語言Python\s+(\d+)\s+(.+?)$", RegexOptions.Multiline);
            if (!titleMatch.Success) return null;

            var difficulty = ExtractDifficulty(problemCode);
            var problem = new Problem
            {
                ProblemCode = problemCode,
                Title = titleMatch.Groups[2].Value.Trim(),
                Description = ExtractDescription(content),
                ExamType = "TQC",
                Difficulty = difficulty,
                Status = "Draft",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            return problem;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 根據題號提取難度等級
    /// 100-199: Level 1 (簡單)
    /// 200-399: Level 2 (中級)
    /// 400+: Level 3 (困難)
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
            // 尋找「範例輸入」和「範例輸出」區段
            var exampleMatch = Regex.Match(content, @"範例輸入(.*?)範例輸出(.*?)($|''')", RegexOptions.Singleline);
            if (!exampleMatch.Success) return testCases;

            var inputText = exampleMatch.Groups[1].Value.Trim();
            var outputText = exampleMatch.Groups[2].Value.Trim();

            // 分割多個測試案例
            var inputs = inputText.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.None);
            var outputs = outputText.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.None);

            for (int i = 0; i < Math.Min(inputs.Length, outputs.Length); i++)
            {
                var testCase = new TestCase
                {
                    ProblemId = problemId,
                    Input = inputs[i].Trim(),
                    ExpectedOutput = outputs[i].Trim(),
                    IsExample = true,
                    OrderIndex = i
                };
                testCases.Add(testCase);
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
        int count = 0;
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
}
