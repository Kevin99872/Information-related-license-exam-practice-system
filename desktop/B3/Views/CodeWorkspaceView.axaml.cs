using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using B3.Services;
using B3.ViewModels;

namespace B3.Views;

/// <summary>
/// Code Space 共用元件 - 檔案清單 + 語法著色編輯器
/// 職責: 編輯器文字與 EditorWorkspace.Buffer 雙向同步；依目前檔案切換語法著色
/// 連結: EditorWorkspace / CodeEditorHighlighting
/// </summary>
public partial class CodeWorkspaceView : UserControl
{
    private readonly LocalSettingsService _settingsService = new();
    private readonly CodeSyntaxColorizer _colorizer;
    private EditorWorkspace? _workspace;
    private bool _syncingEditor;
    private bool _highlightEnabled = true;

    public CodeWorkspaceView()
    {
        InitializeComponent();

        Editor.Options.ConvertTabsToSpaces = true;
        Editor.Options.IndentationSize = 4;
        Editor.Options.EnableHyperlinks = false;
        Editor.Options.EnableEmailHyperlinks = false;

        _colorizer = CodeEditorHighlighting.Attach(Editor, _settingsService.Load().SyntaxColors ?? new());
        Editor.TextChanged += OnEditorTextChanged;
        Editor.TextArea.Caret.PositionChanged += OnCaretPositionChanged;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        // 每次進入頁面重新讀取設定，讓設定頁的配色調整立即生效
        ApplyHighlightSettings();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (_workspace != null)
        {
            _workspace.PropertyChanged -= OnWorkspacePropertyChanged;
        }

        _workspace = DataContext as EditorWorkspace;
        if (_workspace != null)
        {
            _workspace.PropertyChanged += OnWorkspacePropertyChanged;
            SyncEditorFromWorkspace();
            UpdateHighlightLanguage();
            Editor.IsReadOnly = !_workspace.HasFile;
        }
    }

    private void OnWorkspacePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(EditorWorkspace.Buffer):
                SyncEditorFromWorkspace();
                break;
            case nameof(EditorWorkspace.HighlightLanguage):
                UpdateHighlightLanguage();
                break;
            case nameof(EditorWorkspace.HasFile):
                Editor.IsReadOnly = _workspace?.HasFile != true;
                break;
            case nameof(EditorWorkspace.IsDialogOpen):
                FocusDialogOrEditor();
                break;
        }
    }

    // 工作區 → 編輯器 (切換檔案、切換語言、換題時)
    private void SyncEditorFromWorkspace()
    {
        if (_workspace == null || Editor.Text == _workspace.Buffer)
            return;

        _syncingEditor = true;
        Editor.Text = _workspace.Buffer ?? string.Empty;
        _syncingEditor = false;
    }

    // 編輯器 → 工作區 (使用者輸入時)
    private void OnEditorTextChanged(object? sender, EventArgs e)
    {
        if (_syncingEditor || _workspace == null)
            return;

        _workspace.Buffer = Editor.Text;
    }

    private void OnCaretPositionChanged(object? sender, EventArgs e)
    {
        var caret = Editor.TextArea.Caret;
        _workspace?.SetCaret(caret.Line, caret.Column);
    }

    // 對話框開啟時聚焦輸入框 (刪除確認則聚焦取消鈕)；關閉後回到編輯器
    private void FocusDialogOrEditor()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_workspace == null)
                return;

            if (!_workspace.IsDialogOpen)
            {
                Editor.TextArea.Focus();
            }
            else if (_workspace.IsNameDialog)
            {
                DialogNameBox.Focus();
                // 改名時預先選取主檔名，方便直接覆寫
                var name = DialogNameBox.Text ?? string.Empty;
                var dot = name.LastIndexOf('.');
                DialogNameBox.SelectionStart = 0;
                DialogNameBox.SelectionEnd = dot > 0 ? dot : name.Length;
            }
            else
            {
                DialogCancelButton.Focus();
            }
        }, DispatcherPriority.Input);
    }

    private void ApplyHighlightSettings()
    {
        var settings = _settingsService.Load();
        _highlightEnabled = settings.EnableSyntaxHighlighting;
        CodeEditorHighlighting.Apply(Editor, _colorizer, settings.SyntaxColors ?? new(),
            _workspace?.HighlightLanguage, _highlightEnabled);
    }

    private void UpdateHighlightLanguage()
    {
        _colorizer.SetLanguage(_highlightEnabled ? _workspace?.HighlightLanguage : null);
        Editor.TextArea.TextView.Redraw();
    }
}
