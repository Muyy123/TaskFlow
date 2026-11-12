using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TaskFlow.Models;
using TaskFlow.Models.Dtos;
using TaskFlow.Services;

namespace TaskFlow.ViewModels.Pages;

/// <summary>
/// 项目页 ViewModel（加载项目 / 搜索 / 筛选）。
/// 结构和TaskListViewModel一致，但去掉了分页（项目数量少，一次全加载）
/// </summary>
public partial class ProjectsViewModel : ViewModelBase
{
    private readonly IProjectService _projectService; // 用来调用数据库查询
    private CancellationTokenSource? _loadCts; // 用于取消上一次未完成的加载

    // 构造函数：DI自动把IProjectService注入进来
    public ProjectsViewModel(IProjectService projectService)
    {
        _projectService = projectService;

        // 页面初始化，自动加载一次项目
        // _代表“发射后不管” 不等待这个task完成，不阻塞UI
        _ = LoadProjectsAsync();
    }

    // 项目列表，绑定到DataGrid的ItemSource
    [ObservableProperty]
    private ObservableCollection<Project> _projects = new();

    // 搜索框文本，双向绑定到搜索框
    [ObservableProperty]
    private string _searchText = string.Empty;

    // 是否勾选“仅显示有任务的项目”
    [ObservableProperty]
    private bool _hasTasks;
    // 是否正在加载，绑定到进度条
    [ObservableProperty]
    private bool _isLoading;
    // 列表是否为空，“暂无项目”空状态提示
    [ObservableProperty]
    private bool _isEmpty;
    
    /// <summary>
    /// 加载项目列表
    /// 避免快速搜索/连续勾选时，旧请求的结果覆盖新请求
    /// </summary>
    /// <param name="cancellationToken">取消上一次未完成的加载</param>
    /// <returns></returns>
    public async Task LoadProjectsAsync(CancellationToken cancellationToken = default)
    {
        _loadCts?.Cancel(); // ①取消上一次加载
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _loadCts = cts; // ②记录本次的令牌，下次加载时用来取消本次
        
        IsLoading = true;
        try
        {
            // 把界面上的搜索词、筛选状态打包成查询条件传给Service
            var filter = new ProjectFilter
            {
                SearchText = SearchText,
                HasTasks = HasTasks ? true : null, // 没勾选 = 不筛选（传null）
            };
            // 调用服务查询数据库
            var projects = await _projectService.GetListAsync(filter, cts.Token);

            // 用查回来的数据刷新列表
            Projects.Clear();
            foreach (var p in projects)
                Projects.Add(p);

            IsEmpty = Projects.Count == 0; // 更新空状态

        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            // 主动取消属于正常情况，静默捕获，不报错弹窗
        }
        finally
        {
            // 只有当前保存的_loadCts和本次cts是同一个，才关闭加载
            if (_loadCts == cts)
                IsLoading = false;
        }
    }
    /// <summary>
    /// 点击“搜索”按钮; 按当前搜索词重新加载
    /// </summary>
    [RelayCommand]
    private async Task SearchAsync() => await LoadProjectsAsync();

    /// <summary>
    /// HasTasks 这个属性变化时自动执行
    /// CommunityToolkit源生成器约定：On{属性名}Changed
    /// 只要下拉框切换选项，值一变，立刻重新查询
    /// </summary>
    partial void OnHasTasksChanged(bool value) => _ = LoadProjectsAsync();
}
