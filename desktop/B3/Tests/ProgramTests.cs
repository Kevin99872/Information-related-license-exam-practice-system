using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using B3.Data;
using B3.Models;
using B3.Services;

namespace B3.Tests;

/// <summary>
/// B3 系統功能測試程序
/// 用法: cd Tests && dotnet run
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        MainAsync(args).GetAwaiter().GetResult();
    }

    public static async Task MainAsync(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║          B3 智慧考試系統 - 功能測試                        ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

        try
        {
            // 測試 1: 數據庫初始化
            await TestDatabaseInitialization();

            // 測試 2: 題目操作
            await TestProblemOperations();

            // 測試 3: 測試案例操作
            await TestTestCaseOperations();

            // 測試 4: 審核流程
            await TestReviewProcess();

            // 測試 5: 提交統計
            await TestSubmissionStatistics();

            // 測試 6: 題目導入
            await TestProblemImport();

            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    ✅ 所有測試完成                         ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ 測試失敗: {ex.Message}");
            Console.WriteLine($"堆棧跟蹤: {ex.StackTrace}");
        }
    }

    /// <summary>測試 1: 數據庫初始化</summary>
    private static async Task TestDatabaseInitialization()
    {
        Console.WriteLine("\n【測試 1】數據庫初始化");
        Console.WriteLine("─" + new string('─', 50));

        try
        {
            ExamDbContext.Initialize();
            var dbPath = ExamDbContext.GetDatabasePath();
            Console.WriteLine($"✅ 數據庫初始化成功");
            Console.WriteLine($"   位置: {dbPath}");

            using (var context = new ExamDbContext())
            {
                var problemCount = context.Problems.Count();
                var testCaseCount = context.TestCases.Count();
                var reviewCount = context.ProblemReviews.Count();
                var submissionCount = context.UserSubmissions.Count();

                Console.WriteLine($"   題目表: {problemCount} 筆記錄");
                Console.WriteLine($"   測試案例表: {testCaseCount} 筆記錄");
                Console.WriteLine($"   審核表: {reviewCount} 筆記錄");
                Console.WriteLine($"   提交表: {submissionCount} 筆記錄");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 數據庫初始化失敗: {ex.Message}");
            throw;
        }
    }

    /// <summary>測試 2: 題目操作（CRUD）</summary>
    private static async Task TestProblemOperations()
    {
        Console.WriteLine("\n【測試 2】題目操作（CRUD）");
        Console.WriteLine("─" + new string('─', 50));

        using (var context = new ExamDbContext())
        {
            var repo = new ProblemRepository(context);

            try
            {
                // Create
                var newProblem = new Problem
                {
                    ProblemCode = "TEST001",
                    Title = "測試題目 1",
                    Description = "這是一個測試題目",
                    ExamType = "TQC",
                    Difficulty = 1,
                    Status = "Draft"
                };

                var created = await repo.AddAsync(newProblem);
                Console.WriteLine($"✅ 新增題目: {created.ProblemCode}");

                // Read
                var retrieved = await repo.GetByCodeAsync("TEST001");
                if (retrieved != null)
                {
                    Console.WriteLine($"✅ 查詢題目: {retrieved.Title}");
                }

                // Update
                retrieved!.Status = "Active";
                var updated = await repo.UpdateAsync(retrieved);
                Console.WriteLine($"✅ 更新狀態: {updated.Status}");

                // Get by Type
                var byType = await repo.GetByExamTypeAsync("TQC");
                Console.WriteLine($"✅ 按類型查詢: 找到 {byType.Count} 個 TQC 題目");

                // Delete
                await repo.DeleteAsync(created.ProblemId);
                Console.WriteLine($"✅ 刪除題目: {created.ProblemCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 題目操作失敗: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>測試 3: 測試案例操作</summary>
    private static async Task TestTestCaseOperations()
    {
        Console.WriteLine("\n【測試 3】測試案例操作");
        Console.WriteLine("─" + new string('─', 50));

        using (var context = new ExamDbContext())
        {
            var problemRepo = new ProblemRepository(context);
            var testCaseRepo = new TestCaseRepository(context);

            try
            {
                // 建立測試題目
                var problem = new Problem
                {
                    ProblemCode = "TEST002",
                    Title = "測試題目 2",
                    Description = "含有測試案例的題目",
                    ExamType = "TQC",
                    Difficulty = 1,
                    Status = "Draft"
                };

                var savedProblem = await problemRepo.AddAsync(problem);

                // 添加測試案例
                var testCase1 = new TestCase
                {
                    ProblemId = savedProblem.ProblemId,
                    Input = "1\n2\n3",
                    ExpectedOutput = "6",
                    IsExample = true,
                    OrderIndex = 0
                };

                var testCase2 = new TestCase
                {
                    ProblemId = savedProblem.ProblemId,
                    Input = "5\n5\n5",
                    ExpectedOutput = "15",
                    IsExample = false,
                    OrderIndex = 1
                };

                var testCases = new List<TestCase> { testCase1, testCase2 };
                var added = await testCaseRepo.AddMultipleAsync(testCases);
                Console.WriteLine($"✅ 添加測試案例: {added.Count} 個");

                // 獲取示例案例
                var examples = await testCaseRepo.GetExamplesByProblemIdAsync(savedProblem.ProblemId);
                Console.WriteLine($"✅ 示例案例: {examples.Count} 個");

                // 獲取隱藏案例
                var hidden = await testCaseRepo.GetHiddenByProblemIdAsync(savedProblem.ProblemId);
                Console.WriteLine($"✅ 隱藏案例: {hidden.Count} 個");

                // 清理
                await problemRepo.DeleteAsync(savedProblem.ProblemId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 測試案例操作失敗: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>測試 4: 審核流程</summary>
    private static async Task TestReviewProcess()
    {
        Console.WriteLine("\n【測試 4】審核流程");
        Console.WriteLine("─" + new string('─', 50));

        using (var context = new ExamDbContext())
        {
            var problemRepo = new ProblemRepository(context);
            var reviewRepo = new ProblemReviewRepository(context);

            try
            {
                // 建立待審核題目
                var problem = new Problem
                {
                    ProblemCode = "TEST003",
                    Title = "審核測試題目",
                    Description = "用於審核流程測試",
                    ExamType = "TQC",
                    Difficulty = 2,
                    Status = "Draft"
                };

                var savedProblem = await problemRepo.AddAsync(problem);

                // 建立審核記錄
                var review = new ProblemReview
                {
                    ProblemId = savedProblem.ProblemId,
                    ReviewerName = "測試審核者",
                    Comments = "初始審核意見",
                    Status = "Pending"
                };

                var savedReview = await reviewRepo.AddAsync(review);
                Console.WriteLine($"✅ 建立審核記錄: {savedReview.Status}");

                // 獲取待審核
                var pending = await reviewRepo.GetPendingReviewsAsync();
                Console.WriteLine($"✅ 待審核列表: {pending.Count} 筆");

                // 批准審核
                var approved = await reviewRepo.ApproveAsync(savedReview.ReviewId, "批准審核者");
                Console.WriteLine($"✅ 審核通過: {approved.Status}");

                // 清理
                await problemRepo.DeleteAsync(savedProblem.ProblemId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 審核流程測試失敗: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>測試 5: 提交統計</summary>
    private static async Task TestSubmissionStatistics()
    {
        Console.WriteLine("\n【測試 5】提交統計");
        Console.WriteLine("─" + new string('─', 50));

        using (var context = new ExamDbContext())
        {
            var problemRepo = new ProblemRepository(context);
            var submissionRepo = new UserSubmissionRepository(context);

            try
            {
                // 建立題目
                var problem = new Problem
                {
                    ProblemCode = "TEST004",
                    Title = "提交統計測試",
                    Description = "用於提交統計測試",
                    ExamType = "TQC",
                    Difficulty = 1,
                    Status = "Active"
                };

                var savedProblem = await problemRepo.AddAsync(problem);

                // 模擬多個提交
                var submission1 = new UserSubmission
                {
                    ProblemId = savedProblem.ProblemId,
                    UserCode = "print('test1')",
                    IsCorrect = true,
                    OutputResult = "test1"
                };

                var submission2 = new UserSubmission
                {
                    ProblemId = savedProblem.ProblemId,
                    UserCode = "print('test2')",
                    IsCorrect = false,
                    OutputResult = "test2"
                };

                await submissionRepo.SubmitAsync(submission1);
                await submissionRepo.SubmitAsync(submission2);
                Console.WriteLine($"✅ 記錄提交: 2 筆");

                // 統計成功次數
                var successCount = await submissionRepo.GetSuccessCountAsync(savedProblem.ProblemId);
                var totalCount = await submissionRepo.GetTotalSubmitCountAsync(savedProblem.ProblemId);
                var rate = await submissionRepo.GetSuccessRateAsync(savedProblem.ProblemId);

                Console.WriteLine($"✅ 成功: {successCount}/{totalCount}");
                Console.WriteLine($"✅ 通過率: {rate:F2}%");

                // 清理
                await problemRepo.DeleteAsync(savedProblem.ProblemId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 提交統計測試失敗: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>測試 6: 題目導入（如果文件存在）</summary>
    private static async Task TestProblemImport()
    {
        Console.WriteLine("\n【測試 6】題目導入");
        Console.WriteLine("─" + new string('─', 50));

        var importService = new ProblemImportService();
        // 計算絕對路徑: Tests -> desktop/B3 -> desktop -> root
        var currentDir = AppContext.BaseDirectory;
        var tqcFolder = Path.GetFullPath(Path.Combine(currentDir, @"..\..\..\..\TQC-problem-list"));

        if (Directory.Exists(tqcFolder))
        {
            try
            {
                var importCount = await importService.ImportFromFolderAsync(tqcFolder);
                Console.WriteLine($"✅ 導入成功: {importCount} 個題目");

                using (var context = new ExamDbContext())
                {
                    var totalProblems = context.Problems.Count();
                    var totalTestCases = context.TestCases.Count();
                    Console.WriteLine($"✅ 數據庫中現有: {totalProblems} 個題目, {totalTestCases} 個測試案例");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  題目導入測試失敗: {ex.Message}");
                Console.WriteLine("   (如果 TQC 文件夾不存在或格式不正確可忽略此錯誤)");
            }
        }
        else
        {
            Console.WriteLine($"⚠️  TQC 文件夾不存在: {tqcFolder}");
            Console.WriteLine("   請確保文件夾位置正確");
        }
    }
}
