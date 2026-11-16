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
    private readonly IDialogService _dialogService;  // 用来弹确认框
    private CancellationTokenSource? _loadCts; // 用于取消上一次未完成的加载

    // 构造函数：DI自动把IProjectService注入进来
    public ProjectsViewModel(IProjectService projectService, IDialogService dialogService)
    {
        _projectService = projectService;
        _dialogService = dialogService;

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

    // 表格当前选中的项目，双向绑定到 DataGrid 的 SelectedItem
    // NotifyCanExecuteChangedFor：选中变化时，通知 DeleteProjectCommand（删除按钮）重新判断能否点击
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteProjectCommand))]
    [NotifyCanExecuteChangedFor(nameof(EditProjectCommand))] //选中变化也通知编辑按钮
    private Project? _selectedProject;
    
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

    /// <summary>
    /// 删除当前选中的项目
    /// CanExecute = nameof(CanDeleteProject)：没选中项目时，按钮自动置灰、不可点击
    /// 自动生成DeleteProjectCommand
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeleteProject))]
    private async Task DeleteProjectAsync()
    {
        if (SelectedProject is null)
            return;

        var project = SelectedProject; // 先存个引用，防止后续选中状态变化

        // 弹确认框
        var message = $"确认要删除项目{project.Name}吗？\n该项目下的任务不会被删除。";
        if (!_dialogService.Confirm(message, "删除项目"))
            return; // 用户点了“否”，直接取消，什么都不做

        var success = await _projectService.DeleteAsync(project.Id);
        if (!success)
            return; // 项目已不存在，直接返回

        // 删除成功：清空选中+刷新列表
        SelectedProject = null;
        await LoadProjectsAsync();
    }
    /// <summary>
    /// 删除按钮只有再选中了项目时才可点击
    /// </summary>
    /// <returns></returns>
    private bool CanDeleteProject() => SelectedProject is not null;

    /// <summary>
    /// 点“新建项目”：打开对话框 -> 用户确定后保存到数据库 -> 刷新列表
    /// </summary>
    [RelayCommand]
    private async Task AddProjectAsync()
    {
        // ①打开新建项目对话框（传null表示新建）
        var project = _dialogService.ShowProjectDialog();
        if (project is null)
            return; // 用户点了取消，什么都不做

        // ②调用服务层方法，把用户填好的数据保存到数据库
        await _projectService.CreateAsync(project);

        // ③重新加载列表，让新项目立即显示出来
        await LoadProjectsAsync();
    }

    /// <summary>
    /// 点"编辑"：打开对话框（预填当前选中项目）→ 确定后更新数据库 → 刷新列表
    /// CanExecute = nameof(CanEditProject)：没选中项目时按钮自动置灰
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanEditProject))]
    private async Task EditProjectAsync()
    {
        if (SelectedProject is null)
            return;

        var project = SelectedProject;

        // 弹出编辑弹窗，把选中的项目传进去，弹窗自动预填原有信息
        var result = _dialogService.ShowProjectDialog(project);
        if (result is null) return; //用户取消

        await _projectService.UpdateAsync(result);//保存修改到数据库

        await LoadProjectsAsync();//刷新列表
    }
    /// <summary>编辑按钮只有在选中了项目时才可点击。</summary>
    private bool CanEditProject() => SelectedProject is not null;
}
