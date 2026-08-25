using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace B3.Localization;

/// <summary>
/// 介面語言服務 - 提供字串表與執行期語言切換
/// XAML 透過 {loc:Loc Key} 繫結索引子，語言切換時自動更新
/// </summary>
public class LocalizationService : INotifyPropertyChanged
{
    public static LocalizationService Instance { get; } = new();

    public const string ZhTw = "zh-TW";
    public const string EnUs = "en-US";

    private string _currentLanguage = ZhTw;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>語言切換事件 - ViewModel 需重建顯示集合時訂閱</summary>
    public event EventHandler? LanguageChanged;

    /// <summary>目前語言代碼 (zh-TW / en-US)</summary>
    public string CurrentLanguage => _currentLanguage;

    /// <summary>依鍵值取得目前語言的字串</summary>
    public string this[string key]
    {
        get
        {
            if (Strings.TryGetValue(key, out var pair))
            {
                return _currentLanguage == EnUs ? pair.En : pair.Zh;
            }
            return key;
        }
    }

    /// <summary>切換語言並通知所有繫結更新</summary>
    public void SetLanguage(string? languageCode)
    {
        var normalized = languageCode == EnUs ? EnUs : ZhTw;
        if (_currentLanguage == normalized)
        {
            return;
        }

        _currentLanguage = normalized;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguage)));
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>取得字串的靜態捷徑 (供 ViewModel 使用)</summary>
    public static string T(string key) => Instance[key];

    private static readonly Dictionary<string, (string Zh, string En)> Strings = new()
    {
        // 主視窗
        ["AppTitle"] = ("智慧考試系統", "Smart Exam System"),
        ["AppName"] = ("智慧考照", "Smart Exam"),
        ["MainMenu"] = ("主選單", "Main Menu"),
        ["Tools"] = ("工具", "Tools"),
        ["Settings"] = ("設定", "Settings"),
        ["NavHome"] = ("開始畫面", "Home"),
        ["NavQuestion"] = ("題目", "Problems"),
        ["NavBank"] = ("已載入題庫", "Loaded Banks"),
        ["NavImport"] = ("匯入", "Import"),
        ["NavStyle"] = ("樣式", "Style"),

        // 共用
        ["Home"] = ("首頁", "Home"),
        ["Edit"] = ("修改", "Edit"),
        ["Delete"] = ("刪除", "Delete"),
        ["Remove"] = ("移除", "Remove"),
        ["Cancel"] = ("取消", "Cancel"),
        ["Language"] = ("語言", "Language"),
        ["Difficulty"] = ("難度", "Difficulty"),
        ["Status"] = ("狀態", "Status"),
        ["Search"] = ("搜尋", "Search"),
        ["QuestionsFmt"] = ("{0} 題", "{0} questions"),
        ["MinutesFmt"] = ("{0} 分鐘", "{0} min"),
        ["CompletedFmt"] = ("已完成 {0}%", "{0}% completed"),
        ["DiffEasy"] = ("簡單", "Easy"),
        ["DiffMedium"] = ("中等", "Medium"),
        ["DiffHard"] = ("困難", "Hard"),
        ["DiffUnknown"] = ("未知", "Unknown"),

        // 設定頁
        ["SettingsSubtitle"] = ("系統設定", "System Settings"),
        ["RestoreDefaults"] = ("恢復預設", "Restore Defaults"),
        ["SaveChanges"] = ("儲存變更", "Save Changes"),
        ["SectionGeneral"] = ("一般", "General"),
        ["SectionExam"] = ("考試設定", "Exam Settings"),
        ["SectionKey"] = ("快捷鍵", "Shortcuts"),
        ["SectionData"] = ("資料管理", "Data Management"),
        ["SectionAi"] = ("AI 模型", "AI Model"),
        ["SectionAbout"] = ("關於", "About"),
        ["GeneralHeader"] = ("一般設定", "General Settings"),
        ["UiLanguageTitle"] = ("介面語言", "Interface Language"),
        ["UiLanguageDesc"] = ("選擇應用程式的顯示語言", "Choose the display language of the app"),
        ["ExamBehavior"] = ("考試行為", "Exam Behavior"),
        ["ShowAnswerTitle"] = ("作答後立即顯示答案", "Show answer right after submitting"),
        ["ShowAnswerDesc"] = ("不勾選將在最後統一顯示", "If off, answers are shown at the end"),
        ["CountdownTitle"] = ("倒數計時器", "Countdown timer"),
        ["CountdownDesc"] = ("考試頁面顯示剩餘時間", "Show remaining time during exams"),
        ["ShuffleTitle"] = ("題目隨機排序", "Shuffle questions"),
        ["ShuffleDesc"] = ("每次練習重新洗牌題目順序", "Reshuffle question order each practice"),
        ["QuestionsPerExamTitle"] = ("每次練習題數", "Questions per practice"),
        ["QuestionsPerExamDesc"] = ("單次練習的題目數量", "Number of questions per session"),
        ["DifficultyFilterTitle"] = ("難度篩選", "Difficulty filter"),
        ["DifficultyFilterDesc"] = ("優先出現的題目難易度", "Preferred question difficulty"),
        ["KeyboardShortcuts"] = ("鍵盤快捷鍵", "Keyboard Shortcuts"),
        ["DataManagement"] = ("資料管理", "Data Management"),
        ["RuntimeEnvironment"] = ("執行環境", "Runtime Environment"),
        ["PythonPath"] = ("Python 路徑", "Python Path"),
        ["CppCompiler"] = ("C++ 編譯器", "C++ Compiler"),
        ["DotNetPath"] = ("DotNet 路徑", "DotNet Path"),
        ["DefaultLanguage"] = ("預設語言", "Default Language"),
        ["AiModel"] = ("AI 模型", "AI Model"),
        ["AiProviderTitle"] = ("AI 服務供應商", "AI Provider"),
        ["AiProviderDesc"] = ("選擇要串接的 AI API 服務", "Choose which AI API to connect"),
        ["ApiKeyLabel"] = ("API Key", "API Key"),
        ["OpenAiEndpoint"] = ("OpenAI 端點", "OpenAI Endpoint"),
        ["OllamaEndpoint"] = ("Ollama 端點", "Ollama Endpoint"),
        ["ModelName"] = ("模型名稱", "Model Name"),
        ["UseLocalTransformers"] = ("使用本地 Transformers 模型", "Use local Transformers model"),
        ["LocalModelPath"] = ("本地模型路徑/名稱", "Local model path / name"),
        ["AiApplyNote"] = ("儲存後會套用至 AI 問答與程式分析", "Applied to AI Q&A and code analysis after saving"),
        ["AboutTitle"] = ("關於智慧考照", "About Smart Exam"),
        ["Version"] = ("版本", "Version"),
        ["BankLastUpdated"] = ("題庫最後更新", "Bank last updated"),
        ["CheckUpdates"] = ("檢查更新", "Check for updates"),
        ["Check"] = ("檢查", "Check"),
        ["Feedback"] = ("意見回饋", "Feedback"),
        ["FeedbackAction"] = ("回饋", "Send"),

        // 快捷鍵清單
        ["KbRunCode"] = ("執行程式碼", "Run code"),
        ["KbNext"] = ("下一題", "Next question"),
        ["KbPrev"] = ("上一題", "Previous question"),
        ["KbMark"] = ("標記題目", "Mark question"),
        ["KbToggleAnswer"] = ("顯示/隱藏答案", "Show / hide answer"),
        ["KbHome"] = ("回到首頁", "Back to home"),
        ["KbOpenSettings"] = ("開啟設定", "Open settings"),

        // 資料管理操作
        ["DaExportTitle"] = ("匯出練習紀錄", "Export practice records"),
        ["DaExportDesc"] = ("將歷史答題資料匯出為 CSV", "Export answer history as CSV"),
        ["DaExport"] = ("匯出", "Export"),
        ["DaBackupTitle"] = ("備份題庫設定", "Back up bank settings"),
        ["DaBackupDesc"] = ("備存目前所有題庫設定", "Back up all current bank settings"),
        ["DaBackup"] = ("備份", "Back up"),
        ["DaClearTitle"] = ("清除快取", "Clear cache"),
        ["DaClearDesc"] = ("清除暫存資料 (不影響題庫)", "Clear temporary data (banks unaffected)"),
        ["DaClear"] = ("清除", "Clear"),
        ["DaResetTitle"] = ("重置所有練習進度", "Reset all practice progress"),
        ["DaResetDesc"] = ("此操作無法復原，請謹慎使用", "This cannot be undone; use with caution"),
        ["DaReset"] = ("重置", "Reset"),

        // 首頁
        ["QuickSim"] = ("快速模擬", "Quick Practice"),
        ["QuickSimDesc"] = ("選擇您需要的考照類型，直接開始練習。", "Pick the certification you need and start practicing right away."),
        ["ImportBank"] = ("匯入題庫", "Import Bank"),
        ["StartPractice"] = ("開始練習", "Start Practice"),
        ["HotBanks"] = ("考照題庫", "Exam Banks"),
        ["HotBanksDesc"] = ("點選題庫卡片查看詳情，或於上方直接選擇並開始練習", "Click a bank card for details, or pick one above and start practicing directly"),
        ["NoBanksTitle"] = ("尚無題庫", "No banks yet"),
        ["NoBanksDesc"] = ("匯入第一份題庫 CSV / XLS / XLSX，即可在這裡看到練習卡片。", "Import your first CSV / XLS / XLSX bank to see practice cards here."),

        // 題庫頁
        ["SearchBankName"] = ("搜尋題庫名稱...", "Search bank name..."),
        ["SyncUpdate"] = ("同步更新", "Sync"),
        ["AddBank"] = ("新增題庫", "Add Bank"),
        ["LoadedBanks"] = ("已載入題庫", "Loaded Banks"),
        ["TotalQuestions"] = ("題目總數", "Total Questions"),
        ["AnsweredQuestions"] = ("已作答題數", "Answered"),
        ["OverallAccuracy"] = ("整體正確率", "Overall Accuracy"),
        ["OfficialBanks"] = ("官方題庫", "Official Banks"),
        ["OfficialBanksDesc"] = ("點選題庫卡片可展開內部題目列表。", "Click a bank card to expand its problem list."),
        ["InnerProblemList"] = ("內部題目列表", "Problems in Bank"),
        ["CustomBanks"] = ("自訂題庫", "Custom Banks"),
        ["CustomBanksEmpty"] = ("目前尚未綁定自訂題庫資料。", "No custom bank data bound yet."),
        ["UpdatedFmt"] = ("更新於 {0}", "Updated {0}"),
        ["PercentDoneFmt"] = ("{0}% 完成", "{0}% done"),
        ["CreatedAtFmt"] = ("新增日期：{0:yyyy-MM-dd HH:mm}", "Added: {0:yyyy-MM-dd HH:mm}"),

        // 題目列表頁
        ["Filter"] = ("篩選", "Filters"),
        ["ExamType"] = ("考照類型", "Exam Type"),
        ["TestCases"] = ("測試案例", "Test Cases"),
        ["FileMenu"] = ("檔案選單", "Files"),
        ["EditorHints"] = ("編輯器提示", "Editor Hints"),
        ["Compile"] = ("編譯", "Compile"),
        ["CompileOptions"] = ("編譯選項", "Compile Options"),
        ["CompileNote"] = ("目前為模擬編譯；之後可接上實際編譯服務。", "Compilation is simulated for now; a real compile service can be attached later."),
        ["TypeFmt"] = ("類型：{0}", "Type: {0}"),
        ["DifficultyFmt"] = ("難度：{0}", "Difficulty: {0}"),
        ["CodeFmt"] = ("代碼：{0}", "Code: {0}"),
        ["ExamKindFmt"] = ("考照種類：{0}", "Exam type: {0}"),

        // 考試頁
        ["ExamSetupTitle"] = ("模擬考試設定", "Mock Exam Setup"),
        ["ExamSetupDesc"] = ("確認題庫、題數與考試時間後再開始。", "Confirm the bank, question count and duration before starting."),
        ["ExamInfo"] = ("考試資訊", "Exam Info"),
        ["SettingItems"] = ("設定項目", "Settings"),
        ["ExamKind"] = ("考試種類", "Exam Type"),
        ["QuestionCount"] = ("考題數量", "Question Count"),
        ["ExamTime"] = ("考試時間", "Duration"),
        ["BankStatus"] = ("題庫狀態", "Bank Status"),
        ["AvailableFmt"] = ("可用題庫：{0} 題", "Available: {0} questions"),
        ["ThisCountFmt"] = ("本次題數：{0} 題", "This session: {0} questions"),
        ["TimePreviewFmt"] = ("時間預覽：{0}", "Time preview: {0}"),
        ["Actions"] = ("操作", "Actions"),
        ["ConfirmStart"] = ("確認開始", "Start"),
        ["RemainingFmt"] = ("剩餘時間：{0}", "Time left: {0}"),
        ["ProblemNoFmt"] = ("題號：{0}", "Question: {0}"),
        ["TotalFmt"] = ("總題數：{0}", "Total: {0}"),
        ["SampleTestCases"] = ("示例測試案例", "Sample Test Cases"),
        ["RunCode"] = ("試跑程式", "Run Code"),
        ["ExecResult"] = ("執行結果", "Output"),
        ["JudgeStatus"] = ("判題狀態", "Judge Status"),
        ["SubmitAnswer"] = ("提交答案", "Submit"),
        ["Skip"] = ("跳過", "Skip"),
        ["EndExam"] = ("結束考試", "End Exam"),

        // 考試結算頁
        ["ExamResult"] = ("考試結算", "Exam Results"),
        ["GradeFmt"] = ("等級：{0}", "Grade: {0}"),
        ["CorrectFmt"] = ("答對：{0} 題", "Correct: {0}"),
        ["AnsweredFmt"] = ("作答：{0} 題", "Answered: {0}"),
        ["TotalBankFmt"] = ("題庫總題數：{0} 題", "Total in bank: {0}"),
        ["ElapsedFmt"] = ("花費時間：{0}", "Time spent: {0}"),
        ["AccuracyFmt"] = ("答對率 {0}%", "Accuracy {0}%"),
        ["ReturnHome"] = ("返回首頁", "Back to Home"),
        ["ThisExamProblems"] = ("本次考題", "Exam Problems"),
        ["SubmissionLog"] = ("提交記錄", "Submissions"),
        ["ProblemFmt"] = ("題目：{0}", "Problem: {0}"),
        ["IsCorrectFmt"] = ("是否正確：{0}", "Correct: {0}"),
        ["AiQa"] = ("AI 問答", "AI Q&A"),
        ["AiQuestionPlaceholder"] = ("輸入想詢問的題目或程式碼觀念", "Ask about a problem or coding concept"),
        ["SendQuestion"] = ("送出問題", "Send"),
        ["AnalyzeCode"] = ("分析程式", "Analyze Code"),
        ["AiAnswer"] = ("AI 回答", "AI answer"),
        ["AiAnalysis"] = ("AI 程式分析", "AI code analysis"),

        // 匯入頁
        ["ImportBankTitle"] = ("匯入題庫", "Import Question Bank"),
        ["ImportDesc"] = ("支援單題手動新增，以及 CSV / XLS / XLSX 表單匯入。", "Supports manual single-problem entry and CSV / XLS / XLSX imports."),
        ["SupportedFmt"] = ("支援格式：{0}", "Supported formats: {0}"),
        ["TabSingle"] = ("單題新增", "Single Problem"),
        ["TabSheet"] = ("表單匯入", "Spreadsheet Import"),
        ["BasicInfo"] = ("題目基本資料", "Problem Basics"),
        ["ProblemCode"] = ("題目代碼", "Problem Code"),
        ["ProblemCodePlaceholder"] = ("例如：PYD101", "e.g. PYD101"),
        ["TitleLabel"] = ("題目標題", "Title"),
        ["SolutionLanguage"] = ("解法語言", "Solution Language"),
        ["Description"] = ("題目敘述", "Description"),
        ["DescriptionPlaceholder"] = ("請輸入題目敘述", "Enter the problem description"),
        ["SolutionContent"] = ("解法內容", "Solution"),
        ["SolutionPlaceholder"] = ("貼上解法程式碼", "Paste solution code"),
        ["TestAndValidation"] = ("測試資料與驗證資料", "Test & Validation Data"),
        ["TotalRowsFmt"] = ("共 {0} 筆", "{0} total"),
        ["Add"] = ("新增", "Add"),
        ["AddTestCase"] = ("新增測試資料", "Add Test Case"),
        ["ResetForm"] = ("重設", "Reset"),
        ["ImportUpdate"] = ("匯入 / 更新", "Import / Update"),
        ["Order"] = ("順序", "Order"),
        ["TestData"] = ("測試資料", "Test Input"),
        ["TestDataPlaceholder"] = ("輸入資料", "Input data"),
        ["ValidationData"] = ("驗證資料", "Expected Output"),
        ["ValidationDataPlaceholder"] = ("期望輸出", "Expected output"),
        ["Example"] = ("範例", "Example"),
        ["Show"] = ("顯示", "Show"),
        ["FileImportTemplate"] = ("檔案匯入與模板", "File Import & Template"),
        ["SheetRowDesc"] = ("每一列代表一筆測試資料，相同 ProblemCode 的列會合併成同一題。", "Each row is one test case; rows with the same ProblemCode merge into one problem."),
        ["AddRow"] = ("新增一列", "Add Row"),
        ["PickFile"] = ("選擇 CSV / XLS / XLSX", "Choose CSV / XLS / XLSX"),
        ["ValidateRows"] = ("驗證目前列", "Validate Rows"),
        ["PreviewValidate"] = ("預覽 / 驗證", "Preview / Validate"),
        ["DownloadTemplate"] = ("下載模板", "Download Template"),
        ["NoFileSelected"] = ("尚未選擇檔案", "No file selected"),
        ["TemplateSpec"] = ("模板規格", "Template Spec"),
        ["PreviewResult"] = ("預覽結果", "Preview"),
        ["RowFmt"] = ("第 {0} 列", "Row {0}"),
        ["FieldFmt"] = ("欄位：{0}", "Field: {0}"),
        ["ValidationMessages"] = ("驗證訊息", "Validation Messages"),

        // 審核頁
        ["PendingProblems"] = ("待審核題目", "Pending Review"),
        ["Reviewer"] = ("審核者:", "Reviewer:"),
        ["ReviewerPlaceholder"] = ("輸入審核者名稱", "Enter reviewer name"),
        ["ReviewComments"] = ("審核意見:", "Review comments:"),
        ["ReviewCommentsPlaceholder"] = ("輸入審核意見...", "Enter review comments..."),
        ["ApproveReview"] = ("通過審核", "Approve"),
        ["RejectReview"] = ("拒絕審核", "Reject"),
        ["UpdateComments"] = ("更新意見", "Update Comments"),

        // 題目頁 (預留)
        ["QuestionHubTitle"] = ("題目", "Problems"),
        ["QuestionHubDesc"] = ("題目瀏覽與練習入口預留", "Placeholder for problem browsing and practice entry"),

        // 樣式頁
        ["StyleSubtitle"] = ("樣式設定", "Style Settings"),
        ["UiTheme"] = ("介面主題", "UI Theme"),
        ["FontAndSize"] = ("字體與大小", "Font & Size"),
        ["UiFont"] = ("介面字體", "UI Font"),
        ["FontSize"] = ("字體大小", "Font Size"),
        ["OtherOptions"] = ("其他選項", "Other Options"),
        ["ShowProgressBar"] = ("顯示進度條", "Show progress bar"),
        ["LivePreview"] = ("即時預覽", "Live Preview"),
        ["CurrentSummary"] = ("目前設定摘要", "Current Settings Summary"),
        ["ThemeFmt"] = ("主題：{0}", "Theme: {0}"),
        ["FontFmt"] = ("字體：{0}", "Font: {0}"),
        ["FontSizeFmt"] = ("字級：{0}", "Font size: {0}"),
        ["ShowProgressFmt"] = ("顯示進度條：{0}", "Show progress bar: {0}"),
        ["TagRecommended"] = ("推薦", "Featured"),
        ["TagHot"] = ("熱門", "Hot"),
        ["TagNew"] = ("新增", "New"),
        ["PreviewSampleDesc"] = ("台灣學術能力測驗，涵蓋語文、數學、自然及社會科目模擬練習。", "Sample mock exam covering language, math, science and social studies."),
    };
}
