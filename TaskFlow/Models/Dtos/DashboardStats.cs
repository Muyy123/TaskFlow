namespace TaskFlow.Models.Dtos;

/// <summary>
/// 仪表盘统计数据。由 TaskService 聚合查询得到，供仪表盘页展示。
/// </summary>
public class DashboardStats
{
    /// <summary>
    /// 任务总数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 待办任务数
    /// </summary>
    public int TodoCount { get; set; }

    /// <summary>
    /// 进行中任务数
    /// </summary>
    public int InProgressCount { get; set; }

    /// <summary>
    /// 已完成任务数
    /// </summary>
    public int DoneCount {  get; set; }

    /// <summary>
    /// 逾期任务数：截止日期已过且尚未完成
    /// </summary>
    public int OverdueCount {  get; set; }
}
