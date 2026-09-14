using System;
using Avalonia.Controls;
using B3.Models;
using B3.Services;
using B3.ViewModels;

namespace B3.Views;

public partial class SettingsView : UserControl
{
    private readonly CodeSyntaxColorizer _previewColorizer;
    private SettingsViewModel? _viewModel;

    public SettingsView()
    {
        InitializeComponent();
        _previewColorizer = CodeEditorHighlighting.Attach(SyntaxPreviewEditor, new SyntaxColorSettings());
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (_viewModel != null)
        {
            _viewModel.SyntaxAppearanceChanged -= RefreshSyntaxPreview;
        }

        _viewModel = DataContext as SettingsViewModel;
        if (_viewModel != null)
        {
            _viewModel.SyntaxAppearanceChanged += RefreshSyntaxPreview;
            RefreshSyntaxPreview();
        }
    }

    // 配色、開關或預覽語言變更時重繪預覽
    private void RefreshSyntaxPreview()
    {
        if (_viewModel == null)
            return;

        var sample = GetSampleCode(_viewModel.SyntaxPreviewLanguage);
        if (SyntaxPreviewEditor.Text != sample)
        {
            SyntaxPreviewEditor.Text = sample;
        }

        CodeEditorHighlighting.Apply(SyntaxPreviewEditor, _previewColorizer, _viewModel.BuildSyntaxColorSettings(),
            _viewModel.SyntaxPreviewLanguage, _viewModel.EnableSyntaxHighlighting);
    }

    private static string GetSampleCode(string language) => language switch
    {
        "Python" => "import sys\n\n@staticmethod\ndef solve(n: int) -> str:\n    \"\"\"計算總和\"\"\"\n    total = 0\n    for i in range(1, n + 1):  # 累加\n        total += i * 2.5\n    return f\"Sum: {total}\"\n\nif __name__ == '__main__':\n    print(solve(int(input())))",
        "C#" => "using System;\n\n#region Solution\npublic class Program\n{\n    /* 計算總和 */\n    public static void Main(string[] args)\n    {\n        int n = int.Parse(Console.ReadLine());\n        double total = 0;\n        for (var i = 1; i <= n; i++) // 累加\n            total += i * 2.5;\n        Console.WriteLine($\"Sum: {total}\");\n    }\n}\n#endregion",
        _ => "#include <iostream>\nusing namespace std;\n\n/* 計算總和 */\nint main() {\n    int n;\n    cin >> n;\n    double total = 0;\n    for (int i = 1; i <= n; i++) { // 累加\n        total += i * 2.5;\n    }\n    cout << \"Sum: \" << total << endl;\n    return 0;\n}"
    };
}
