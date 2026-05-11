using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;

namespace B3.ViewModels;

/// <summary>
/// 主窗口ViewModel - 協調各頁面的切換和全局狀態
/// 職責: 管理當前顯示的視圖 切換不同功能頁面
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>當前顯示的視圖</summary>
    [ObservableProperty]
    private object currentView = new object();

    public MainWindowViewModel()
    {
        Debug.WriteLine("MainWindowViewModel 初始化...");
        try
        {
            // 初始化時顯示題目列表視圖
            currentView = new ProblemListViewModel();
            Debug.WriteLine("MainWindowViewModel 初始化完成");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MainWindowViewModel 初始化失敗: {ex.Message}");
            Debug.WriteLine($"堆棧跟蹤: {ex.StackTrace}");
        }
    }

    /// <summary>顯示題目瀏覽列表</summary>
    [RelayCommand]
    public void ShowProblemList()
    {
        CurrentView = new ProblemListViewModel();
        Debug.WriteLine("切換至題目列表視圖");
    }

    /// <summary>顯示考試介面</summary>
    [RelayCommand]
    public void ShowExam()
    {
        CurrentView = new ExamViewModel();
        Debug.WriteLine("切換至考試視圖");
    }

    /// <summary>顯示題目審核介面</summary>
    [RelayCommand]
    public void ShowReview()
    {
        CurrentView = new ReviewViewModel();
        Debug.WriteLine("切換至審核視圖");
    }

    /// <summary>顯示系統設定</summary>
    [RelayCommand]
    public void ShowSettings()
    {
        // TODO: 實現設定視圖
        Debug.WriteLine("設定視圖待實現");
    }
}


