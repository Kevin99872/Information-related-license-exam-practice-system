using B3.Data;
using B3.Services;

namespace B3;

/// <summary>
/// 服務容器 - 集中管理應用程式的服務和數據庫上下文
/// 採用單例模式避免重複創建 提升性能
/// </summary>
public static class ServiceContainer
{
    private static ExamDbContext? _dbContext;
    private static LocalSettingsService? _settingsService;
    private static CodeJudgeService? _judgeService;
    private static OllamaService? _ollamaService;
    private static ProblemImportService? _importService;

    /// <summary>取得數據庫上下文 (單例)</summary>
    public static ExamDbContext GetDbContext()
    {
        _dbContext ??= new ExamDbContext();
        return _dbContext;
    }

    /// <summary>取得本機設定服務 (單例)</summary>
    public static LocalSettingsService GetSettingsService()
    {
        _settingsService ??= new LocalSettingsService();
        return _settingsService;
    }

    /// <summary>取得程式碼判題服務 (單例)</summary>
    public static CodeJudgeService GetCodeJudgeService()
    {
        _judgeService ??= new CodeJudgeService();
        return _judgeService;
    }

    /// <summary>取得 Ollama 服務 (單例)</summary>
    public static OllamaService GetOllamaService()
    {
        _ollamaService ??= new OllamaService();
        return _ollamaService;
    }

    /// <summary>取得題目匯入服務 (單例)</summary>
    public static ProblemImportService GetImportService()
    {
        _importService ??= new ProblemImportService();
        return _importService;
    }

    /// <summary>建立題目Repository</summary>
    public static ProblemRepository CreateProblemRepository()
    {
        return new ProblemRepository(GetDbContext());
    }

    /// <summary>建立測試案例Repository</summary>
    public static TestCaseRepository CreateTestCaseRepository()
    {
        return new TestCaseRepository(GetDbContext());
    }

    /// <summary>建立用戶提交Repository</summary>
    public static UserSubmissionRepository CreateUserSubmissionRepository()
    {
        return new UserSubmissionRepository(GetDbContext());
    }

    /// <summary>建立題目審核Repository</summary>
    public static ProblemReviewRepository CreateProblemReviewRepository()
    {
        return new ProblemReviewRepository(GetDbContext());
    }

    /// <summary>清理資源 - 應用程式結束時呼叫</summary>
    public static void Dispose()
    {
        _dbContext?.Dispose();
        _settingsService = null;
        _judgeService = null;
        _ollamaService = null;
        _importService = null;
    }
}
