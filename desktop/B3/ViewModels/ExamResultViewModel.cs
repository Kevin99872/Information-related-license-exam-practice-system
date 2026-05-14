using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using B3.Models;
using B3.Services;
using System.Collections.ObjectModel;
using System;
using System.Threading.Tasks;

namespace B3.ViewModels;

public partial class ExamResultViewModel : ViewModelBase
{
    private readonly OllamaService _ollamaService = new();

    public Action? RequestHomeNavigation { get; set; }

    [ObservableProperty]
    private string scoreGrade = string.Empty;

    [ObservableProperty]
    private int correctCount;

    [ObservableProperty]
    private int answeredCount;

    [ObservableProperty]
    private int totalProblems;

    [ObservableProperty]
    private string elapsedTimeText = string.Empty;

    [ObservableProperty]
    private int scorePercent;

    [ObservableProperty]
    private string lastSubmittedCode = string.Empty;

    [ObservableProperty]
    private string outputResult = string.Empty;

    [ObservableProperty]
    private string aiQuestion = string.Empty;

    [ObservableProperty]
    private string aiAnswer = string.Empty;

    [ObservableProperty]
    private string aiAnalysis = string.Empty;

    [ObservableProperty]
    private bool isAiBusy = false;

    [ObservableProperty]
    private ObservableCollection<Problem> examProblems = new();

    [ObservableProperty]
    private ObservableCollection<UserSubmission> examSubmissions = new();

    [ObservableProperty]
    private Problem? selectedExamProblem;

    [ObservableProperty]
    private UserSubmission? selectedSubmission;

    public bool IsAiNotBusy => !IsAiBusy;

    [RelayCommand]
    private void ReturnHome()
    {
        RequestHomeNavigation?.Invoke();
    }

    public void LoadFromExam(ExamViewModel source)
    {
        ScoreGrade = source.ScoreGrade;
        CorrectCount = source.CorrectCount;
        AnsweredCount = source.AnsweredCount;
        TotalProblems = source.TotalProblems;
        ElapsedTimeText = source.ElapsedTimeText;
        ScorePercent = source.ScorePercent;
        LastSubmittedCode = source.LastSubmittedCode;
        OutputResult = source.OutputResult;
        AiQuestion = source.AiQuestion;
        AiAnswer = source.AiAnswer;
        AiAnalysis = source.AiAnalysis;
        ExamProblems = new ObservableCollection<Problem>(source.ExamProblems);
        ExamSubmissions = new ObservableCollection<UserSubmission>(source.ExamSubmissions);
        SelectedExamProblem = ExamProblems.Count > 0 ? ExamProblems[0] : null;
        SelectedSubmission = ExamSubmissions.Count > 0 ? ExamSubmissions[0] : null;
    }

    [RelayCommand]
    private async Task AskAiAsync()
    {
        if (string.IsNullOrWhiteSpace(AiQuestion))
        {
            AiAnswer = "請先輸入問題。";
            return;
        }

        IsAiBusy = true;
        try
        {
            AiAnswer = await _ollamaService.AskAsync(AiQuestion);
        }
        finally
        {
            IsAiBusy = false;
        }
    }

    [RelayCommand]
    private async Task AnalyzeCodeAsync()
    {
        if (string.IsNullOrWhiteSpace(LastSubmittedCode))
        {
            AiAnalysis = "目前沒有提交的程式碼可分析。";
            return;
        }

        IsAiBusy = true;
        try
        {
            AiAnalysis = await _ollamaService.AnalyzeCodeAsync(LastSubmittedCode, string.Empty);
        }
        finally
        {
            IsAiBusy = false;
        }
    }

    partial void OnIsAiBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsAiNotBusy));
    }
}
