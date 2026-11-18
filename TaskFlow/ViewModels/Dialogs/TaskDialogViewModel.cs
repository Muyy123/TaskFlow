using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TaskFlow.Models;
using TaskFlow.Models.Dtos;
using TaskFlow.Models.Enums;
using TaskFlow.Services;

namespace TaskFlow.ViewModels.Dialogs;

/// <summary>
/// 任务编辑对话框的 ViewModel（新建 / 编辑通用）。
/// 比项目对话框多：状态下拉、优先级下拉、截止日期、所属项目下拉（要加载项目列表）。
/// </summary>
public partial class TaskDialogViewModel : ObservableObject
{
    private readonly IProjectService _projectService;// 用来加载"所属项目"下拉列表
    private readonly Action<bool> _close;// 关闭回调：true=确定，false=取消
    private readonly TaskItem? _existing; // 新建时为 null；编辑时存原始任务数据

    /// <summary>
    /// 项目下拉框选项记录：界面显示名称 + 项目Id
    /// ProjectId 为 null 代表【无项目】，任务不归属任何项目
    /// </summary>
    public record ProjectOption(string DisplayName, int? ProjectId);
    /// <summary>状态下拉选项集合，直接读取枚举全部值</summary>
    public IReadOnlyList<TaskItemStatus> StatusOptions { get; } = Enum.GetValues<TaskItemStatus>();
    /// <summary>优先级下拉选项集合，直接读取枚举全部值</summary>
    public IReadOnlyList<Priority> PriorityOptions { get; } = Enum.GetValues<Priority>();
    /// <summary>所属项目下拉数据源，ObservableCollection 改动后界面自动刷新</summary>
    public ObservableCollection<ProjectOption> ProjectOptions { get; } = new();

    public TaskDialogViewModel(IProjectService projectService, Action<bool> close, TaskItem? existing = null)
    {
        _projectService = projectService;
        _close = close;
        _existing = existing;

        // 如果是编辑模式：用原有任务预填所有表单字段
        if (existing is not null)
        {
            Title = existing.Title;
            Description = existing.Description ?? string.Empty;
            Status = existing.Status;
            Priority = existing.Priority;
            DueDate = existing.DueDate;
            SelectedProjectId = existing.ProjectId;
        }

        // 异步加载项目列表，不阻塞弹窗打开（fire-and-forget）
        _ = LoadProjectAsync();
    }

    // 标题（必填）。NotifyCanExecuteChangedFor：标题变化时让"确定"按钮重新判断能不能点
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _title = string.Empty;

    // 描述（可空）
    [ObservableProperty]
    private string _description = string.Empty;

    // 当前选中的状态，默认"待办"
    [ObservableProperty]
    private TaskItemStatus _status = TaskItemStatus.Todo;

    // 当前选中的优先级，默认"中"
    [ObservableProperty]
    private Priority _priority = Priority.Medium;

    // 截止日期（可空，表示无截止时间）
    [ObservableProperty]
    private DateTime? _dueDate;

    // 所属项目 Id（null 表示"无项目"）
    [ObservableProperty]
    private int? _selectedProjectId;

    /// <summary>最终返回给外部的任务数据，只有点击确定且校验通过才会赋值</summary>
    public TaskItem? Result { get; private set; }

    /// <summary>加载项目列表，填充"所属项目"下拉框。</summary>
    private async Task LoadProjectAsync()
    {
        // new ProjectFilter() 是空条件 = 查出所有项目
        var projects = await _projectService.GetListAsync(new ProjectFilter());

        ProjectOptions.Clear();
        ProjectOptions.Add(new ProjectOption("无项目", null));// 第一项：不挂任何项目
        foreach (var p in projects.OrderBy(p => p.Name))   // 按项目名排序，方便查找
            ProjectOptions.Add(new ProjectOption(p.Name, p.Id));
    }

    /// <summary>
    /// 保存命令：点击确定按钮
    /// CanExecute绑定CanSave，标题为空时按钮自动灰掉不可点击
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        // 完成时间联动：状态是"已完成"才填完成时间，否则清空
        DateTime? completedAt = _existing?.CompletedAt; // 编辑时先保留原值
        if (Status == TaskItemStatus.Done)
            completedAt ??= DateTime.UtcNow;            // 变成"已完成"且原本没时间 → 填当前时间
        else
            completedAt = null;                         // 非"已完成" → 清空

        // 组装任务实体
        Result = new TaskItem
        {
            Id = _existing?.Id ?? 0,// 编辑沿用原有Id；新建传0，EF自动生成主键
            Title = Title.Trim(),
            Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
            Status = Status,
            Priority = Priority,
            DueDate = DueDate,
            CompletedAt = completedAt,
            ProjectId = SelectedProjectId,
        };
        // 回调通知窗口关闭，标记为确认
        _close(true);
    }
    /// <summary>保存校验规则：标题不能是空或者全空格</summary>
    private bool CanSave() => !string.IsNullOrWhiteSpace(Title);

    /// <summary>取消命令：关闭弹窗，不生成Result</summary>
    [RelayCommand]
    private void Cancel() => _close(false);
}
