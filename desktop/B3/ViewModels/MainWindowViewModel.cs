using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using B3.Localization;
using B3.Services;
using B3.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
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

    /// <summary>滑動 + 淡化 組合頁面過渡</summary>
    private static readonly IPageTransition SlideFadeTransition = CreateSlideFadeTransition();

    /// <summary>當前顯示的視圖</summary>
    [ObservableProperty]
    private object currentView = new object();

    /// <summary>
    /// 近乎即時的過渡 (設定頁使用)。
    /// 不可用 null：TransitioningContentControl 的兩個容器會輪流使用，上一次淡出會把容器 Opacity 留在 0，
    /// 沒有過渡時不會還原，導致設定頁整頁空白；極短的淡入可確保 Opacity 回到 1。
    /// </summary>
    private static readonly IPageTransition InstantTransition = new CrossFade(TimeSpan.FromMilliseconds(1));

    /// <summary>目前頁面過渡效果 - 設定頁使用 InstantTransition，其他頁面為滑動 + 淡化</summary>
    [ObservableProperty]
    private IPageTransition? pageTransition = SlideFadeTransition;

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

    private bool _isNarrowWindow;
    private ExamViewModel? _trackedExamViewModel;
    private ExamResultViewModel? _trackedExamResultViewModel;

    public MainWindowViewModel()
    {
        LoggerService.LogDebug("MainWindowViewModel 初始化...");
        try
        {
            InitializeNavigation();
            LocalizationService.Instance.LanguageChanged += OnLanguageChanged;
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
        // 只在跨越寬度門檻時自動收合 / 展開，避免每次調整視窗大小都覆蓋使用者手動切換的狀態
        var isNarrow = width < 980;
        if (isNarrow == _isNarrowWindow)
        {
            return;
        }

        _isNarrowWindow = isNarrow;
        IsSidebarCollapsed = isNarrow;
    }

    /// <summary>初始化導航項目</summary>
    private void InitializeNavigation()
    {
        MainItems = new ObservableCollection<NavItem>
        {
            new("home", LocalizationService.T("NavHome")),
            new("question", LocalizationService.T("NavQuestion")),
            new("bank", LocalizationService.T("NavBank")),
            new("import", LocalizationService.T("NavImport"))
        };

        ToolItems = new ObservableCollection<NavItem>
        {

            new("style", LocalizationService.T("NavStyle"))
        };
    }

    /// <summary>導航項目的字串表鍵值</summary>
    private static string NavTitleKey(string key) => key switch
    {
        "home" => "NavHome",
        "question" => "NavQuestion",
        "bank" => "NavBank",
        "import" => "NavImport",
        "style" => "NavStyle",
        _ => key
    };

    /// <summary>語言切換時就地更新導航文字 - 不重建集合以保留選取狀態</summary>
    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        foreach (var item in MainItems)
        {
            item.Title = LocalizationService.T(NavTitleKey(item.Key));
        }

        foreach (var item in ToolItems)
        {
            item.Title = LocalizationService.T(NavTitleKey(item.Key));
        }
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

    /// <summary>建立滑動 + 淡化的組合過渡</summary>
    private static IPageTransition CreateSlideFadeTransition()
    {
        var composite = new CompositePageTransition();
        composite.PageTransitions.Add(new CrossFade(TimeSpan.FromMilliseconds(250)));
        composite.PageTransitions.Add(new PageSlide(TimeSpan.FromMilliseconds(250), PageSlide.SlideAxis.Horizontal)
        {
            SlideInEasing = new CubicEaseOut(),
            SlideOutEasing = new CubicEaseOut()
        });
        return composite;
    }

    /// <summary>切換顯示視圖</summary>
    private void NavigateTo(string key)
    {
        if (!CanNavigate)
        {
            return;
        }

        // 設定頁幾乎直接顯示，其他頁面使用滑動 + 淡化
        PageTransition = key == "settings" ? InstantTransition : SlideFadeTransition;

            object newView = key switch
            {
                "home" => new HomeViewModel(),
                // 將 question 導向題目列表 ViewModel
                "question" => new ProblemListViewModel(),
                "bank" => new BankViewModel(),
                "import" => new ImportViewModel(),
                
            "style" => new StyleViewModel(),
                "settings" => new SettingsViewModel(),
            _ => new HomeViewModel()
        };

        // 如果是需要父引用的 ViewModel，設定其父 ViewModel 引用以支援導航
        if (newView is HomeViewModel homeVM)
        {
            homeVM.SetMainViewModel(this);
        }

        if (newView is ProblemListViewModel problemListVM)
        {
            problemListVM.SetMainViewModel(this);
        }

        if (newView is BankViewModel bankVM)
        {
            bankVM.SetMainViewModel(this);
        }

        CurrentView = newView;
    }

    /// <summary>從考試卡片啟動考試 - 由 HomeView 呼叫</summary>
    public async System.Threading.Tasks.Task StartExamFromCardAsync(ExamCard card)
    {
        PageTransition = SlideFadeTransition;
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
        PageTransition = SlideFadeTransition;
        var vm = new ProblemListViewModel();
        vm.SetMainViewModel(this);
        CurrentView = vm;
        LoggerService.LogDebug("切換至題目列表視圖");
    }

    /// <summary>顯示匯入頁面</summary>
    public void ShowImport(Problem? problem = null)
    {
        PageTransition = SlideFadeTransition;
        var vm = new ImportViewModel();
        if (problem != null)
        {
            vm.LoadProblemForEdit(problem);
        }

        CurrentView = vm;
        SelectedMainItem = MainItems.FirstOrDefault(item => item.Key == "import");
        SelectedToolItem = null;
        LoggerService.LogDebug("切換至匯入視圖");
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
/// 導航項目 - Title 可於語言切換時就地更新
/// </summary>
public partial class NavItem : ObservableObject
{
    public NavItem(string key, string title)
    {
        Key = key;
        Title = title;
    }

    public string Key { get; }

    /// <summary>導航圖示 (16x16 向量路徑) - 新增頁面時在 IconData 補上對應鍵值</summary>
    public Avalonia.Media.Geometry Icon => Avalonia.Media.Geometry.Parse(IconData(Key));

    private static string IconData(string key) => key switch
    {
        "home" => "M8 1.5 1 8h2v6.5h4V10h2v4.5h4V8h2z",
        "question" => "M2 3h12v2H2zM2 7h12v2H2zM2 11h8v2H2z",
        "bank" => "M8 1 1 4.5 8 8l7-3.5zM1 8l7 3.5L15 8v2.2l-7 3.5-7-3.5z",
        "import" => "M7 1h2v7h3l-4 4-4-4h3zM2 13h12v2H2z",
        "style" => "M1 4h8v1.5H1zM12 4h3v1.5h-3zM9.5 2.5h2v4.5h-2zM1 10.5h3v1.5H1zM7 10.5h8v1.5H7zM4.5 9h2v4.5h-2z",
        _ => "M3 3h10v10H3z"
    };

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string shortTitle = string.Empty;

    partial void OnTitleChanged(string value)
    {
        ShortTitle = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Substring(0, 1);
    }
}


