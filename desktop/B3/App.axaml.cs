using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System;
using System.Linq;
using Avalonia.Markup.Xaml;
using B3.ViewModels;
using B3.Views;
using B3.Services;
using Avalonia.Styling;

namespace B3;

public partial class App : Application
{
    public Models.AppSettings CurrentSettings { get; private set; } = new Models.AppSettings();

    private readonly Services.LocalSettingsService _settingsService = new Services.LocalSettingsService();
    public override void Initialize()
    {
        LoggerService.LogDebug("App.Initialize() 開始...");
        try
        {
            AvaloniaXamlLoader.Load(this);
            LoggerService.LogDebug("Xaml資源加載完成");
            // 載入本機設定並套用樣式
            try
            {
                CurrentSettings = _settingsService.Load();
                ApplySettings(CurrentSettings);
                LoggerService.LogDebug("本機樣式設定已載入並套用");
            }
            catch (Exception ex)
            {
                LoggerService.LogError("載入或套用本機樣式設定時發生錯誤", ex);
            }
        }
        catch (Exception ex)
        {
            LoggerService.LogError("Xaml加載失敗", ex);
            throw;
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        LoggerService.LogDebug("App.OnFrameworkInitializationCompleted() 開始...");
        try
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var vm = new MainWindowViewModel();
                var mainWindow = new MainWindow
                {
                    DataContext = vm,
                };
                desktop.MainWindow = mainWindow;
                LoggerService.LogDebug("主窗口創建成功");
            }

            base.OnFrameworkInitializationCompleted();
            LoggerService.Log("App 初始化完成");
        }
        catch (Exception ex)
        {
            LoggerService.LogError("App 初始化失敗", ex);
            throw;
        }
    }

    /// <summary>
    /// 將 AppSettings 套用到全域資源
    /// </summary>
    public void ApplySettings(Models.AppSettings settings)
    {
        if (settings == null) return;
        CurrentSettings = settings;

        // 更新資源字典
        try
        {
            Resources["GlobalFontSize"] = (double)settings.FontSize;
            Resources["GlobalFontFamily"] = new Avalonia.Media.FontFamily(settings.FontFamily ?? string.Empty);
            Resources["ShowProgress"] = settings.ShowProgress;

            // apply theme colors for 深色 / 淺色; 系統設定 = 不改變（保留預設或先前值）
            if (!string.IsNullOrWhiteSpace(settings.ThemeName))
            {
                if (settings.ThemeName == "淺色")
                {
                    RequestedThemeVariant = ThemeVariant.Light;
                    // light theme colors
                    Resources["PrimaryColor"] = Avalonia.Media.Color.Parse("#2563EB");
                    Resources["PrimaryDarkColor"] = Avalonia.Media.Color.Parse("#1D4ED8");
                    Resources["SurfaceColor"] = Avalonia.Media.Color.Parse("#FFFFFF");
                    Resources["PageBgColor"] = Avalonia.Media.Color.Parse("#F3F4F6");
                    Resources["SidebarBgColor"] = Avalonia.Media.Color.Parse("#F8FAFC");
                    Resources["MutedTextColor"] = Avalonia.Media.Color.Parse("#6B7280");
                    Resources["TextColor"] = Avalonia.Media.Color.Parse("#111827");
                    Resources["TagBgColor"] = Avalonia.Media.Color.Parse("#EEF2FF");
                    Resources["TagAltBgColor"] = Avalonia.Media.Color.Parse("#F8FAFF");
                    Resources["IconBgColor"] = Avalonia.Media.Color.Parse("#F1F5F9");
                    Resources["SoftPanelColor"] = Avalonia.Media.Color.Parse("#FFFFFF");
                    Resources["CardBorderColor"] = Avalonia.Media.Color.Parse("#E5E7EB");
                }
                else if (settings.ThemeName == "深色")
                {
                    RequestedThemeVariant = ThemeVariant.Dark;
                    // dark theme (original defaults)
                    Resources["PrimaryColor"] = Avalonia.Media.Color.Parse("#4C7ED6");
                    Resources["PrimaryDarkColor"] = Avalonia.Media.Color.Parse("#355AA5");
                    Resources["SurfaceColor"] = Avalonia.Media.Color.Parse("#151A23");
                    Resources["PageBgColor"] = Avalonia.Media.Color.Parse("#0F131A");
                    Resources["SidebarBgColor"] = Avalonia.Media.Color.Parse("#121823");
                    Resources["MutedTextColor"] = Avalonia.Media.Color.Parse("#A1A8B3");
                    Resources["TextColor"] = Avalonia.Media.Color.Parse("#E6E9EE");
                    Resources["TagBgColor"] = Avalonia.Media.Color.Parse("#1E2A3D");
                    Resources["TagAltBgColor"] = Avalonia.Media.Color.Parse("#222036");
                    Resources["IconBgColor"] = Avalonia.Media.Color.Parse("#1B2434");
                    Resources["SoftPanelColor"] = Avalonia.Media.Color.Parse("#121720");
                    Resources["CardBorderColor"] = Avalonia.Media.Color.Parse("#1F2633");
                }
                else
                {
                    RequestedThemeVariant = ThemeVariant.Default;
                    // 系統設定 - 保留系統主題
                }

                // Update brushes from colors
                try
                {
                    Resources["PrimaryBrush"] = new Avalonia.Media.SolidColorBrush((Avalonia.Media.Color)Resources["PrimaryColor"]);
                    Resources["PrimaryDarkBrush"] = new Avalonia.Media.SolidColorBrush((Avalonia.Media.Color)Resources["PrimaryDarkColor"]);
                    Resources["SurfaceBrush"] = new Avalonia.Media.SolidColorBrush((Avalonia.Media.Color)Resources["SurfaceColor"]);
                    Resources["PageBgBrush"] = new Avalonia.Media.SolidColorBrush((Avalonia.Media.Color)Resources["PageBgColor"]);
                    Resources["SidebarBgBrush"] = new Avalonia.Media.SolidColorBrush((Avalonia.Media.Color)Resources["SidebarBgColor"]);
                    Resources["MutedTextBrush"] = new Avalonia.Media.SolidColorBrush((Avalonia.Media.Color)Resources["MutedTextColor"]);
                    Resources["TextBrush"] = new Avalonia.Media.SolidColorBrush((Avalonia.Media.Color)Resources["TextColor"]);
                    Resources["TagBgBrush"] = new Avalonia.Media.SolidColorBrush((Avalonia.Media.Color)Resources["TagBgColor"]);
                    Resources["TagAltBgBrush"] = new Avalonia.Media.SolidColorBrush((Avalonia.Media.Color)Resources["TagAltBgColor"]);
                    Resources["IconBgBrush"] = new Avalonia.Media.SolidColorBrush((Avalonia.Media.Color)Resources["IconBgColor"]);
                    Resources["SoftPanelBrush"] = new Avalonia.Media.SolidColorBrush((Avalonia.Media.Color)Resources["SoftPanelColor"]);
                    Resources["CardBorderBrush"] = new Avalonia.Media.SolidColorBrush((Avalonia.Media.Color)Resources["CardBorderColor"]);
                }
                catch (Exception ex)
                {
                    LoggerService.LogError("更新 Brush 資源失敗", ex);
                }
            }

        }
        catch (Exception ex)
        {
            LoggerService.LogError("ApplySettings 失敗", ex);
        }
    }
}
