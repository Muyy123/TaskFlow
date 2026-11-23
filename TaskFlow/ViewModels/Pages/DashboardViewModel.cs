using CommunityToolkit.Mvvm.ComponentModel;
using TaskFlow.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using TaskFlow.Models.Dtos;

namespace TaskFlow.ViewModels.Pages;

/// <summary>仪表盘页面 ViewModel: 加载统计数据，展示KPI卡片</summary>
public partial class DashboardViewModel : ViewModelBase
{

    private readonly ITaskService _taskService;
    private readonly IProjectService _projectService;

    public DashboardViewModel(ITaskService taskService, IProjectService projectService)
    {
        _taskService = taskService;
        _projectService = projectService;
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

    /// <summary>项目任务数柱状图的系列数据</summary>
    [ObservableProperty]
    private ISeries[] _projectSeries = Array.Empty<ISeries>();

    ///<summary>柱状图X轴（项目名称）</summary>
    [ObservableProperty]
    private Axis[] _projectXAxes = Array.Empty<Axis>();
    ///<summary>柱状图Y轴（任务数）</summary>
    [ObservableProperty]
    private Axis[] _projectYAxes = Array.Empty<Axis>();

    /// <summary>
    /// 加载统计数据，填充各卡片数字
    /// </summary>
    public async Task LoadStatsAsync()
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

            // ——项目任务数柱状图——
            // GetListAsync 已经 Include(Tasks)，所以 Tasks.Count 可用；按项目名排序让柱子稳定
            var projects = (await _projectService.GetListAsync(new ProjectFilter()))
                .OrderBy(p => p.Name)
                .ToList();

            var names = projects.Select(p => p.Name).ToArray();       // X 轴文字
            var counts = projects.Select(p => p.Tasks.Count).ToArray(); // Y轴数值，每个项目的任务数

            // ColumnSeries柱状图  Values柱子高度的数字源  Name图例显示的文字
            ProjectSeries = new ISeries[]
            {
                new ColumnSeries<int> { Values = counts, Name = "任务数" }
            };

            ProjectXAxes = new Axis[] { new Axis { Labels = names, LabelsRotation = 45 } }; // X轴，把names贴在柱子下面; LabelsRotation字体调整旋转角度
            ProjectYAxes = new Axis[] { new Axis { MinLimit = 0 } };   // Y轴，强制Y轴最小值是0

        }
        finally
        {
            IsLoading = false;
        }
    }
}
