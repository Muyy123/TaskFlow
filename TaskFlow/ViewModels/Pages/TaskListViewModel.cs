using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TaskFlow.Models;
using TaskFlow.Models.Dtos;
using TaskFlow.Models.Enums;
using TaskFlow.Services;

namespace TaskFlow.ViewModels.Pages;

/// <summary>
/// 任务列表页 ViewModel：加载、搜索、分页、增删任务。
/// 使用 CommunityToolkit.Mvvm 的 [ObservableProperty] 与 [RelayCommand] 源生成器。
/// </summary>
public partial class TaskListViewModel : ViewModelBase
{
    private readonly ITaskService _taskService;
    private CancellationTokenSource? _loadCts;

    public TaskListViewModel(ITaskService taskService)
    {
        _taskService = taskService;

        // 首次进入即加载数据（fire-and-forget，取消相关异常已在 LoadTasksAsync 内处理）
        _ = LoadTasksAsync();
    }

    /// <summary>任务列表。</summary>
    [ObservableProperty]
    private ObservableCollection<TaskItem> _tasks = new();

    /// <summary>表格当前选中行。</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteTaskCommand))]
    private TaskItem? _selectedTask;

    /// <summary>搜索关键字。</summary>
    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>当前页码（从 1 开始）。</summary>
    [ObservableProperty]
    private int _currentPage = 1;

    /// <summary>总页数。</summary>
    [ObservableProperty]
    private int _totalPages = 1;

    /// <summary>每页条数。</summary>
    [ObservableProperty]
    private int _pageSize = 20;

    /// <summary>是否正在加载。</summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// 加载当前页任务。用 CancellationTokenSource 取消上一次未完成的加载，
    /// 避免快速搜索/翻页时旧请求的结果覆盖新请求。
    /// </summary>
    public async Task LoadTasksAsync(CancellationToken cancellationToken = default)
    {
        // 取消上一次加载；旧请求会抛 OperationCanceledException 并被下面捕获
        _loadCts?.Cancel();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _loadCts = cts;

        IsLoading = true;
        try
        {
            var filter = new TaskFilter
            {
                SearchText = SearchText,
                PageIndex = CurrentPage,
                PageSize = PageSize,
            };

            var result = await _taskService.GetPagedAsync(filter, cts.Token);

            Tasks.Clear();
            foreach (var task in result.Items)
                Tasks.Add(task);

            CurrentPage = result.PageIndex;
            TotalPages = result.TotalPages;
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            // 被更新的加载取消，静默忽略（新请求会负责填充数据）
        }
        finally
        {
            // 只有当本次加载仍是最新一次时，才清除加载状态
            if (_loadCts == cts)
                IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        CurrentPage = 1; // 搜索总是从第一页开始
        await LoadTasksAsync();
    }

    [RelayCommand(CanExecute = nameof(CanDeleteTask))]
    private async Task DeleteTaskAsync()
    {
        if (SelectedTask is null)
            return;

        var task = SelectedTask;
        await _taskService.DeleteAsync(task.Id);
        await LoadTasksAsync();
    }

    private bool CanDeleteTask() => SelectedTask is not null;

    [RelayCommand]
    private async Task AddTaskAsync()
    {
        // 暂以默认值创建示例任务；后续接入编辑对话框后改为打开对话框
        var task = new TaskItem
        {
            Title = $"新建任务 {DateTime.Now:MM-dd HH:mm:ss}",
            Status = TaskItemStatus.Todo,
            Priority = Priority.Medium,
        };

        await _taskService.CreateAsync(task);
        await LoadTasksAsync();
    }

    /// <summary>分页控件跳转到指定页。</summary>
    [RelayCommand]
    private async Task GoToPageAsync(int page)
    {
        // 越界或目标页即当前页时直接忽略
        if (page < 1 || page > TotalPages || page == CurrentPage)
            return;

        CurrentPage = page;
        await LoadTasksAsync();
    }
}
