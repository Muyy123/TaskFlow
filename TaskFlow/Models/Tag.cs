namespace TaskFlow.Models;

/// <summary>
/// 标签实体，与任务是多对多关系。纯 POCO。
/// </summary>
public class Tag
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>标签颜色（十六进制），仅用于 UI 展示。</summary>
    public string Color { get; set; } = "#8E8E93";

    /// <summary>导航属性：使用该标签的任务。</summary>
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
