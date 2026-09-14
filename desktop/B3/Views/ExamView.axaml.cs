using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using B3.Services;
using B3.ViewModels;

namespace B3.Views;

public partial class ExamView : UserControl
{
    private readonly LocalSettingsService _settingsService = new();
    private readonly CodeSyntaxColorizer _colorizer;
    private ExamViewModel? _viewModel;
    private bool _syncingEditor;
    private bool _highlightEnabled = true;

    public ExamView()
    {
        InitializeComponent();

        ExamEditor.Options.ConvertTabsToSpaces = true;
        ExamEditor.Options.IndentationSize = 4;
        ExamEditor.Options.EnableHyperlinks = false;
        ExamEditor.Options.EnableEmailHyperlinks = false;

        _colorizer = CodeEditorHighlighting.Attach(ExamEditor, _settingsService.Load().SyntaxColors ?? new());
        ExamEditor.TextChanged += OnEditorTextChanged;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        // 每次進入考試頁重新讀取設定，讓設定頁的配色調整立即生效
        ApplyHighlightSettings();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _viewModel = DataContext as ExamViewModel;
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            SyncEditorFromViewModel();
            UpdateHighlightLanguage();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ExamViewModel.EditorBuffer):
                SyncEditorFromViewModel();
                break;
            case nameof(ExamViewModel.SelectedLanguage):
            case nameof(ExamViewModel.SelectedEditorFile):
                UpdateHighlightLanguage();
                break;
        }
    }

    // ViewModel → 編輯器 (切換檔案、切換語言時)
    private void SyncEditorFromViewModel()
    {
        if (_viewModel == null || ExamEditor.Text == _viewModel.EditorBuffer)
            return;

        _syncingEditor = true;
        ExamEditor.Text = _viewModel.EditorBuffer ?? string.Empty;
        _syncingEditor = false;
    }

    // 編輯器 → ViewModel (使用者輸入時)
    private void OnEditorTextChanged(object? sender, EventArgs e)
    {
        if (_syncingEditor || _viewModel == null)
            return;

        _viewModel.EditorBuffer = ExamEditor.Text;
    }

    private void ApplyHighlightSettings()
    {
        var settings = _settingsService.Load();
        _highlightEnabled = settings.EnableSyntaxHighlighting;
        CodeEditorHighlighting.Apply(ExamEditor, _colorizer, settings.SyntaxColors ?? new(), GetHighlightLanguage(), _highlightEnabled);
    }

    private void UpdateHighlightLanguage()
    {
        _colorizer.SetLanguage(_highlightEnabled ? GetHighlightLanguage() : null);
        ExamEditor.TextArea.TextView.Redraw();
    }

    // input.txt 等非程式檔不上色
    private string? GetHighlightLanguage() =>
        _viewModel?.SelectedEditorFile?.Kind == EditorFileKind.Input ? null : _viewModel?.SelectedLanguage;
}
