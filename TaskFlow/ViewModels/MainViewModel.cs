using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TaskFlow.Theme;
using TaskFlow.ViewModels.Pages;

namespace TaskFlow.ViewModels;

/// <summary>
/// 主窗口的 ViewModel：管理左侧导航项与内容区当前页面的切换。
/// 各页面 ViewModel 通过构造函数注入（而非 new），这样页面可以依赖 Service。
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    /// <summary>内容区当前显示的页面 ViewModel（由 MainWindow 的 ContentControl 绑定）。</summary>
    [ObservableProperty]
    private ObservableObject? _currentViewModel;

    /// <summary>导航栏当前选中项。</summary>
    [ObservableProperty]
    private NavItemViewModel? _selectedNavItem;

    private readonly ThemeService _themeService;
    // 按钮文字，[ObservableProperty]自动通知ui
    [ObservableProperty]
    private string _themeButtonText = "切换深色";

    public ObservableCollection<NavItemViewModel> NavItems { get; }

    public MainViewModel(ThemeService themeService,
        DashboardViewModel dashboard,
        ProjectsViewModel projects,
        TaskListViewModel tasks,
        SettingsViewModel settings)
    {
        _themeService = themeService;
        NavItems = new ObservableCollection<NavItemViewModel>
        {
            new("仪表盘", dashboard),
            new("项目", projects),
            new("任务", tasks),
            new("设置", settings),
        };

        // 默认选中第一项（走属性 setter，触发 OnSelectedNavItemChanged 切换内容区）
        SelectedNavItem = NavItems[0];
    }

    // 由 CommunityToolkit.Mvvm 的 [ObservableProperty] 生成的 setter 调用
    partial void OnSelectedNavItemChanged(NavItemViewModel? value)
    {
        if (value is not null)
        {
            CurrentViewModel = value.ViewModel;
        }
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        _themeService.ApplyTheme(!_themeService.IsDark);
        ThemeButtonText = _themeService.IsDark ? "切换浅色" : "切换深色";
    }
}

/// <summary>导航项：标题 + 对应的页面 ViewModel。</summary>
public sealed record NavItemViewModel(string Title, ObservableObject ViewModel);
