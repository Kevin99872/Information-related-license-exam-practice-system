using CommunityToolkit.Mvvm.ComponentModel;

namespace B3.ViewModels;

/// <summary>
/// 匯入ViewModel - 題庫匯入流程入口
/// </summary>
public partial class ImportViewModel : ViewModelBase
{
    /// <summary>可匯入格式提示</summary>
    [ObservableProperty]
    private string supportedFormats = "CSV / JSON / TXT";
}
