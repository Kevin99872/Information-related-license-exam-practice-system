using B3.Localization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace B3.ViewModels;

/// <summary>編輯器檔案類型</summary>
public enum EditorFileKind
{
    /// <summary>主要程式檔 (送出判題的檔案，不可改名 / 刪除)</summary>
    Source,
    /// <summary>試跑輸入資料 input.txt (不可改名 / 刪除)</summary>
    Input,
    /// <summary>使用者自行新增的檔案</summary>
    UserAdded
}

/// <summary>檔案管理對話框種類</summary>
public enum WorkspaceDialog
{
    None,
    Add,
    Rename,
    Delete
}

/// <summary>
/// 程式語言與檔名的對應表 - 新增語言時只需修改這裡
/// 連結: EditorWorkspace / CodeWorkspaceView (依副檔名決定語法著色)
/// </summary>
public static class EditorLanguages
{
    /// <summary>取得語言的主要程式檔名</summary>
    public static string SourceFileName(string language) => language switch
    {
        "C#" => "Program.cs",
        "C++" => "main.cpp",
        "Java" => "Main.java",
        _ => "main.py"
    };

    /// <summary>取得語言的預設範例程式</summary>
    public static string Template(string language) => language switch
    {
        "C#" => "using System;\n\nConsole.WriteLine(\"Hello World\");",
        "C++" => "#include <bits/stdc++.h>\nusing namespace std;\n\nint main() {\n    ios::sync_with_stdio(false);\n    cin.tie(nullptr);\n\n    cout << \"Hello World\" << endl;\n    return 0;\n}",
        "Java" => "public class Main {\n    public static void main(String[] args) {\n        System.out.println(\"Hello World\");\n    }\n}",
        _ => "print('Hello World')"
    };

    /// <summary>依副檔名判斷語法著色語言；非程式檔回傳 null (不著色)</summary>
    public static string? FromFileName(string? fileName) =>
        Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant() switch
        {
            ".py" => "Python",
            ".cs" => "C#",
            ".cpp" or ".cc" or ".cxx" or ".c" or ".h" or ".hpp" => "C++",
            ".java" => "Java",
            _ => null
        };
}

/// <summary>編輯器檔案項目 - 檔名可變更，內容隨編輯即時寫回</summary>
public partial class EditorFileItem : ObservableObject
{
    /// <summary>檔名</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Badge))]
    private string name = string.Empty;

    public EditorFileItem(EditorFileKind kind, string name, string description, string content = "")
    {
        Kind = kind;
        Name = name;
        Description = description;
        Content = content;
    }

    public EditorFileKind Kind { get; }

    /// <summary>檔案說明 (顯示於檔名下方)</summary>
    public string Description { get; }

    /// <summary>檔案內容</summary>
    public string Content { get; set; }

    /// <summary>是否可改名 / 刪除 (僅使用者新增的檔案)</summary>
    public bool CanManage => Kind == EditorFileKind.UserAdded;

    /// <summary>語法著色語言，null 表示純文字</summary>
    public string? Language => EditorLanguages.FromFileName(Name);

    /// <summary>檔案類型標籤 (副檔名大寫，例如 PY / CPP / TXT)</summary>
    public string Badge
    {
        get
        {
            var ext = Path.GetExtension(Name).TrimStart('.').ToUpperInvariant();
            return ext.Length == 0 ? "FILE" : ext.Length > 4 ? ext[..4] : ext;
        }
    }
}

/// <summary>
/// 編輯器工作區 - 管理 Code Space 的檔案清單、目前檔案內容與檔案管理 (新增 / 改名 / 刪除)
/// 考試頁與練習頁共用
/// 連結: ExamViewModel.Workspace / ProblemListViewModel.Workspace / CodeWorkspaceView
/// </summary>
public partial class EditorWorkspace : ObservableObject
{
    private const int MaxNameLength = 64;

    private readonly EditorFileItem _source;
    private readonly EditorFileItem _input;
    private EditorFileItem? _dialogTarget;
    private bool _loading;

    /// <summary>檔案清單</summary>
    public ObservableCollection<EditorFileItem> Files { get; } = new();

    /// <summary>目前編輯的檔案</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HighlightLanguage))]
    [NotifyPropertyChangedFor(nameof(HasFile))]
    [NotifyCanExecuteChangedFor(nameof(RenameFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteFileCommand))]
    private EditorFileItem? selectedFile;

    /// <summary>目前檔案的內容 (與編輯器雙向同步)</summary>
    [ObservableProperty]
    private string buffer = string.Empty;

    /// <summary>編輯器狀態列文字 (語言 / 行數 / 游標位置)</summary>
    [ObservableProperty]
    private string statusText = string.Empty;

