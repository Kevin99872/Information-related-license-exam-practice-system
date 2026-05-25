using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using B3.Services;
using B3.ViewModels;
using B3.Views;

namespace B3;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
        {
            return null;
        }

        try
        {
            return param switch
            {
                HomeViewModel => new HomeView(),
                ProblemListViewModel => new ProblemListView(),
                BankViewModel => new BankView(),
                ImportViewModel => new ImportView(),
                StyleViewModel => new StyleView(),
                SettingsViewModel => new SettingsView(),
                QuestionHubViewModel => new QuestionHubView(),
                ReviewViewModel => new ReviewView(),
                ExamViewModel => new ExamView(),
                ExamResultViewModel => new ExamResultView(),
                _ => new TextBlock { Text = "Not Found: " + param.GetType().FullName }
            };
        }
        catch (Exception ex)
        {
            LoggerService.LogError($"View 建立失敗: {param.GetType().FullName}", ex);
            return new TextBlock { Text = "View Create Error: " + param.GetType().FullName };
        }
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
