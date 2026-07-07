using Avalonia.Controls;
using Avalonia;
using B3.ViewModels;
using Avalonia.Input;
using System.Diagnostics;
using Avalonia.Threading;
using Avalonia.Media;
using System;

namespace B3.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
    }

    private MainWindowViewModel? _vm;
    private Grid? _sidebarInner;
    private DispatcherTimer? _sidebarTimer;
    private Stopwatch? _sidebarStopwatch;
    private double _sidebarFrom;
    private double _sidebarTo;
    private double _sidebarWidthCurrent = ExpandedWidth;
    private static readonly System.TimeSpan SidebarAnimDuration = System.TimeSpan.FromMilliseconds(200);
    private const double ExpandedWidth = 240;
    private const double CollapsedWidth = 72;

    protected override void OnOpened(System.EventArgs e)
    {
        base.OnOpened(e);
        _sidebarInner = this.FindControl<Grid>("SidebarInner");
        HookDataContext();
    }

    private void HookDataContext()
    {
        if (_vm != null)
            _vm.PropertyChanged -= Vm_PropertyChanged;

        _vm = DataContext as MainWindowViewModel;

        if (_vm != null)
            _vm.PropertyChanged += Vm_PropertyChanged;
    }

    private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.IsSidebarExpanded) || e.PropertyName == nameof(MainWindowViewModel.IsSidebarCollapsed))
        {
            if (_vm != null)
                AnimateSidebar(_vm.IsSidebarExpanded);
        }
    }

    private void AnimateSidebar(bool expanded)
    {
        if (_sidebarInner == null)
            return;

        var transform = _sidebarInner.RenderTransform as TranslateTransform;
        if (transform == null)
        {
            transform = new TranslateTransform();
            _sidebarInner.RenderTransform = transform;
        }

        _sidebarFrom = _sidebarWidthCurrent;
        _sidebarTo = expanded ? ExpandedWidth : CollapsedWidth;

        _sidebarTimer?.Stop();

        _sidebarStopwatch = Stopwatch.StartNew();
        // 5ms ≈ 200fps，進度以 Stopwatch 實際經過時間計算，計時器頻率只影響更新密度
        _sidebarTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(5), DispatcherPriority.Render, (s, ev) =>
        {
            if (_sidebarStopwatch == null) return;
            var progress = _sidebarStopwatch.Elapsed.TotalMilliseconds / SidebarAnimDuration.TotalMilliseconds;
            if (progress >= 1.0)
            {
                _sidebarWidthCurrent = _sidebarTo;
                if (_vm != null)
                    _vm.SidebarWidth = _sidebarTo;
                transform.X = _sidebarTo - ExpandedWidth;
                _sidebarTimer?.Stop();
                _sidebarStopwatch?.Stop();
                return;
            }

            var eased = EaseOutCubic(progress);
            _sidebarWidthCurrent = _sidebarFrom + (_sidebarTo - _sidebarFrom) * eased;
            if (_vm != null)
                _vm.SidebarWidth = _sidebarWidthCurrent;
            transform.X = _sidebarWidthCurrent - ExpandedWidth;
        });

        _sidebarTimer.Start();
    }

    private static double EaseOutCubic(double t)
    {
        t = Math.Clamp(t, 0, 1);
        return 1 - Math.Pow(1 - t, 3);
    }

    private void OnSidebarPointerEntered(object? sender, PointerEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SetSidebarHover(true);
        }
    }

    private void OnSidebarPointerExited(object? sender, PointerEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SetSidebarHover(false);
        }
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.UpdateResponsiveState(Bounds.Width);
        }
    }
}