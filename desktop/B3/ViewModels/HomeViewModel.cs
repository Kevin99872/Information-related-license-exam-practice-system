using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Threading;
using B3.Data;
using B3.Services;
using B3.Models;
using System;
using System.Collections.Generic;
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
        // 先顯示載入前的後備資料，再非同步從資料庫載入正式卡片
        LoadFallbackCards();
        _ = LoadCardsAsync();
    }

    /// <summary>
    /// 從 catalog.db 讀取考試種類，並查詢 exam.db 取得各題庫的實際題數後組成卡片
    /// </summary>
    private async Task LoadCardsAsync()
    {
        try
        {
            List<ExamCategory> categories;
            using (var catalogContext = new ExamCatalogDbContext())
            {
                var categoryRepo = new ExamCategoryRepository(catalogContext);
                categories = await categoryRepo.GetAllOrderedAsync();
            }

            if (categories.Count == 0)
            {
                return;
            }

            Dictionary<string, int> questionCounts;
            using (var examContext = new ExamDbContext())
            {
                var problemRepo = new ProblemRepository(examContext);
                questionCounts = await problemRepo.GetActiveCountByExamTypeAsync();
            }

            var hot = new ObservableCollection<ExamCard>();
            var extra = new ObservableCollection<ExamCard>();

            foreach (var category in categories)
            {
                var questionCount = questionCounts.TryGetValue(category.ExamType, out var count) ? count : 0;
                var card = new ExamCard(category.Title, category.ExamType, category.Tag,
                    category.Description, questionCount, category.DurationMinutes, 0);

                if (category.IsHot)
                {
                    hot.Add(card);
                }
                else
                {
                    extra.Add(card);
                }
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                HotCards = hot;
                ExtraCards = extra;
            });
        }
        catch (Exception ex)
        {
            LoggerService.LogError("從資料庫載入題庫卡片失敗，沿用後備資料", ex);
        }
    }

    /// <summary>資料庫尚未載入或載入失敗時的後備卡片</summary>
    private void LoadFallbackCards()
    {
        HotCards = new ObservableCollection<ExamCard>
        {
            new("TQC+ Python3", "TQC", "熱門", "TQC+ Python 考驗對於Python的基礎程式邏輯與演算法能力。", 0, 180, 0),
        };

        ExtraCards = new ObservableCollection<ExamCard>
        {     };
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
