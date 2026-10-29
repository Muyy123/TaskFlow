namespace TaskFlow.Models;

/// <summary>
/// 项目实体。纯 POCO，不依赖 EF Core 或任何 UI 框架。
/// </summary>
public class Project
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>项目主题色（十六进制，如 #4A90D9），仅用于 UI 展示。</summary>
    public string Color { get; set; } = "#4A90D9";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>导航属性：项目下的所有任务。</summary>
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
