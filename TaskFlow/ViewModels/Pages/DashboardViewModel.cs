using CommunityToolkit.Mvvm.ComponentModel;
using TaskFlow.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

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

    /// <summary>状态分布饼图的系列数据（每个状态一个扇区）。</summary>
    [ObservableProperty]
    private ISeries[] _statusSeries = Array.Empty<ISeries>();

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

            // 构建状态分布饼图：每个 PieSeries 是一个扇区
            // Values 传数组（这里每个扇区只放一个值）；Name 是扇区在图例上的名字
            StatusSeries = new ISeries[]
            {
                new PieSeries<double> { Values = new double[] {stats.TodoCount}, Name = "待办"},
                new PieSeries<double> { Values = new double[] {stats.InProgressCount}, Name = "进行中"},
                new PieSeries<double> { Values = new double[] {stats.DoneCount}, Name = "已完成"},
            };
        }
        finally
        {
            IsLoading = false;
        }
    }
}
