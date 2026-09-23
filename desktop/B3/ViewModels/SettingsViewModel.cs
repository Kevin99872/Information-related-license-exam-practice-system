using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using B3.Localization;
using B3.Models;
using B3.Services;
using Avalonia.Media;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace B3.ViewModels;

/// <summary>
/// 系統設定ViewModel - 設定頁籤與配置
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly LocalSettingsService _settingsService = new();
    private readonly RuntimeInstallerService _runtimeInstaller = new();
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

    /// <summary>程式編輯器設定顯示</summary>
    [ObservableProperty]
    private bool isEditorSection;

    /// <summary>是否啟用 Code Space 語法上色</summary>
    [ObservableProperty]
    private bool enableSyntaxHighlighting = true;

    /// <summary>語法上色配色項目</summary>
    [ObservableProperty]
    private ObservableCollection<SyntaxColorItem> syntaxColorItems = new();

    /// <summary>預覽可選的程式語言</summary>
    public ObservableCollection<string> SyntaxPreviewLanguages { get; } = new() { "Python", "C#", "C++" };

    /// <summary>預覽使用的程式語言</summary>
    [ObservableProperty]
    private string syntaxPreviewLanguage = "C++";

    /// <summary>配色或開關變更時通知 (供設定頁預覽重繪)</summary>
    public event Action? SyntaxAppearanceChanged;

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

    /// <summary>AI 服務供應商選項</summary>
    [ObservableProperty]
    private ObservableCollection<string> aiProviderOptions = new();

    /// <summary>選取的 AI 服務供應商</summary>
    [ObservableProperty]
    private string selectedAiProvider = "Ollama";

    /// <summary>Ollama 供應商欄位顯示</summary>
    [ObservableProperty]
    private bool isOllamaProvider = true;

    /// <summary>OpenAI 供應商欄位顯示</summary>
    [ObservableProperty]
    private bool isOpenAiProvider;

    /// <summary>Claude 供應商欄位顯示</summary>
    [ObservableProperty]
    private bool isClaudeProvider;

    /// <summary>Ollama 端點</summary>
    [ObservableProperty]
    private string ollamaEndpoint = string.Empty;

    /// <summary>Ollama 模型</summary>
    [ObservableProperty]
    private string ollamaModel = string.Empty;

    /// <summary>OpenAI 端點</summary>
    [ObservableProperty]
    private string openAiEndpoint = string.Empty;

    /// <summary>OpenAI API Key</summary>
    [ObservableProperty]
    private string openAiApiKey = string.Empty;

    /// <summary>OpenAI 模型</summary>
    [ObservableProperty]
    private string openAiModel = string.Empty;

    /// <summary>Claude API Key</summary>
    [ObservableProperty]
    private string claudeApiKey = string.Empty;

    /// <summary>Claude 模型</summary>
    [ObservableProperty]
    private string claudeModel = string.Empty;

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

    /// <summary>Java 路徑</summary>
    [ObservableProperty]
    private string javaPath = string.Empty;

    /// <summary>Python 自動配置狀態</summary>
    [ObservableProperty]
    private string pythonSetupStatus = string.Empty;

    /// <summary>C++ 自動配置狀態</summary>
    [ObservableProperty]
    private string cppSetupStatus = string.Empty;

    /// <summary>DotNet 自動配置狀態</summary>
    [ObservableProperty]
    private string dotNetSetupStatus = string.Empty;

    /// <summary>Java 自動配置狀態</summary>
    [ObservableProperty]
    private string javaSetupStatus = string.Empty;

    /// <summary>是否正在自動配置執行環境 (同時只允許一個安裝程序)</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRuntimeSetupIdle))]
    private bool isRuntimeSetupRunning;

    /// <summary>可以開始自動配置</summary>
    public bool IsRuntimeSetupIdle => !IsRuntimeSetupRunning;

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

        AiProviderOptions = new ObservableCollection<string>
        {
            "Ollama",
            "OpenAI",
            "Claude"
        };

        Sections = new ObservableCollection<SettingsSection>
        {
            new("general", LocalizationService.T("SectionGeneral")),
            new("exam", LocalizationService.T("SectionExam")),
            new("editor", LocalizationService.T("SectionEditor")),
            new("key", LocalizationService.T("SectionKey")),
            new("data", LocalizationService.T("SectionData")),
            new("ai", LocalizationService.T("SectionAi")),
            new("about", LocalizationService.T("SectionAbout"))
        };
        SelectedSection = Sections[0];
        UpdateSectionFlags();

        SyntaxColorItems = new ObservableCollection<SyntaxColorItem>
        {
            new("SyntaxPlainText", s => s.PlainText, (s, v) => s.PlainText = v),
            new("SyntaxKeyword", s => s.Keyword, (s, v) => s.Keyword = v),
            new("SyntaxType", s => s.Type, (s, v) => s.Type = v),
            new("SyntaxFunction", s => s.Function, (s, v) => s.Function = v),
            new("SyntaxString", s => s.String, (s, v) => s.String = v),
            new("SyntaxNumber", s => s.Number, (s, v) => s.Number = v),
            new("SyntaxComment", s => s.Comment, (s, v) => s.Comment = v),
            new("SyntaxPreprocessor", s => s.Preprocessor, (s, v) => s.Preprocessor = v)
        };
        foreach (var item in SyntaxColorItems)
        {
            item.ColorChanged += OnSyntaxColorChanged;
        }

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
        "editor" => "SectionEditor",
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

        foreach (var item in SyntaxColorItems)
        {
            item.RefreshTitle();
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
        IsEditorSection = key == "editor";
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

    /// <summary>供應商切換 - 更新對應輸入欄位的顯示</summary>
    partial void OnSelectedAiProviderChanged(string value)
    {
        IsOllamaProvider = value == "Ollama";
        IsOpenAiProvider = value == "OpenAI";
        IsClaudeProvider = value == "Claude";
    }

    /// <summary>將設定值套用到 ViewModel 屬性</summary>
    private void ApplyToViewModel(AppSettings settings)
    {
        SelectedAiProvider = AiProviderOptions.Contains(settings.AiProvider) ? settings.AiProvider : "Ollama";
        OllamaEndpoint = settings.OllamaEndpoint;
        OllamaModel = settings.OllamaModel;
        OpenAiEndpoint = settings.OpenAiEndpoint;
        OpenAiApiKey = settings.OpenAiApiKey;
        OpenAiModel = settings.OpenAiModel;
        ClaudeApiKey = settings.ClaudeApiKey;
        ClaudeModel = settings.ClaudeModel;
        UseLocalTransformers = settings.UseLocalTransformers;
        LocalTransformersModelPath = settings.LocalTransformersModelPath;
        PythonPath = settings.PythonPath;
        CppCompilerPath = settings.CppCompilerPath;
        DotNetPath = settings.DotNetPath;
        JavaPath = settings.JavaPath;
        DefaultLanguage = settings.DefaultLanguage;
        QuestionsPerExam = settings.QuestionsPerExam;
        ShowAnswerOnSubmit = settings.ShowAnswerOnSubmit;
        EnableCountdown = settings.EnableCountdown;
        ShuffleQuestions = settings.ShuffleQuestions;
        Difficulty = settings.Difficulty;
        SelectedUiLanguage = LanguageOptions.FirstOrDefault(o => o.Code == settings.UiLanguage) ?? LanguageOptions[0];
        EnableSyntaxHighlighting = settings.EnableSyntaxHighlighting;
        foreach (var item in SyntaxColorItems)
        {
            item.LoadFrom(settings.SyntaxColors ?? new SyntaxColorSettings());
        }
        SyntaxAppearanceChanged?.Invoke();
    }

    /// <summary>儲存本機設定</summary>
    [RelayCommand]
    public void SaveSettings()
    {
        // 以現有設定為基底，避免覆寫樣式頁維護的欄位 (主題、字體等)
        var settings = _settingsService.Load();
        settings.AiProvider = SelectedAiProvider;
        settings.OllamaEndpoint = OllamaEndpoint;
        settings.OllamaModel = OllamaModel;
        settings.OpenAiEndpoint = OpenAiEndpoint;
        settings.OpenAiApiKey = OpenAiApiKey;
        settings.OpenAiModel = OpenAiModel;
        settings.ClaudeApiKey = ClaudeApiKey;
        settings.ClaudeModel = ClaudeModel;
        settings.UseLocalTransformers = UseLocalTransformers;
        settings.LocalTransformersModelPath = LocalTransformersModelPath;
        settings.PythonPath = PythonPath;
        settings.CppCompilerPath = CppCompilerPath;
        settings.DotNetPath = DotNetPath;
        settings.JavaPath = JavaPath;
        settings.DefaultLanguage = DefaultLanguage;
        settings.QuestionsPerExam = QuestionsPerExam;
        settings.ShowAnswerOnSubmit = ShowAnswerOnSubmit;
        settings.EnableCountdown = EnableCountdown;
        settings.ShuffleQuestions = ShuffleQuestions;
        settings.Difficulty = Difficulty;
        settings.UiLanguage = SelectedUiLanguage?.Code ?? LocalizationService.ZhTw;
        settings.EnableSyntaxHighlighting = EnableSyntaxHighlighting;
        settings.SyntaxColors = BuildSyntaxColorSettings();
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

    /// <summary>自動配置單一執行環境 (參數: Python / Cpp / DotNet / Java)</summary>
    [RelayCommand]
    public async Task AutoConfigureRuntime(string kindName)
    {
        if (IsRuntimeSetupRunning || !Enum.TryParse<RuntimeKind>(kindName, out var kind))
        {
            return;
        }

        IsRuntimeSetupRunning = true;
        try
        {
            await ConfigureRuntimeCoreAsync(kind);
        }
        finally
        {
            IsRuntimeSetupRunning = false;
        }
    }

    /// <summary>依序自動配置全部執行環境</summary>
    [RelayCommand]
    public async Task AutoConfigureAllRuntimes()
    {
        if (IsRuntimeSetupRunning)
        {
            return;
        }

        IsRuntimeSetupRunning = true;
        try
        {
            foreach (var kind in new[] { RuntimeKind.Python, RuntimeKind.DotNet, RuntimeKind.Java, RuntimeKind.Cpp })
            {
                await ConfigureRuntimeCoreAsync(kind);
            }
        }
        finally
        {
            IsRuntimeSetupRunning = false;
        }
    }

    private async Task ConfigureRuntimeCoreAsync(RuntimeKind kind)
    {
        // Progress<T> 會在建立時的 UI 執行緒回呼，可直接更新綁定屬性
        var progress = new Progress<string>(message => SetRuntimeStatus(kind, message));
        try
        {
            var result = await _runtimeInstaller.ConfigureAsync(kind, GetRuntimePath(kind), progress);
            if (result.Success && result.ExecutablePath != null)
            {
                SetRuntimePath(kind, result.ExecutablePath);
                SaveSettings();
            }

            SetRuntimeStatus(kind, result.Message);
        }
        catch (Exception ex)
        {
            LoggerService.LogError($"自動配置 {kind} 失敗", ex);
            SetRuntimeStatus(kind, string.Format(LocalizationService.T("RtErrorFmt"), ex.Message));
        }
    }

    private string GetRuntimePath(RuntimeKind kind) => kind switch
    {
        RuntimeKind.Python => PythonPath,
        RuntimeKind.Cpp => CppCompilerPath,
        RuntimeKind.DotNet => DotNetPath,
        RuntimeKind.Java => JavaPath,
        _ => string.Empty
    };

    private void SetRuntimePath(RuntimeKind kind, string path)
    {
        switch (kind)
        {
            case RuntimeKind.Python: PythonPath = path; break;
            case RuntimeKind.Cpp: CppCompilerPath = path; break;
            case RuntimeKind.DotNet: DotNetPath = path; break;
            case RuntimeKind.Java: JavaPath = path; break;
        }
    }

    private void SetRuntimeStatus(RuntimeKind kind, string message)
    {
        switch (kind)
        {
            case RuntimeKind.Python: PythonSetupStatus = message; break;
            case RuntimeKind.Cpp: CppSetupStatus = message; break;
            case RuntimeKind.DotNet: DotNetSetupStatus = message; break;
            case RuntimeKind.Java: JavaSetupStatus = message; break;
        }
    }

    partial void OnEnableSyntaxHighlightingChanged(bool value)
    {
        SyntaxAppearanceChanged?.Invoke();
        PersistSettingsIfAllowed();
    }

    partial void OnSyntaxPreviewLanguageChanged(string value) => SyntaxAppearanceChanged?.Invoke();

    /// <summary>還原預設配色</summary>
    [RelayCommand]
    public void ResetSyntaxColors()
    {
        var defaults = new SyntaxColorSettings();
        foreach (var item in SyntaxColorItems)
        {
            item.LoadFrom(defaults);
        }

        SyntaxAppearanceChanged?.Invoke();
        PersistSettingsIfAllowed();
    }

    /// <summary>將目前配色項目組成設定物件</summary>
    public SyntaxColorSettings BuildSyntaxColorSettings()
    {
        var colors = new SyntaxColorSettings();
        foreach (var item in SyntaxColorItems)
        {
            item.WriteTo(colors);
        }

        return colors;
    }

    private void OnSyntaxColorChanged()
    {
        SyntaxAppearanceChanged?.Invoke();
        PersistSettingsIfAllowed();
    }
}

/// <summary>
/// 語法上色配色項目 - 對應 SyntaxColorSettings 的單一欄位
/// </summary>
public partial class SyntaxColorItem : ObservableObject
{
    private readonly string _titleKey;
    private readonly Func<SyntaxColorSettings, string> _getter;
    private readonly Action<SyntaxColorSettings, string> _setter;
    private bool _suppressNotify;

    public SyntaxColorItem(string titleKey, Func<SyntaxColorSettings, string> getter, Action<SyntaxColorSettings, string> setter)
    {
        _titleKey = titleKey;
        _getter = getter;
        _setter = setter;
        title = LocalizationService.T(titleKey);
        color = CodeSyntaxColorizer.ParseColor(null, getter(new SyntaxColorSettings()));
    }

    /// <summary>顯示名稱</summary>
    [ObservableProperty]
    private string title;

    /// <summary>目前顏色</summary>
    [ObservableProperty]
    private Color color;

    /// <summary>色碼文字</summary>
    public string Hex => $"#{Color.R:X2}{Color.G:X2}{Color.B:X2}";

    /// <summary>使用者調整顏色時觸發</summary>
    public event Action? ColorChanged;

    public void RefreshTitle() => Title = LocalizationService.T(_titleKey);

    /// <summary>從設定載入顏色 (不觸發 ColorChanged)</summary>
    public void LoadFrom(SyntaxColorSettings settings)
    {
        _suppressNotify = true;
        Color = CodeSyntaxColorizer.ParseColor(_getter(settings), _getter(new SyntaxColorSettings()));
        _suppressNotify = false;
    }

    public void WriteTo(SyntaxColorSettings settings) => _setter(settings, Hex);

    partial void OnColorChanged(Color value)
    {
        OnPropertyChanged(nameof(Hex));
        if (!_suppressNotify)
        {
            ColorChanged?.Invoke();
        }
    }
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
