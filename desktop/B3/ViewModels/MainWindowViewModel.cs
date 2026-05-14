using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using B3.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace B3.ViewModels;

/// <summary>
/// 主窗口ViewModel - 協調各頁面的切換和全局狀態
/// 職責: 管理當前顯示的視圖 切換不同功能頁面
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private const double ExpandedWidth = 240;
    private const double CollapsedWidth = 72;

    /// <summary>當前顯示的視圖</summary>
    [ObservableProperty]
    private object currentView = new object();

    /// <summary>主選單清單</summary>
    [ObservableProperty]
    private ObservableCollection<NavItem> mainItems = new();

    /// <summary>工具清單</summary>
    [ObservableProperty]
    private ObservableCollection<NavItem> toolItems = new();

    /// <summary>主選單選取</summary>
    [ObservableProperty]
    private NavItem? selectedMainItem;

    /// <summary>工具選取</summary>
    [ObservableProperty]
    private NavItem? selectedToolItem;

    /// <summary>側欄是否收合</summary>
    [ObservableProperty]
    private bool isSidebarCollapsed = false;

    /// <summary>側欄是否展開</summary>
    [ObservableProperty]
    private bool isSidebarExpanded = true;

    /// <summary>側欄寬度</summary>
    [ObservableProperty]
    private double sidebarWidth = ExpandedWidth;

    /// <summary>滑鼠是否停留側欄</summary>
    [ObservableProperty]
    private bool isSidebarHovering = false;

    /// <summary>是否可切換頁面</summary>
    [ObservableProperty]
    private bool canNavigate = true;

    private ExamViewModel? _trackedExamViewModel;
    private ExamResultViewModel? _trackedExamResultViewModel;

    public MainWindowViewModel()
    {
        LoggerService.LogDebug("MainWindowViewModel 初始化...");
        try
        {
            InitializeNavigation();
            NavigateTo("home");
            SelectedMainItem = MainItems.FirstOrDefault(item => item.Key == "home");
            LoggerService.LogDebug("MainWindowViewModel 初始化完成");
        }
        catch (Exception ex)
        {
            LoggerService.LogError("MainWindowViewModel 初始化失敗", ex);
        }
    }

    /// <summary>切換側欄狀態</summary>
    [RelayCommand]
    public void ToggleSidebar()
    {
        
        IsSidebarCollapsed = !IsSidebarCollapsed;
        
    }

    

    /// <summary>視窗寬度改變時調整側欄</summary>
    public void UpdateResponsiveState(double width)
    {
        if (width < 980)
        {
            IsSidebarCollapsed = true;
        }
        else if (width >= 980)
        {
            IsSidebarCollapsed = false;
        }
    }

    /// <summary>初始化導航項目</summary>
    private void InitializeNavigation()
    {
        MainItems = new ObservableCollection<NavItem>
        {
            new("home", "開始畫面"),
            new("question", "題目"),
            new("bank", "已載入題庫"),
            new("import", "匯入")
        };

        ToolItems = new ObservableCollection<NavItem>
        {
            
            new("style", "樣式")
        };
    }

    /// <summary>主選單變更時導向對應頁面</summary>
    partial void OnSelectedMainItemChanged(NavItem? value)
    {
        if (value == null)
        {
            return;
        }

        if (!CanNavigate)
        {
            SelectedMainItem = null;
            return;
        }

        SelectedToolItem = null;
        NavigateTo(value.Key);
    }

    /// <summary>工具選單變更時導向對應頁面</summary>
    partial void OnSelectedToolItemChanged(NavItem? value)
    {
        if (value == null)
        {
            return;
        }

        if (!CanNavigate)
        {
            SelectedToolItem = null;
            return;
        }

        SelectedMainItem = null;
        NavigateTo(value.Key);
    }

    /// <summary>目前頁面變更時同步導航鎖定狀態</summary>
    partial void OnCurrentViewChanged(object value)
    {
        TrackExamViewModel(CurrentView as ExamViewModel);
        TrackExamResultViewModel(CurrentView as ExamResultViewModel);

        if (CurrentView is ExamResultViewModel resultVM)
        {
            resultVM.RequestHomeNavigation = () => NavigateTo("home");
        }

        UpdateNavigationState();
    }

    /// <summary>更新側欄顯示</summary>
    partial void OnIsSidebarCollapsedChanged(bool value)
    {
        UpdateSidebarState();
    }

    /// <summary>更新側欄滑鼠狀態</summary>
    partial void OnIsSidebarHoveringChanged(bool value)
    {
        UpdateSidebarState();
    }

    /// <summary>更新側欄狀態</summary>
    private void UpdateSidebarState()
    {
        IsSidebarExpanded = !IsSidebarCollapsed || IsSidebarHovering;
        SidebarWidth = IsSidebarExpanded ? ExpandedWidth : CollapsedWidth;
    }

    /// <summary>設定側欄滑鼠狀態</summary>
    public void SetSidebarHover(bool isHovering)
    {
        IsSidebarHovering = isHovering;
    }

    /// <summary>切換顯示視圖</summary>
    private void NavigateTo(string key)
    {
        if (!CanNavigate)
        {
            return;
        }

        object newView = key switch
        {
            "home" => new HomeViewModel(),
            "question" => new QuestionHubViewModel(),
            "bank" => new BankViewModel(),
            "import" => new ImportViewModel(),
            
            "style" => new StyleViewModel(),
            "settings" => new SettingsViewModel(),
            _ => new HomeViewModel()
        };

        // 如果是 HomeViewModel，設定其父 ViewModel 引用以支援導航
        if (newView is HomeViewModel homeVM)
        {
            homeVM.SetMainViewModel(this);
        }

        CurrentView = newView;
    }

    /// <summary>從考試卡片啟動考試 - 由 HomeView 呼叫</summary>
    public async System.Threading.Tasks.Task StartExamFromCardAsync(ExamCard card)
    {
        var examVM = new ExamViewModel();
        examVM.RequestHomeNavigation = () => NavigateTo("home");
        examVM.RequestResultNavigation = resultViewModel =>
        {
            if (resultViewModel != null)
            {
                resultViewModel.RequestHomeNavigation = () => NavigateTo("home");
                CurrentView = resultViewModel;
                SelectedMainItem = null;
                SelectedToolItem = null;
            }
        };
        CurrentView = examVM;
        SelectedMainItem = null;
        SelectedToolItem = null;

        await examVM.StartExamFromCardAsync(card);
    }

    /// <summary>顯示題目瀏覽列表</summary>
    [RelayCommand]
    public void ShowProblemList()
    {
        CurrentView = new ProblemListViewModel();
        LoggerService.LogDebug("切換至題目列表視圖");
    }

    /// <summary>顯示考試介面</summary>
    [RelayCommand]
    public void ShowExam()
    {
        CurrentView = new ExamViewModel();
        LoggerService.LogDebug("切換至考試視圖");
    }

    /// <summary>顯示題目審核介面</summary>
    [RelayCommand]
    public void ShowReview()
    {
        CurrentView = new ReviewViewModel();
        LoggerService.LogDebug("切換至審核視圖");
    }

    private void TrackExamResultViewModel(ExamResultViewModel? viewModel)
    {
        if (_trackedExamResultViewModel != null)
        {
            _trackedExamResultViewModel.PropertyChanged -= OnTrackedExamResultViewModelPropertyChanged;
        }

        _trackedExamResultViewModel = viewModel;

        if (_trackedExamResultViewModel != null)
        {
            _trackedExamResultViewModel.PropertyChanged += OnTrackedExamResultViewModelPropertyChanged;
        }
    }

    private void OnTrackedExamResultViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
    }

    /// <summary>顯示系統設定</summary>
    [RelayCommand]
    public void ShowSettings()
    {
        NavigateTo("settings");
        LoggerService.LogDebug("切換至設定視圖");
    }

    /// <summary>側欄設定按鈕</summary>
    [RelayCommand]
    public void OpenSettings()
    {
        if (!CanNavigate)
        {
            return;
        }

        SelectedMainItem = null;
        SelectedToolItem = null;
        NavigateTo("settings");
    }

    /// <summary>追蹤考試頁面狀態變化</summary>
    private void TrackExamViewModel(ExamViewModel? examVM)
    {
        if (_trackedExamViewModel != null)
        {
            _trackedExamViewModel.PropertyChanged -= OnTrackedExamViewModelPropertyChanged;
        }

        _trackedExamViewModel = examVM;

        if (_trackedExamViewModel != null)
        {
            _trackedExamViewModel.PropertyChanged += OnTrackedExamViewModelPropertyChanged;
        }
    }

    /// <summary>考試狀態改變時更新導航鎖定</summary>
    private void OnTrackedExamViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ExamViewModel.IsExamActive) ||
            e.PropertyName == nameof(ExamViewModel.IsPreparingExam))
        {
            UpdateNavigationState();
        }
    }

    /// <summary>更新是否可導航</summary>
    private void UpdateNavigationState()
    {
        if (CurrentView is ExamViewModel examVM)
        {
            CanNavigate = !examVM.IsExamActive && !examVM.IsPreparingExam;
            return;
        }

        CanNavigate = true;
    }
}

/// <summary>
/// 導航項目
/// </summary>
public class NavItem
{
    public NavItem(string key, string title)
    {
        Key = key;
        Title = title;
        ShortTitle = string.IsNullOrWhiteSpace(title) ? string.Empty : title.Substring(0, 1);
    }

    public string Key { get; }
    public string Title { get; }
    public string ShortTitle { get; }
}


