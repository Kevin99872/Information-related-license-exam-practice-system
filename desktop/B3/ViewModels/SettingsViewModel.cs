using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using B3.Models;
using B3.Services;
using System.Collections.ObjectModel;

namespace B3.ViewModels;

/// <summary>
/// 系統設定ViewModel - 設定頁籤與配置
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly LocalSettingsService _settingsService = new();
    private bool _suppressAutoSave;

    /// <summary>設定頁籤</summary>
    [ObservableProperty]
    private ObservableCollection<SettingsSection> sections = new();

    /// <summary>選取頁籤</summary>
    [ObservableProperty]
    private SettingsSection? selectedSection;

    /// <summary>考試設定顯示</summary>
    [ObservableProperty]
    private bool isExamSection;


    /// <summary>快捷鍵顯示</summary>
    [ObservableProperty]
    private bool isKeySection;

    /// <summary>資料管理顯示</summary>
    [ObservableProperty]
    private bool isDataSection;

    /// <summary>關於顯示</summary>
    [ObservableProperty]
    private bool isAboutSection;

    /// <summary>AI 設定顯示</summary>
    [ObservableProperty]
    private bool isAiSection;

    /// <summary>作答後顯示答案</summary>
    [ObservableProperty]
    private bool showAnswerOnSubmit = true;


    /// <summary>倒數計時器</summary>
    [ObservableProperty]
    private bool enableCountdown = true;

    /// <summary>題目隨機排序</summary>
    [ObservableProperty]
    private bool shuffleQuestions = true;

    /// <summary>每次練習題數</summary>
    [ObservableProperty]
    private int questionsPerExam = 20;

    /// <summary>每次練習題數選項</summary>
    [ObservableProperty]
    private ObservableCollection<int> questionsPerExamOptions = new();

    /// <summary>難度篩選</summary>
    [ObservableProperty]
    private string difficulty = "中等";

    /// <summary>快捷鍵清單</summary>
    [ObservableProperty]
    private ObservableCollection<KeyBindingItem> keyBindings = new();

    /// <summary>資料管理操作</summary>
    [ObservableProperty]
    private ObservableCollection<DataActionItem> dataActions = new();

    /// <summary>版本</summary>
    [ObservableProperty]
    private string appVersion = "v1.0.0";

    /// <summary>資料庫最後更新</summary>
    [ObservableProperty]
    private string lastSyncDate = "2026-05-10";

    /// <summary>Ollama 端點</summary>
    [ObservableProperty]
    private string ollamaEndpoint = string.Empty;

    /// <summary>Ollama 模型</summary>
    [ObservableProperty]
    private string ollamaModel = string.Empty;

    /// <summary>是否使用本地 Transformers 模型</summary>
    [ObservableProperty]
    private bool useLocalTransformers = false;

    /// <summary>本地 Transformers 模型路徑或名稱</summary>
    [ObservableProperty]
    private string localTransformersModelPath = string.Empty;

    /// <summary>Python 路徑</summary>
    [ObservableProperty]
    private string pythonPath = string.Empty;

    /// <summary>C++ 編譯器路徑</summary>
    [ObservableProperty]
    private string cppCompilerPath = string.Empty;

    /// <summary>DotNet 路徑</summary>
    [ObservableProperty]
    private string dotNetPath = string.Empty;

    /// <summary>預設語言</summary>
    [ObservableProperty]
    private string defaultLanguage = "Python";

    public SettingsViewModel()
    {
        QuestionsPerExamOptions = new ObservableCollection<int>
        {
            10,
            20,
            30,
            40
        };

        Sections = new ObservableCollection<SettingsSection>
        {
            new("exam", "考試設定"),
            new("key", "快捷鍵"),
            new("data", "資料管理"),
            new("ai", "AI 模型"),
            new("about", "關於")
        };
        SelectedSection = Sections[0];
        UpdateSectionFlags();

        _suppressAutoSave = true;
        LoadSettings();
        _suppressAutoSave = false;

        KeyBindings = new ObservableCollection<KeyBindingItem>
        {
            new("執行程式碼", "Ctrl + Enter"),
            new("下一題", "→ 或 N"),
            new("上一題", "← 或 P"),
            new("標記題目", "M"),
            new("顯示/隱藏答案", "Space"),
            new("回到首頁", "Ctrl + H"),
            new("開啟設定", "Ctrl + ,")
        };

        DataActions = new ObservableCollection<DataActionItem>
        {
            new("匯出練習紀錄", "將歷史答題資料匯出為 CSV", "匯出", false),
            new("備份題庫設定", "備存目前所有題庫設定", "備份", false),
            new("清除快取", "清除暫存資料 (不影響題庫)", "清除", false),
            new("重置所有練習進度", "此操作無法復原，請謹慎使用", "重置", true)
        };
    }

    /// <summary>切換區塊顯示狀態</summary>
    partial void OnSelectedSectionChanged(SettingsSection? value)
    {
        UpdateSectionFlags();
    }

    /// <summary>更新區塊顯示旗標</summary>
    private void UpdateSectionFlags()
    {
        var key = SelectedSection?.Key ?? string.Empty;
        IsExamSection = key == "exam";
        IsKeySection = key == "key";
        IsDataSection = key == "data";
        IsAiSection = key == "ai";
        IsAboutSection = key == "about";
    }

    /// <summary>讀取本機設定</summary>
    private void LoadSettings()
    {
        var settings = _settingsService.Load();
        OllamaEndpoint = settings.OllamaEndpoint;
        OllamaModel = settings.OllamaModel;
        UseLocalTransformers = settings.UseLocalTransformers;
        LocalTransformersModelPath = settings.LocalTransformersModelPath;
        PythonPath = settings.PythonPath;
        CppCompilerPath = settings.CppCompilerPath;
        DotNetPath = settings.DotNetPath;
        DefaultLanguage = settings.DefaultLanguage;
        QuestionsPerExam = settings.QuestionsPerExam;
        ShowAnswerOnSubmit = settings.ShowAnswerOnSubmit;
        EnableCountdown = settings.EnableCountdown;
        ShuffleQuestions = settings.ShuffleQuestions;
        Difficulty = settings.Difficulty;
    }

    /// <summary>儲存本機設定</summary>
    [RelayCommand]
    public void SaveSettings()
    {
        var settings = new AppSettings
        {
            OllamaEndpoint = OllamaEndpoint,
            OllamaModel = OllamaModel,
            UseLocalTransformers = UseLocalTransformers,
            LocalTransformersModelPath = LocalTransformersModelPath,
            PythonPath = PythonPath,
            CppCompilerPath = CppCompilerPath,
            DotNetPath = DotNetPath,
            DefaultLanguage = DefaultLanguage,
            QuestionsPerExam = QuestionsPerExam,
            ShowAnswerOnSubmit = ShowAnswerOnSubmit,
            EnableCountdown = EnableCountdown,
            ShuffleQuestions = ShuffleQuestions,
            Difficulty = Difficulty
        };
        _settingsService.Save(settings);
    }

    /// <summary>恢復預設設定</summary>
    [RelayCommand]
    public void ResetSettings()
    {
        _suppressAutoSave = true;
        var settings = new AppSettings();
        OllamaEndpoint = settings.OllamaEndpoint;
        OllamaModel = settings.OllamaModel;
        UseLocalTransformers = settings.UseLocalTransformers;
        LocalTransformersModelPath = settings.LocalTransformersModelPath;
        PythonPath = settings.PythonPath;
        CppCompilerPath = settings.CppCompilerPath;
        DotNetPath = settings.DotNetPath;
        DefaultLanguage = settings.DefaultLanguage;
        QuestionsPerExam = settings.QuestionsPerExam;
        ShowAnswerOnSubmit = settings.ShowAnswerOnSubmit;
        EnableCountdown = settings.EnableCountdown;
        ShuffleQuestions = settings.ShuffleQuestions;
        Difficulty = settings.Difficulty;
        _settingsService.Save(settings);
        _suppressAutoSave = false;
    }

    private void PersistSettingsIfAllowed()
    {
        if (_suppressAutoSave)
        {
            return;
        }

        SaveSettings();
    }

    partial void OnQuestionsPerExamChanged(int value) => PersistSettingsIfAllowed();

    partial void OnShowAnswerOnSubmitChanged(bool value) => PersistSettingsIfAllowed();

    partial void OnEnableCountdownChanged(bool value) => PersistSettingsIfAllowed();

    partial void OnShuffleQuestionsChanged(bool value) => PersistSettingsIfAllowed();

    partial void OnDifficultyChanged(string value) => PersistSettingsIfAllowed();
}

/// <summary>
/// 設定頁籤
/// </summary>
public class SettingsSection
{
    public SettingsSection(string key, string title)
    {
        Key = key;
        Title = title;
    }

    public string Key { get; }
    public string Title { get; }
}

/// <summary>
/// 快捷鍵項目
/// </summary>
public class KeyBindingItem
{
    public KeyBindingItem(string action, string shortcut)
    {
        Action = action;
        Shortcut = shortcut;
    }

    public string Action { get; }
    public string Shortcut { get; }
}

/// <summary>
/// 資料管理操作
/// </summary>
public class DataActionItem
{
    public DataActionItem(string title, string description, string actionLabel, bool isDanger)
    {
        Title = title;
        Description = description;
        ActionLabel = actionLabel;
        IsDanger = isDanger;
    }

    public string Title { get; }
    public string Description { get; }
    public string ActionLabel { get; }
    public bool IsDanger { get; }
}
