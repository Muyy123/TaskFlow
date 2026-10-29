using TaskFlow.Models.Enums;

namespace TaskFlow.Models.Dtos;

/// <summary>
/// 任务列表的查询 / 筛选条件。
/// 后续如需排序，可在此追加 SortBy / SortDescending 字段，并在 Service 里按字段排序。
/// </summary>
public class TaskFilter
{
    /// <summary>搜索关键字（匹配标题或描述）。</summary>
    public string? SearchText { get; set; }

    /// <summary>按状态筛选（可空表示不筛选）。</summary>
    public TaskItemStatus? Status { get; set; }

    /// <summary>按优先级筛选（可空表示不筛选）。</summary>
    public Priority? Priority { get; set; }

    /// <summary>按项目筛选（可空表示不筛选）。</summary>
    public int? ProjectId { get; set; }

    /// <summary>页码，从 1 开始。</summary>
    public int PageIndex { get; set; } = 1;

    /// <summary>每页条数。</summary>
    public int PageSize { get; set; } = 20;
}
