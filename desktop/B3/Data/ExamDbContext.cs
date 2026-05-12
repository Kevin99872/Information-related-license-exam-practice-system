using Microsoft.EntityFrameworkCore;
using B3.Models;
using System;
using System.IO;

namespace B3.Data;

/// <summary>
/// 數據庫上下文 - 配置SQLite連接及Entity映射
/// 2NF正規化: Problems主表 + TestCases/ProblemReviews/UserSubmissions從表
/// </summary>
public class ExamDbContext : DbContext
{
    private static readonly string DbPath = ResolveProjectDatabasePath();

    public DbSet<Problem> Problems { get; set; }
    public DbSet<TestCase> TestCases { get; set; }
    public DbSet<ProblemReview> ProblemReviews { get; set; }
    public DbSet<UserSubmission> UserSubmissions { get; set; }

    /// <summary>
    /// 配置數據庫連接 使用本地SQLite
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var connectionString = $"Data Source={DbPath}";
        options.UseSqlite(connectionString);
    }

    /// <summary>
    /// 配置Entity映射 定義主鍵 外鍵 唯一約束等
    /// 2NF設計: 消除部分函數依賴 完全依賴主鍵
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Problem配置 主表
        modelBuilder.Entity<Problem>(entity =>
        {
            entity.HasKey(e => e.ProblemId);
            entity.HasIndex(e => e.ProblemCode).IsUnique();
            entity.Property(e => e.ProblemCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.ExamType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.HasMany(e => e.TestCases).WithOne(e => e.Problem).HasForeignKey(e => e.ProblemId);
            entity.HasMany(e => e.Reviews).WithOne(e => e.Problem).HasForeignKey(e => e.ProblemId);
            entity.HasMany(e => e.Submissions).WithOne(e => e.Problem).HasForeignKey(e => e.ProblemId);
        });

        // TestCase配置 分離輸入輸出 實現2NF
        modelBuilder.Entity<TestCase>(entity =>
        {
            entity.HasKey(e => e.TestCaseId);
            entity.Property(e => e.Input).IsRequired();
            entity.Property(e => e.ExpectedOutput).IsRequired();
            entity.HasIndex(e => new { e.ProblemId, e.OrderIndex });
        });

        // ProblemReview配置 題目審核機制
        modelBuilder.Entity<ProblemReview>(entity =>
        {
            entity.HasKey(e => e.ReviewId);
            entity.Property(e => e.ReviewerName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Comments).IsRequired();
            entity.HasIndex(e => new { e.ProblemId, e.ReviewedAt });
        });

        // UserSubmission配置 用戶提交記錄
        modelBuilder.Entity<UserSubmission>(entity =>
        {
            entity.HasKey(e => e.SubmissionId);
            entity.Property(e => e.UserCode).IsRequired();
            entity.Property(e => e.OutputResult).IsRequired();
            entity.HasIndex(e => new { e.ProblemId, e.SubmittedAt });
        });
    }

    /// <summary>
    /// 初始化數據庫 第一次運行時自動建立表結構
    /// </summary>
    public static void Initialize()
    {
        // 確保數據庫目錄存在
        var dbDir = Path.GetDirectoryName(DbPath);
        if (string.IsNullOrWhiteSpace(dbDir))
        {
            return;
        }

        if (!Directory.Exists(dbDir))
        {
            Directory.CreateDirectory(dbDir);
        }

        using (var context = new ExamDbContext())
        {
            // TODO: 實現數據庫遷移邏輯
            context.Database.EnsureCreated();
            EnsureSolutionColumns(context);
        }
    }

    /// <summary>
    /// 確保解法欄位存在
    /// </summary>
    private static void EnsureSolutionColumns(ExamDbContext context)
    {
        if (!ColumnExists(context, "Problems", "SolutionLanguage"))
        {
            context.Database.ExecuteSqlRaw("ALTER TABLE Problems ADD COLUMN SolutionLanguage TEXT NOT NULL DEFAULT 'Python'");
        }

        if (!ColumnExists(context, "Problems", "SolutionCode"))
        {
            context.Database.ExecuteSqlRaw("ALTER TABLE Problems ADD COLUMN SolutionCode TEXT NOT NULL DEFAULT ''");
        }
    }

    /// <summary>
    /// 檢查欄位是否存在
    /// </summary>
    private static bool ColumnExists(ExamDbContext context, string table, string column)
    {
        var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = $"PRAGMA table_info({table});";
        context.Database.OpenConnection();
        try
        {
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var name = reader.GetString(1);
                if (string.Equals(name, column, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        finally
        {
            context.Database.CloseConnection();
        }

        return false;
    }

    /// <summary>
    /// 獲取數據庫文件路徑
    /// </summary>
    public static string GetDatabasePath() => DbPath;

    /// <summary>
    /// 取得專案內部 data/exam.db 路徑
    /// </summary>
    private static string ResolveProjectDatabasePath()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 6 && current != null; i++)
        {
            var dataDir = Path.Combine(current.FullName, "data");
            if (Directory.Exists(dataDir))
            {
                return Path.Combine(dataDir, "exam.db");
            }
            current = current.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, "exam.db");
    }
}
