using Microsoft.EntityFrameworkCore;
using B3.Models;
using System;
using System.IO;
using System.Linq;

namespace B3.Data;

/// <summary>
/// 考試種類目錄資料庫 - 獨立 SQLite (catalog.db)
/// 負責儲存「考試種類」與「介紹名稱」等題庫中繼資料，與題目資料庫 exam.db 分離
/// </summary>
public class ExamCatalogDbContext : DbContext
{
    private static readonly string DbPath = ResolveCatalogDatabasePath();

    public DbSet<ExamCategory> ExamCategories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ExamCategory>(entity =>
        {
            entity.HasKey(e => e.ExamCategoryId);
            entity.HasIndex(e => e.ExamType).IsUnique();
            entity.Property(e => e.ExamType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Tag).HasMaxLength(20);
        });
    }

    /// <summary>初始化目錄資料庫，第一次執行時建立結構並寫入預設考試種類</summary>
    public static void Initialize()
    {
        var dbDir = Path.GetDirectoryName(DbPath);
        if (!string.IsNullOrWhiteSpace(dbDir) && !Directory.Exists(dbDir))
        {
            Directory.CreateDirectory(dbDir);
        }

        using var context = new ExamCatalogDbContext();
        context.Database.EnsureCreated();
        SeedDefaults(context);
    }

    /// <summary>若目錄為空，寫入預設考試種類資料</summary>
    private static void SeedDefaults(ExamCatalogDbContext context)
    {
        if (context.ExamCategories.Any())
        {
            return;
        }

        context.ExamCategories.AddRange(
            new ExamCategory { ExamType = "TQC", Title = "TQC+ Python3", Tag = "熱門", IsHot = true, SortOrder = 0, DurationMinutes = 180, Description = "TQC+ Python 考驗對於 Python 的基礎程式邏輯與演算法能力。" },
            new ExamCategory { ExamType = "CPE", Title = "CPE", Tag = "新增", IsHot = true, SortOrder = 1, DurationMinutes = 150, Description = "大學程式設計先修檢測，考驗 C/C++ 程式邏輯與演算法能力。共 1-5 級題目。" },
            new ExamCategory { ExamType = "APCS", Title = "APCS", Tag = "熱門", IsHot = true, SortOrder = 2, DurationMinutes = 120, Description = "APCS 程式設計先修檢測，涵蓋各級程度的演算法與資料結構題庫。" },
            new ExamCategory { ExamType = "Software", Title = "電腦軟體設計 丙級技術士", Tag = "", IsHot = false, SortOrder = 3, DurationMinutes = 100, Description = "技術士技能檢定丙級，涵蓋各類軟體工程師基本演算法題庫。" }
        );

        context.SaveChanges();
    }

    /// <summary>取得目錄資料庫路徑 (與 exam.db 同目錄)</summary>
    public static string GetDatabasePath() => DbPath;

    /// <summary>解析專案內部 data/catalog.db 路徑</summary>
    private static string ResolveCatalogDatabasePath()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 6 && current != null; i++)
        {
            var dataDir = Path.Combine(current.FullName, "data");
            if (Directory.Exists(dataDir))
            {
                return Path.Combine(dataDir, "catalog.db");
            }
            current = current.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, "catalog.db");
    }
}
