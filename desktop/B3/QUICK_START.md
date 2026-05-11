# 快速參考指南

## 🚀 快速開始

### 1. 編譯專案
```bash
cd desktop/B3
dotnet build
```
✅ 預期輸出: `成功但有 2 個警告` (安全警告可忽略)

### 2. 運行應用
```bash
dotnet run
```

### 3. 首次啟動
- 應用自動建立SQLite數據庫
- 數據庫位置: `%AppData%/B3ExamSystem/exam.db`

---

## 📚 項目架構

```
UI層 (Views)
    ↓ 綁定
ViewModel層 (CommunityToolkit.MVVM)
    ↓ 調用
Service層 (業務邏輯)
    ↓ 使用
Repository層 (數據訪問)
    ↓ 操作
Database層 (SQLite via EF Core)
```

---

## 🔧 核心模塊使用

### 導入題目
```csharp
// 從文件夾批量導入
var service = new ProblemImportService();
int importedCount = await service.ImportFromFolderAsync(
    @"C:\TQC-problem-list"
);
Console.WriteLine($"導入了 {importedCount} 個題目");
```

### 查詢題目
```csharp
var repo = new ProblemRepository(dbContext);

// 查詢所有題目
var all = await repo.GetAllAsync();

// 按代碼查詢 (如PYD101)
var problem = await repo.GetByCodeAsync("PYD101");

// 按考照類型查詢
var tqcProblems = await repo.GetByExamTypeAsync("TQC");

// 獲取待審核題目
var pending = await repo.GetPendingReviewAsync();
```

### 題目審核
```csharp
var reviewRepo = new ProblemReviewRepository(dbContext);

// 獲取待審核列表
var reviews = await reviewRepo.GetPendingReviewsAsync();

// 審核通過
await reviewRepo.ApproveAsync(reviewId, "審核者名稱");

// 審核拒絕
await reviewRepo.RejectAsync(reviewId, "審核者", "不符合要求...");
```

### 用戶提交
```csharp
var submissionRepo = new UserSubmissionRepository(dbContext);

// 記錄提交
var submission = new UserSubmission 
{
    ProblemId = 1,
    UserCode = "print('hello')",
    IsCorrect = true,
    OutputResult = "hello"
};
await submissionRepo.SubmitAsync(submission);

// 查詢提交統計
int success = await submissionRepo.GetSuccessCountAsync(1);
decimal rate = await submissionRepo.GetSuccessRateAsync(1);
```

---

## 🎨 ViewModel綁定示例

### ProblemListViewModel
```xml
<ComboBox SelectedItem="{Binding SelectedExamType}"/>
<TextBox Text="{Binding SearchKeyword}"/>
<ListBox ItemsSource="{Binding Problems}"/>
```

### ExamViewModel
```xml
<TextBlock Text="{Binding RemainingSeconds}"/>
<TextBox Text="{Binding UserCode}"/>
<ListBox ItemsSource="{Binding CurrentTestCases}"/>
```

---

## 📊 2NF正規化驗證

| 表 | 主鍵 | 依賴關係 | 符合度 |
|----|------|--------|------|
| Problems | ProblemId | ✅ | 2NF |
| TestCases | TestCaseId | TestCaseId→ProblemId | ✅ |
| ProblemReviews | ReviewId | ReviewId→ProblemId | ✅ |
| UserSubmissions | SubmissionId | SubmissionId→ProblemId | ✅ |

---

## 🔍 常見操作

### 建立新題目
```csharp
var problem = new Problem
{
    ProblemCode = "PYD101",
    Title = "整數格式化輸出",
    Description = "請撰寫一程式...",
    ExamType = "TQC",
    Difficulty = 1,
    Status = "Draft"
};

var repo = new ProblemRepository(dbContext);
await repo.AddAsync(problem);
```

### 新增測試案例
```csharp
var testCase = new TestCase
{
    ProblemId = 1,
    Input = "85\n4\n299\n478",
    ExpectedOutput = "|   85     4|\n|  299   478|",
    IsExample = true,
    OrderIndex = 0
};

var tcRepo = new TestCaseRepository(dbContext);
await tcRepo.AddAsync(testCase);
```

### 查詢通過率
```csharp
var submissionRepo = new UserSubmissionRepository(dbContext);
var rate = await submissionRepo.GetSuccessRateAsync(problemId);
Console.WriteLine($"通過率: {rate:F2}%");
```

---

## 📝 開發守則檢查清單

開發新功能時檢查:

- [ ] 是否使用了最簡命名?
- [ ] 是否添加了XML文檔註釋?
- [ ] 是否標註了連結函式?
- [ ] 是否有TODO標註未完成項?
- [ ] 是否遵循了MVVM模式?
- [ ] 是否保持了2NF設計?
- [ ] 是否考慮了擴展性?

---

## ⚠️ 注意事項

1. **數據庫路徑** - 不要手動修改 `exam.db` 位置
2. **Entity Relations** - 刪除Problem時會級聯刪除所有相關記錄
3. **Repository異步** - 所有數據操作都是異步 需要使用 `await`
4. **View更新** - MVVM Property變更時View自動更新 無需手動調用

---

## 🐛 調試技巧

### 檢查數據庫
```csharp
// 查看數據庫路徑
var dbPath = ExamDbContext.GetDatabasePath();
Console.WriteLine(dbPath);
```

### 列印SQL
```csharp
// 在ExamDbContext.OnConfiguring中添加
optionsBuilder.LogTo(Console.WriteLine);
```

### 驗證數據
```csharp
var repo = new ProblemRepository(dbContext);
var all = await repo.GetAllAsync();
Console.WriteLine($"共有 {all.Count} 個題目");
```

---

## 💡 Tips

1. **篩選性能** - 使用 `GetByExamType` 等特定查詢而非 `GetAll` 後篩選
2. **批量操作** - 使用 `AddMultiple` 批量新增測試案例
3. **事務支持** - 需要多個操作原子性時使用 `DbContext.Database.BeginTransaction()`
4. **緩存考慮** - 題目信息相對穩定 可考慮實現緩存層

---

## 📞 常見問題

**Q: 如何更改數據庫位置?**
A: 修改 `Data/ExamDbContext.cs` 中的 `DbPath`

**Q: 如何添加新的Entity?**
A: 
1. 在Models中創建類
2. 在ExamDbContext中添加DbSet
3. 在OnModelCreating中配置映射

**Q: 如何支持新的程式語言?**
A: 擴展 `CodeJudgeService` 添加新的執行方法

**Q: 如何實現用戶系統?**
A: 添加User表和UserProblemProgress表的多對多關係

---

**版本**: 1.0.0 Beta
**最後更新**: 2026-05-11
