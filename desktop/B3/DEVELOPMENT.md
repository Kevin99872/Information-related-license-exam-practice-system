# B3 智慧考試系統 - 開發文檔

## 項目概述
基於Avalonia MVVM框架的考照練習系統，支持TQC、CPE等多種證照題目。本地SQLite數據庫存儲，無需網絡環境。

## 已完成模塊

### 1. 數據庫層 (Data)
✅ **2NF正規化設計**
- **問題表 (Problems)**: ProblemId主鍵 + ProblemCode唯一索引
- **測試案例表 (TestCases)**: 分離輸入輸出 實現1:多關係
- **審核記錄表 (ProblemReviews)**: 題目審核機制 Pending->Approved/Rejected流程
- **提交記錄表 (UserSubmissions)**: 用戶答題結果統計

✅ **Repository模式**
- ProblemRepository: 題目CRUD和篩選
- TestCaseRepository: 測試案例管理 支持示例/隱藏分離
- ProblemReviewRepository: 審核流程管理
- UserSubmissionRepository: 提交記錄查詢和統計

✅ **數據庫上下文**
- ExamDbContext: Entity映射和關係配置
- SQLite本地存儲路徑: `%AppData%/B3ExamSystem/exam.db`

### 2. 模型層 (Models)
✅ 四個Entity類
- Problem: 題目信息 包含狀態和時間戳
- TestCase: 測試案例 支持示例標記和排序
- ProblemReview: 審核記錄 完整的審核工作流
- UserSubmission: 提交記錄 含輸出結果

### 3. ViewModel層 (MVVM)
✅ **MainWindowViewModel**
- 頁面導航管理
- 全局狀態控制

✅ **ProblemListViewModel**
- 題目瀏覽和篩選 (按考照類型/狀態/關鍵字)
- 題目列表加載和排序
- 新增/刪除題目操作

✅ **ExamViewModel**
- 考試流程管理
- 計時器實現 (倒計時)
- 題目隨機抽選
- 答案提交判題接口

✅ **ReviewViewModel**
- 待審核題目列表
- 審核通過/拒絕流程
- 審核意見編輯

### 4. 視圖層 (Views)
✅ **ProblemListView.axaml**
- 題目瀏覽界面
- 篩選工具欄 (考照類型/狀態/搜尋)
- 題目DataGrid列表

✅ **ExamView.axaml**
- 考試界面
- 計時器顯示
- 代碼編輯區
- 測試案例展示
- 執行結果輸出

✅ **ReviewView.axaml**
- 待審核題目列表
- 審核詳情面板
- 審核操作按鈕

✅ **MainWindow.axaml**
- 主窗口導航
- 頁面切換菜單

### 5. 服務層 (Services)
✅ **ProblemImportService**
- 從TQC文本文件解析題目
- 自動提取題目信息和測試案例
- 批量導入到數據庫

🔄 **CodeJudgeService** (架構就位 待實現)
- Python程式碼執行
- C#程式碼執行
- 輸出比對邏輯

## 架構設計特點

### 2NF正規化
- 消除部分函數依賴
- 測試案例獨立表存儲 (從表)
- 審核記錄完全獨立 (從表)
- 每個從表完全依賴於主表ProblemId

### 開發守則遵循
✅ 最簡命名 - 類名/方法名簡潔明確
✅ Function前標註使用說明 - 所有public方法均有XML文檔註釋
✅ 標註連結函式 - 導航屬性清晰
✅ TODO標註 - 未完成項目清楚標記
✅ 可擴充題庫 - Repository模式便於新增題目來源
✅ 方便修改 - 配置集中 邏輯分層

## 待實現功能 (TODO)

### 優先級 P1 (核心功能)
- [ ] CodeJudgeService: 程式碼執行引擎
  - Python執行環境集成
  - 輸出比對算法
  - 超時和錯誤處理
- [ ] 主窗口導航邏輯 - 頁面切換實現
- [ ] 考試流程完整化 - 時間到自動結束

### 優先級 P2 (重要功能)
- [ ] 數據庫遷移策略 - 版本控制
- [ ] 批量題目導入 - TQC問題文件夾處理
- [ ] 成績評測畫面 - 考試結束後統計展示
- [ ] AI問答集成 - prompt工程實現
- [ ] 代碼漏洞分析 - 一鍵分析功能

### 優先級 P3 (增強功能)
- [ ] 設定頁面 - API_KEY/主題/字體配置
- [ ] 本地AI模型集成 - Ollama等
- [ ] 多語言支持
- [ ] 深色模式
- [ ] 用戶進度統計

## 項目結構
```
B3/
├── Data/
│   ├── ExamDbContext.cs          - EF Core上下文
│   ├── ProblemRepository.cs      - 題目存儲庫
│   ├── TestCaseRepository.cs     - 測試案例存儲庫
│   ├── ProblemReviewRepository.cs- 審核存儲庫
│   └── UserSubmissionRepository.cs- 提交存儲庫
├── Models/
│   ├── Problem.cs                - 題目模型
│   ├── TestCase.cs               - 測試案例模型
│   ├── ProblemReview.cs          - 審核模型
│   └── UserSubmission.cs         - 提交模型
├── ViewModels/
│   ├── MainWindowViewModel.cs    - 主窗口VM
│   ├── ProblemListViewModel.cs   - 題目列表VM
│   ├── ExamViewModel.cs          - 考試VM
│   ├── ReviewViewModel.cs        - 審核VM
│   └── ViewModelBase.cs          - 基類
├── Views/
│   ├── MainWindow.axaml          - 主窗口
│   ├── ProblemListView.axaml     - 題目列表視圖
│   ├── ExamView.axaml            - 考試視圖
│   └── ReviewView.axaml          - 審核視圖
├── Services/
│   ├── ProblemImportService.cs   - 題目導入服務
│   └── CodeJudgeService.cs       - 程式碼判題服務
└── B3.csproj                     - 項目文件
```

## 使用依賴
- Avalonia 12.0.2 - UI框架
- CommunityToolkit.Mvvm 8.4.1 - MVVM工具包
- Microsoft.EntityFrameworkCore.Sqlite 8.0.0 - 數據庫ORM
- .NET 10.0 - 運行環境

## 下一步開發建議
1. 實現CodeJudgeService的Python執行邏輯
2. 完成主窗口頁面導航
3. 導入現有TQC問題文件到數據庫
4. 測試考試流程端到端
5. 實現成績評測和統計功能

---
*開發守則來源: Delvaptor rule.md*
*最後更新: 2026-05-11*
