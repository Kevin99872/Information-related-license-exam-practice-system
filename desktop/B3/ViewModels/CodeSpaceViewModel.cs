using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using B3.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace B3.ViewModels;

/// <summary>
/// Code Space ViewModel - 程式練習工作區
/// </summary>
public partial class CodeSpaceViewModel : ViewModelBase
{
    private readonly CodeJudgeService _judgeService = new();
    private readonly LocalSettingsService _settingsService = new();

    /// <summary>專案清單</summary>
    [ObservableProperty]
    private ObservableCollection<string> projects = new();

    /// <summary>題庫清單</summary>
    [ObservableProperty]
    private ObservableCollection<string> repositories = new();

    /// <summary>語言清單</summary>
    [ObservableProperty]
    private ObservableCollection<string> languages = new();

    /// <summary>選取語言</summary>
    [ObservableProperty]
    private string selectedLanguage = "Python";

    /// <summary>程式碼</summary>
    [ObservableProperty]
    private string code = string.Empty;

    /// <summary>輸入</summary>
    [ObservableProperty]
    private string input = string.Empty;

    /// <summary>輸出</summary>
    [ObservableProperty]
    private string output = string.Empty;

    /// <summary>是否執行中</summary>
    [ObservableProperty]
    private bool isRunning = false;

    /// <summary>是否可執行</summary>
    [ObservableProperty]
    private bool isNotRunning = true;

    public CodeSpaceViewModel()
    {
        Projects = new ObservableCollection<string>
        {
            "cpe_practice"
        };

        Repositories = new ObservableCollection<string>
        {
            "CPE 2024",
            "q1_sort.cpp",
            "q2_tree.cpp",
            "q3_dp.cpp"
        };

        Languages = new ObservableCollection<string>
        {
            "Python",
            "C#",
            "C++"
        };

        var settings = _settingsService.Load();
        SelectedLanguage = settings.DefaultLanguage;
        Code = "print('Hello World')";
    }

    /// <summary>執行程式碼</summary>
    [RelayCommand]
    public async Task RunCodeAsync()
    {
        if (IsRunning)
        {
            return;
        }

        IsRunning = true;
        try
        {
            Output = await _judgeService.ExecuteAsync(SelectedLanguage, Code, Input);
        }
        finally
        {
            IsRunning = false;
        }
    }

    /// <summary>更新執行狀態</summary>
    partial void OnIsRunningChanged(bool value)
    {
        IsNotRunning = !value;
    }
}
