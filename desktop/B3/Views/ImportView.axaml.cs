using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using B3.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace B3.Views;

public partial class ImportView : UserControl
{
    public ImportView()
    {
        InitializeComponent();
    }

    private ImportViewModel? ViewModel => DataContext as ImportViewModel;

    private async void OnPickSpreadsheetClicked(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null)
        {
            return;
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "選擇題目表單",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Spreadsheet")
                {
                    Patterns = new[] { "*.csv", "*.xlsx", "*.xls" }
                }
            }
        });

        var file = files.FirstOrDefault();
        if (file == null)
        {
            return;
        }

        var localPath = file.Path.LocalPath;
        if (ViewModel != null)
        {
            await ViewModel.PreviewSpreadsheetAsync(localPath);
        }
    }

    private async void OnPreviewSpreadsheetClicked(object? sender, RoutedEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        await ViewModel.PreviewSpreadsheetAsync(ViewModel.SpreadsheetFilePath);
    }

    private async void OnImportSpreadsheetClicked(object? sender, RoutedEventArgs e)
    {
        if (ViewModel == null)
        {
            return;
        }

        await ViewModel.ImportSpreadsheetAsync();
    }

    private async void OnDownloadTemplateClicked(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null || ViewModel == null)
        {
            return;
        }

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "下載匯入模板",
            SuggestedFileName = "problem-import-template.csv",
            DefaultExtension = ".csv"
        });

        if (file == null)
        {
            return;
        }

        var localPath = file.Path.LocalPath;
        await ViewModel.DownloadTemplateAsync(localPath);
    }
}
