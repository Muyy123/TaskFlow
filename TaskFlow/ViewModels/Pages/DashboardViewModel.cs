using CommunityToolkit.Mvvm.ComponentModel;
using TaskFlow.Services;

namespace TaskFlow.ViewModels.Pages;

/// <summary>仪表盘页面 ViewModel: 加载统计数据，展示KPI卡片</summary>
public partial class DashboardViewModel : ViewModelBase
{

    private readonly ITaskService _taskService;

    public DashboardViewModel(ITaskService taskService)
    {
        _taskService = taskService;
        _ = LoadStatsAsync(); //页面打开自动加载统计
    }
    /// <summary>
    /// 是否正在加载
    /// </summary>
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _todoCount;

    [ObservableProperty]
    private int _inProgressCount;

    [ObservableProperty]
    private int _doneCount;

    [ObservableProperty]
    private int _overdueCount;

    /// <summary>
    /// 加载统计数据，填充各卡片数字
    /// </summary>
    private async Task LoadStatsAsync()
    {
        IsLoading = true;

        try
        {
            // 调用服务获取聚合统计结果
            var stats = await _taskService.GetDashboardStatsAsync();

            TotalCount = stats.TotalCount;
            TodoCount = stats.TodoCount;
            InProgressCount = stats.InProgressCount;
            DoneCount = stats.DoneCount;
            OverdueCount = stats.OverdueCount;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
