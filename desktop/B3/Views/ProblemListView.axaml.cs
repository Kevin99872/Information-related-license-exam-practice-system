using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace B3.Views;

public partial class ProblemListView : UserControl
{
    private ScrollViewer? _editorScrollViewer;

    public ProblemListView()
    {
        InitializeComponent();
        ProblemEditor.AddHandler(ScrollViewer.ScrollChangedEvent, OnEditorScrollChanged);
    }

    // 將編輯器的垂直捲動量同步到行號欄，使兩者捲動一致。
    private void OnEditorScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        _editorScrollViewer ??= ProblemEditor.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        if (_editorScrollViewer is null)
            return;

        ProblemLineNumberScroll.Offset = new Vector(0, _editorScrollViewer.Offset.Y);
    }
}