    /// <summary>目前開啟的對話框</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDialogOpen))]
    [NotifyPropertyChangedFor(nameof(IsNameDialog))]
    [NotifyPropertyChangedFor(nameof(DialogTitle))]
    [NotifyPropertyChangedFor(nameof(DialogMessage))]
    private WorkspaceDialog dialog = WorkspaceDialog.None;

    /// <summary>對話框中輸入的檔名</summary>
    [ObservableProperty]
    private string dialogName = string.Empty;

    /// <summary>對話框的錯誤訊息 (檔名不合法時顯示)</summary>
    [ObservableProperty]
    private string dialogError = string.Empty;

    private int _caretLine = 1;
    private int _caretColumn = 1;

    public EditorWorkspace(string language = "Python")
    {
        Language = language;
        _source = new EditorFileItem(EditorFileKind.Source, EditorLanguages.SourceFileName(language),
            LocalizationService.T("MainSourceFile"), EditorLanguages.Template(language));
        _input = new EditorFileItem(EditorFileKind.Input, "input.txt", LocalizationService.T("InputFileDesc"));
        Files.Add(_source);
        Files.Add(_input);
        SelectedFile = _source;
    }

    /// <summary>主要程式語言 (決定主要程式檔名與新檔預設副檔名)</summary>
    public string Language { get; private set; }

    /// <summary>目前檔案的語法著色語言，null 表示不著色</summary>
    public string? HighlightLanguage => SelectedFile?.Language;

    /// <summary>是否有選取檔案 (無選取時編輯器唯讀)</summary>
    public bool HasFile => SelectedFile != null;

    public bool IsDialogOpen => Dialog != WorkspaceDialog.None;

    /// <summary>是否為需要輸入檔名的對話框 (新增 / 改名)</summary>
    public bool IsNameDialog => Dialog is WorkspaceDialog.Add or WorkspaceDialog.Rename;

    public string DialogTitle => Dialog switch
    {
        WorkspaceDialog.Add => LocalizationService.T("AddFile"),
        WorkspaceDialog.Rename => LocalizationService.T("RenameFile"),
        WorkspaceDialog.Delete => LocalizationService.T("DeleteFile"),
        _ => string.Empty
    };

    public string DialogMessage => Dialog == WorkspaceDialog.Delete && _dialogTarget != null
        ? string.Format(LocalizationService.T("DeleteFileConfirmFmt"), _dialogTarget.Name)
        : string.Empty;

    /// <summary>主要程式檔內容 (送出判題用)</summary>
    public string SourceCode
    {
        get => _source.Content;
        set
        {
            _source.Content = value ?? string.Empty;
            if (SelectedFile == _source)
            {
                LoadBuffer();
            }
        }
    }

    /// <summary>input.txt 內容 (試跑時的自訂輸入)</summary>
    public string InputText => _input.Content;

    /// <summary>主要程式檔以外的所有檔案 (檔名 → 內容)，執行時一併寫入工作目錄</summary>
    public IReadOnlyDictionary<string, string> ExtraFiles =>
        Files.Where(f => f.Kind != EditorFileKind.Source).ToDictionary(f => f.Name, f => f.Content);

    /// <summary>重設工作區：移除自訂檔案、清空輸入，主要程式檔換成指定內容 (未指定則用範例程式)</summary>
    public void Reset(string language, string? source = null)
    {
        Dialog = WorkspaceDialog.None;
        SelectedFile = _source;
        foreach (var file in Files.Where(f => f.CanManage).ToList())
        {
            Files.Remove(file);
        }

        Language = language;
        _source.Name = EditorLanguages.SourceFileName(language);
        _source.Content = string.IsNullOrWhiteSpace(source) ? EditorLanguages.Template(language) : source;
        _input.Content = string.Empty;
        OnPropertyChanged(nameof(HighlightLanguage));
        LoadBuffer();
    }

    /// <summary>切換主要語言：主要程式檔改名；內容為空或仍是舊範例時換成新語言範例</summary>
    public void SetLanguage(string language)
    {
        var oldTemplate = EditorLanguages.Template(Language);
        Language = language;
        _source.Name = EditorLanguages.SourceFileName(language);
        if (string.IsNullOrWhiteSpace(_source.Content) || _source.Content.Trim() == oldTemplate.Trim())
        {
            _source.Content = EditorLanguages.Template(language);
        }

        OnPropertyChanged(nameof(HighlightLanguage));
        if (SelectedFile == _source)
        {
            LoadBuffer();
        }
        else
        {
            UpdateStatus();
        }
    }

    /// <summary>更新游標位置 (由 CodeWorkspaceView 呼叫)</summary>
    public void SetCaret(int line, int column)
    {
        _caretLine = line;
        _caretColumn = column;
        UpdateStatus();
    }

