using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using B3.Localization;
using B3.Models;
using B3.Services;
using System.Collections.ObjectModel;
using System.Linq;

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

    /// <summary>一般設定顯示</summary>
    [ObservableProperty]
    private bool isGeneralSection;

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

    /// <summary>介面語言選項</summary>
    [ObservableProperty]
    private ObservableCollection<LanguageOption> languageOptions = new();

    /// <summary>選取的介面語言</summary>
    [ObservableProperty]
    private LanguageOption? selectedUiLanguage;

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

        LanguageOptions = new ObservableCollection<LanguageOption>
        {
            new(LocalizationService.ZhTw, "繁體中文"),
            new(LocalizationService.EnUs, "English")
        };

        Sections = new ObservableCollection<SettingsSection>
        {
            new("general", LocalizationService.T("SectionGeneral")),
            new("exam", LocalizationService.T("SectionExam")),
            new("key", LocalizationService.T("SectionKey")),
            new("data", LocalizationService.T("SectionData")),
            new("ai", LocalizationService.T("SectionAi")),
            new("about", LocalizationService.T("SectionAbout"))
        };
        SelectedSection = Sections[0];
        UpdateSectionFlags();

        _suppressAutoSave = true;
        LoadSettings();
        _suppressAutoSave = false;

        BuildLocalizedLists();
    }

    /// <summary>設定頁籤的字串表鍵值</summary>
    private static string SectionTitleKey(string key) => key switch
    {
        "general" => "SectionGeneral",
        "exam" => "SectionExam",
        "key" => "SectionKey",
        "data" => "SectionData",
        "ai" => "SectionAi",
        "about" => "SectionAbout",
        _ => key
    };

    /// <summary>語言切換時就地更新頁籤標題 - 不重建集合以保留 ListBox 選取狀態</summary>
    private void RefreshSectionTitles()
    {
        foreach (var section in Sections)
        {
            section.Title = LocalizationService.T(SectionTitleKey(section.Key));
        }
    }

    /// <summary>依目前語言重建快捷鍵與資料管理清單</summary>
    private void BuildLocalizedLists()
    {
        KeyBindings = new ObservableCollection<KeyBindingItem>
        {
            new(LocalizationService.T("KbRunCode"), "Ctrl + Enter"),
            new(LocalizationService.T("KbNext"), "→ 或 N"),
            new(LocalizationService.T("KbPrev"), "← 或 P"),
            new(LocalizationService.T("KbMark"), "M"),
            new(LocalizationService.T("KbToggleAnswer"), "Space"),
            new(LocalizationService.T("KbHome"), "Ctrl + H"),
            new(LocalizationService.T("KbOpenSettings"), "Ctrl + ,")
        };

        DataActions = new ObservableCollection<DataActionItem>
        {
            new(LocalizationService.T("DaExportTitle"), LocalizationService.T("DaExportDesc"), LocalizationService.T("DaExport"), false),
            new(LocalizationService.T("DaBackupTitle"), LocalizationService.T("DaBackupDesc"), LocalizationService.T("DaBackup"), false),
            new(LocalizationService.T("DaClearTitle"), LocalizationService.T("DaClearDesc"), LocalizationService.T("DaClear"), false),
            new(LocalizationService.T("DaResetTitle"), LocalizationService.T("DaResetDesc"), LocalizationService.T("DaReset"), true)
        };
    }

    /// <summary>切換區塊顯示狀態</summary>
    partial void OnSelectedSectionChanged(SettingsSection? value)
    {
        // ListBox 更新過程可能暫時推入 null，保持目前顯示避免區塊全部消失
        if (value == null)
        {
            return;
        }

        UpdateSectionFlags();
    }

    /// <summary>介面語言變更 - 立即套用、儲存並更新顯示文字</summary>
    partial void OnSelectedUiLanguageChanged(LanguageOption? value)
    {
        if (value == null)
        {
            return;
        }

        LocalizationService.Instance.SetLanguage(value.Code);

        RefreshSectionTitles();
        BuildLocalizedLists();

        PersistSettingsIfAllowed();
    }

    /// <summary>更新區塊顯示旗標</summary>
    private void UpdateSectionFlags()
    {
        var key = SelectedSection?.Key ?? string.Empty;
        IsGeneralSection = key == "general";
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
        ApplyToViewModel(settings);
    }

    /// <summary>將設定值套用到 ViewModel 屬性</summary>
    private void ApplyToViewModel(AppSettings settings)
    {
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
        SelectedUiLanguage = LanguageOptions.FirstOrDefault(o => o.Code == settings.UiLanguage) ?? LanguageOptions[0];
    }

    /// <summary>儲存本機設定</summary>
    [RelayCommand]
    public void SaveSettings()
    {
        // 以現有設定為基底，避免覆寫樣式頁維護的欄位 (主題、字體等)
        var settings = _settingsService.Load();
        settings.OllamaEndpoint = OllamaEndpoint;
        settings.OllamaModel = OllamaModel;
        settings.UseLocalTransformers = UseLocalTransformers;
        settings.LocalTransformersModelPath = LocalTransformersModelPath;
        settings.PythonPath = PythonPath;
        settings.CppCompilerPath = CppCompilerPath;
        settings.DotNetPath = DotNetPath;
        settings.DefaultLanguage = DefaultLanguage;
        settings.QuestionsPerExam = QuestionsPerExam;
        settings.ShowAnswerOnSubmit = ShowAnswerOnSubmit;
        settings.EnableCountdown = EnableCountdown;
        settings.ShuffleQuestions = ShuffleQuestions;
        settings.Difficulty = Difficulty;
        settings.UiLanguage = SelectedUiLanguage?.Code ?? LocalizationService.ZhTw;
        _settingsService.Save(settings);
    }

    /// <summary>恢復預設設定</summary>
    [RelayCommand]
    public void ResetSettings()
    {
        _suppressAutoSave = true;
        var settings = new AppSettings();
        ApplyToViewModel(settings);
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
/// 設定頁籤 - Title 可於語言切換時就地更新
/// </summary>
public partial class SettingsSection : ObservableObject
{
    public SettingsSection(string key, string title)
    {
        Key = key;
        this.title = title;
    }

    public string Key { get; }

    [ObservableProperty]
    private string title;
}

/// <summary>
/// 介面語言選項
/// </summary>
public class LanguageOption
{
    public LanguageOption(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    public string Code { get; }
    public string DisplayName { get; }
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
