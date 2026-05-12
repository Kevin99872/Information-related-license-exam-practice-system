using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace B3.ViewModels;

/// <summary>
/// 開始畫面ViewModel - 提供熱門考照卡片與進度
/// </summary>
public partial class HomeViewModel : ViewModelBase
{
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
            new("TACT", "熱門", "台灣學術能力測驗，涵蓋語文、數學、自然及社會科目模擬練習。", 1240, 180, 34),
            new("CPE", "新增", "大學程式設計先修檢測，考驗 C/C++ 程式邏輯與演算法能力。", 860, 150, 12),
            new("GEPT", "熱門", "全民英檢能力分級檢定，提供初、中、中高、高、優級完整題庫。", 2100, 120, 5)
        };

        ExtraCards = new ObservableCollection<ExamCard>
        {
            new("乙級技士", "", "技術士技能檢定乙級，涵蓋各類職業技能挑戰題庫。", 980, 100, 0),
            new("公務人員", "", "初等、普考、高考等類科題庫，含行政法、國文、英文等科目。", 3500, 200, 0)
        };
    }
}

/// <summary>
/// 考照卡片資料
/// </summary>
public class ExamCard
{
    public ExamCard(string title, string tag, string description, int questionCount, int durationMinutes, int progressPercent)
    {
        Title = title;
        Tag = tag;
        Description = description;
        QuestionCount = questionCount;
        DurationMinutes = durationMinutes;
        ProgressPercent = progressPercent;
    }

    public string Title { get; }
    public string Tag { get; }
    public string Description { get; }
    public int QuestionCount { get; }
    public int DurationMinutes { get; }
    public int ProgressPercent { get; }
}