    // ==================== 檔案管理 ====================

    /// <summary>開啟新增檔案對話框</summary>
    [RelayCommand]
    private void AddFile()
    {
        _dialogTarget = null;
        DialogName = string.Empty;
        DialogError = string.Empty;
        Dialog = WorkspaceDialog.Add;
    }

    /// <summary>開啟重新命名對話框 (對象為目前選取的檔案)</summary>
    [RelayCommand(CanExecute = nameof(CanManageSelected))]
    private void RenameFile()
    {
        _dialogTarget = SelectedFile;
        DialogName = SelectedFile?.Name ?? string.Empty;
        DialogError = string.Empty;
        Dialog = WorkspaceDialog.Rename;
    }

    /// <summary>開啟刪除確認對話框 (對象為目前選取的檔案)</summary>
    [RelayCommand(CanExecute = nameof(CanManageSelected))]
    private void DeleteFile()
    {
        _dialogTarget = SelectedFile;
        DialogError = string.Empty;
        Dialog = WorkspaceDialog.Delete;
    }

    /// <summary>確認對話框 - 依種類執行新增 / 改名 / 刪除</summary>
    [RelayCommand]
    private void ConfirmDialog()
    {
        switch (Dialog)
        {
            case WorkspaceDialog.Add:
            {
                var newName = DialogName.Trim();
                // 未輸入副檔名時，自動補上目前語言的副檔名
                if (newName.Length > 0 && !newName.Contains('.'))
                {
                    newName += Path.GetExtension(EditorLanguages.SourceFileName(Language));
                }

                if (!TryValidateName(newName, null))
                {
                    return;
                }

                var file = new EditorFileItem(EditorFileKind.UserAdded, newName, LocalizationService.T("UserFileDesc"));
                Files.Add(file);
                SelectedFile = file;
                break;
            }
            case WorkspaceDialog.Rename when _dialogTarget != null:
            {
                var newName = DialogName.Trim();
                if (!TryValidateName(newName, _dialogTarget))
                {
                    return;
                }

                _dialogTarget.Name = newName;
                OnPropertyChanged(nameof(HighlightLanguage));
                UpdateStatus();
                break;
            }
            case WorkspaceDialog.Delete when _dialogTarget != null:
            {
                // 先把選取移到上一個檔案，再移除 (主要程式檔固定在最前面，一定存在)
                if (SelectedFile == _dialogTarget)
                {
                    var index = Files.IndexOf(_dialogTarget);
                    SelectedFile = Files[Math.Max(0, index - 1)];
                }

                Files.Remove(_dialogTarget);
                break;
            }
        }

        CancelDialog();
    }

    /// <summary>關閉對話框</summary>
    [RelayCommand]
    private void CancelDialog()
    {
        Dialog = WorkspaceDialog.None;
        DialogError = string.Empty;
        _dialogTarget = null;
    }

    private bool CanManageSelected() => SelectedFile?.CanManage == true;

    /// <summary>檢查檔名是否合法，不合法時設定 DialogError 並回傳 false</summary>
    private bool TryValidateName(string newName, EditorFileItem? self)
    {
        string? error = null;
        if (string.IsNullOrWhiteSpace(newName))
        {
            error = "FileNameEmpty";
        }
        else if (newName.Length > MaxNameLength)
        {
            error = "FileNameTooLong";
        }
        else if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || newName.Trim('.').Length == 0)
        {
            error = "FileNameInvalid";
        }
        else if (Files.Any(f => f != self && f.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
        {
            error = "FileNameExists";
        }

        DialogError = error == null ? string.Empty : LocalizationService.T(error);
        return error == null;
    }

    // ==================== 內容同步 ====================

    partial void OnSelectedFileChanged(EditorFileItem? value) => LoadBuffer();

    partial void OnBufferChanged(string value)
    {
        if (!_loading && SelectedFile != null)
        {
            SelectedFile.Content = value;
        }

        UpdateStatus();
    }

    /// <summary>把目前檔案的內容載入編輯器緩衝 (不回寫檔案)</summary>
    private void LoadBuffer()
    {
        _loading = true;
        Buffer = SelectedFile?.Content ?? string.Empty;
        _loading = false;
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        var lines = string.IsNullOrEmpty(Buffer) ? 1 : Buffer.Count(c => c == '\n') + 1;
        var language = HighlightLanguage ?? LocalizationService.T("PlainTextLabel");
        StatusText = $"{language}  ·  {string.Format(LocalizationService.T("LinesFmt"), lines)}  ·  Ln {_caretLine}, Col {_caretColumn}";
    }
}
