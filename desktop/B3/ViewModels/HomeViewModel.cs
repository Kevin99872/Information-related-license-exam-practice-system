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

    /// <summary>目前是否完全沒有任何題庫 (用於顯示空狀態畫面)</summary>
    [ObservableProperty]
    private bool hasNoBanks;

    public HomeViewModel()
    {
        _ = LoadCardsAsync();
    }

    /// <summary>
    /// 從 catalog.db 讀取考試種類，並查詢 exam.db 取得各題庫的實際題數後組成卡片。
    /// 題庫卡片完全由資料庫動態組成，不再寫死任何預設題庫；
    /// 一旦沒有任何題庫，HasNoBanks 會變 true 以顯示空狀態畫面。
    /// </summary>
    private async Task LoadCardsAsync()
    {
        var hot = new ObservableCollection<ExamCard>();
        var extra = new ObservableCollection<ExamCard>();

        try
        {
            List<ExamCategory> categories;
            using (var catalogContext = new ExamCatalogDbContext())
            {
                var categoryRepo = new ExamCategoryRepository(catalogContext);
                categories = await categoryRepo.GetAllOrderedAsync();
            }

            if (categories.Count > 0)
            {
                Dictionary<string, int> questionCounts;
                using (var examContext = new ExamDbContext())
                {
                    var problemRepo = new ProblemRepository(examContext);
                    questionCounts = await problemRepo.GetActiveCountByExamTypeAsync();
                }

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
            }
        }
        catch (Exception ex)
        {
            LoggerService.LogError("從資料庫載入題庫卡片失敗", ex);
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            HotCards = hot;
            ExtraCards = extra;
            HasNoBanks = hot.Count == 0 && extra.Count == 0;
        });
    }

    /// <summary>設定父視窗 ViewModel - 用於導航</summary>
    public void SetMainViewModel(MainWindowViewModel mainVM)
    {
        _mainVM = mainVM;
    }

    /// <summary>前往匯入題庫頁面 - 由「匯入題庫」按鈕或空狀態畫面呼叫</summary>
    [RelayCommand]
    public void GoToImport()
    {
        _mainVM?.ShowImport();
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
