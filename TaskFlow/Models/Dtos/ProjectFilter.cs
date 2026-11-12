namespace TaskFlow.Models.Dtos;
/// <summary>
/// 项目查询筛选条件DTO
/// 本轮只用到 SearchText 搜索和“仅显示有任务”筛选
/// </summary>
public class ProjectFilter
{
    /// <summary>搜索关键词：匹配项目名称、描述</summary>
    public string? SearchText { get; set; }

    /// <summary> 是否只显示有任务的项目
    /// bool? 不赋值就是null
    /// true 有任务的项目
    /// false 无任务的项目
    /// null 所有项目 </summary>
    public bool? HasTasks { get; set; }
}