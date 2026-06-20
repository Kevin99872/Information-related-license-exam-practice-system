using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace B3.Views;

public partial class ExamView : UserControl
{
    private ScrollViewer? _editorScrollViewer;

    public ExamView()
    {
        InitializeComponent();
        ExamEditor.AddHandler(ScrollViewer.ScrollChangedEvent, OnEditorScrollChanged);
    }

    // 將編輯器的垂直捲動量同步到行號欄，使兩者捲動一致。
    private void OnEditorScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        _editorScrollViewer ??= ExamEditor.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        if (_editorScrollViewer is null)
            return;

        ExamLineNumberScroll.Offset = new Vector(0, _editorScrollViewer.Offset.Y);
    }
}
