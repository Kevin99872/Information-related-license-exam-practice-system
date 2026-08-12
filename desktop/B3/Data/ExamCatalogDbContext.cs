using Microsoft.EntityFrameworkCore;
using B3.Models;
using System;
using System.IO;

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

    /// <summary>初始化目錄資料庫，第一次執行時建立結構 (不寫入任何預設考試種類)</summary>
    public static void Initialize()
    {
        var dbDir = Path.GetDirectoryName(DbPath);
        if (!string.IsNullOrWhiteSpace(dbDir) && !Directory.Exists(dbDir))
        {
            Directory.CreateDirectory(dbDir);
        }

        using var context = new ExamCatalogDbContext();
        context.Database.EnsureCreated();
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
