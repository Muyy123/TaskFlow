using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TaskFlow.Models;

namespace TaskFlow.ViewModels.Dialogs;

/// <summary>
/// 项目编辑对话框的 ViewModel（新建/编辑通用）。
/// 只负责：收集输入、校验、点击确定时构造结果；不直接引用窗口。
/// 关闭窗口通过构造函数注入的Action回调完成。
/// </summary>
public partial class ProjectDialogViewModel : ObservableObject
{
    private readonly Action<bool> _close; //窗口传入的关闭回调委托：true:确认保存, false=取消

    private readonly Project? _existing;

    /// <summary>
    /// 预设颜色列表，展示成一行可点的色块
    /// </summary>
    public IReadOnlyList<string> PresetColors { get; } = new[]
    {
        "#4A90D9", // 蓝
        "#2ECC71", // 绿
        "#E74C3C", // 红
        "#F39C12", // 橙
        "#9B59B6", // 紫
        "#1ABC9C", // 青
        "#34495E", // 深蓝灰
        "#95A5A6", // 灰
    };

    public ProjectDialogViewModel(Action<bool> close, Project? existing = null)
    {
        _close = close;//窗口传入的关闭回调，用来通知窗口关闭弹窗
        _existing = existing;

        // 编辑模式：用原项目的数据预填输入框
        if (existing is not null)
        {
            Name = existing.Name;
            Description = existing.Description ?? string.Empty;//描述可能为null，转成空串给输入框
            Color = existing.Color;
        }
    }

    // 项目名称。NotifyCanExecuteChangedFor：名称变化时，自动触发SaveCommand的可用性检验（控制按钮灰不灰）
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _name = string.Empty;

    // 项目描述（可空）
    [ObservableProperty]
    private string _description = string.Empty;

    // 项目颜色，默认选中第一个预设色；双向绑定到色块列表的SelectedItem
    [ObservableProperty]
    private string _color = "#4A90D9";

    // 对话框结果：点“确定”且校验通过后才会有值
    public Project? Result { get; private set; }

    /// <summary>
    /// 保存命令（确定按钮）
    /// CanExecute绑定CanSave方法，控制按钮是否可用
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        Result = new Project
        {
            Id = _existing?.Id ?? 0, // 编辑时保留原Id；新建时为0（Id交给数据库生成）
            Name = Name.Trim(),  // 去掉首尾空格
            Description = string.IsNullOrWhiteSpace(Description)  // 描述为空就存null
                    ? null : Description.Trim(),
            Color = Color,
        };

        _close(true); // 关闭窗口，告诉调用方“用户点了确认”
    }

    /// <summary>
    /// 保存可用性判断逻辑
    /// 规则：名称不能是空、不能全是空格
    /// 返回true → 确定按钮可用；false → 按钮置灰不可点
    /// </summary>
    private bool CanSave() => !string.IsNullOrWhiteSpace(Name);

    /// <summary>
    /// 点“取消”；关闭窗口，不反悔结果
    /// </summary>
    [RelayCommand]
    private void Cancel() => _close(false);
}
