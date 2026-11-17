using System.Globalization;
using System.Windows.Data;
using TaskFlow.Models.Enums;

namespace TaskFlow.Converters;

/// <summary>
/// 枚举 → 中文 显示转换器。
/// 任务状态、优先级在界面上显示中文标签，而不是枚举名（例如 Todo → "待办"）。
/// 用一个 switch 同时处理 TaskItemStatus 和 Priority 两种枚举。
/// </summary>
public class EnumToChineseConverter : IValueConverter
{
    /// <summary>
    /// 把枚举值转成中文标签。
    /// value：绑定的源值（这里是 TaskItemStatus 或 Priority 枚举）
    /// </summary>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            // 任务状态
            TaskItemStatus.Todo => "待办",
            TaskItemStatus.InProgress => "进行中",
            TaskItemStatus.Done => "已完成",
            // 优先级
            Priority.Low => "低",
            Priority.Medium => "中",
            Priority.High => "高",
            Priority.Urgent => "紧急",

            // 兜底分支：空或者未定义枚举，返回原始字符串，防止界面报错崩溃
            _ => value?.ToString() ?? string.Empty,
        };
    }

    /// <summary>
    /// 反向转换。本转换器只用于单向显示（枚举 → 中文），不需要反向，
    /// 返回 Binding.DoNothing 表示"什么都不做"。
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}
