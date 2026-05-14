using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using B3.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace B3.ViewModels;

/// <summary>
/// 開始畫面ViewModel - 提供熱門考照卡片與進度- 考試種類整理
/// </summary>
public partial class HomeViewModel : ViewModelBase
{
    /// <summary>父視窗 ViewModel</summary>
    private MainWindowViewModel? _mainVM;

    /// <summary>熱門考照卡片</summary>
    [ObservableProperty]
    private ObservableCollection<ExamCard> hotCards = new();

    /// <summary>其他考照卡片</summary>
    [ObservableProperty]
    private ObservableCollection<ExamCard> extraCards = new();

    public HomeViewModel()
    {
        HotCards = new ObservableCollection<ExamCard>
        {
            new("TQC+ Python3", "TQC", "熱門", "TQC+ Python 考驗對於Python的基礎程式邏輯與演算法能力。", 90, 180, 0),
            new("CPE", "CPE", "新增", "大學程式設計先修檢測，考驗 C/C++ 程式邏輯與演算法能力。共1-5級及題目。", 4000, 150, 0),
            new("APCS", "APCS", "熱門", "APCS 程式設計先修檢測，涵蓋各級程度的演算法與資料結構題庫。", 2100, 120, 5)
        };

        ExtraCards = new ObservableCollection<ExamCard>
        {
            new("電腦軟體設計 丙級技術士", "Software", "", "技術士技能檢定丙級，涵蓋各類軟體工程師基本演算法題庫。", 980, 100, 0),
        };
    }

    /// <summary>設定父視窗 ViewModel - 用於導航</summary>
    public void SetMainViewModel(MainWindowViewModel mainVM)
    {
        _mainVM = mainVM;
    }

    /// <summary>啟動考試 - 由卡片按鈕呼叫</summary>
    [RelayCommand]
    public async System.Threading.Tasks.Task StartExamAsync(ExamCard card)
    {
        if (card == null)
        {
            LoggerService.LogWarning("未收到考試卡片參數，無法啟動考試");
            return;
        }

        if (_mainVM == null)
        {
            LoggerService.LogWarning("MainWindowViewModel 未設定");
            return;
        }

        try
        {
            LoggerService.LogDebug($"從卡片啟動考試: {card.Title}");
            await _mainVM.StartExamFromCardAsync(card);
        }
        catch (Exception ex)
        {
            LoggerService.LogError("啟動考試失敗", ex);
        }
    }
}

/// <summary>
/// 考照卡片資料 - 代表一個考試類型
/// </summary>
public class ExamCard
{
    public ExamCard(string title, string examType, string tag, string description, int questionCount, int durationMinutes, int progressPercent)
    {
        Title = title;
        ExamType = examType;
        Tag = tag;
        Description = description;
        QuestionCount = questionCount;
        DurationMinutes = durationMinutes;
        ProgressPercent = progressPercent;
    }

    /// <summary>顯示的標題</summary>
    public string Title { get; }
    
    /// <summary>考試類型 (TQC, CPE, APCS 等)</summary>
    public string ExamType { get; }
    
    /// <summary>標籤 (熱門, 新增等)</summary>
    public string Tag { get; }
    
    /// <summary>描述文字</summary>
    public string Description { get; }
    
    /// <summary>題目總數</summary>
    public int QuestionCount { get; }
    
    /// <summary>建議時間(分鐘)</summary>
    public int DurationMinutes { get; }
    
    /// <summary>進度百分比</summary>
    public int ProgressPercent { get; }
}
