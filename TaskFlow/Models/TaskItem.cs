using TaskFlow.Models.Enums;

namespace TaskFlow.Models;

/// <summary>
/// 任务实体。纯 POCO。
/// 注意：枚举类型命名为 TaskItemStatus 而非 TaskStatus，
/// 是为了避免与 System.Threading.Tasks.TaskStatus 命名冲突。
/// </summary>
public class TaskItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;

    public Priority Priority { get; set; } = Priority.Medium;

    /// <summary>截止日期（可空，表示无截止时间）。</summary>
    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>完成时间（状态变为 Done 时填写，可空）。</summary>
    public DateTime? CompletedAt { get; set; }

    // ── 外键：所属项目（可空，任务可不隶属于任何项目） ──
    public int? ProjectId { get; set; }
    public Project? Project { get; set; }

    // ── 多对多：标签 ──
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
