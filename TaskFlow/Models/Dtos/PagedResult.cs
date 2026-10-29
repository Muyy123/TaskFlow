namespace TaskFlow.Models.Dtos;

/// <summary>
/// 分页查询结果（通用）。供 Service 层返回给 ViewModel 使用。
/// </summary>
public class PagedResult<T>
{
    /// <summary>当前页的数据。</summary>
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    /// <summary>满足条件的总记录数。</summary>
    public int TotalCount { get; set; }

    /// <summary>当前页码（从 1 开始）。</summary>
    public int PageIndex { get; set; }

    /// <summary>每页条数。</summary>
    public int PageSize { get; set; }

    /// <summary>总页数。</summary>
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => PageIndex > 1;

    public bool HasNextPage => PageIndex < TotalPages;
}
