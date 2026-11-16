using TaskFlow.Models;
using TaskFlow.Models.Dtos;

namespace TaskFlow.Services;
/// <summary>
/// 项目服务接口
/// 本轮只实现列表查询方法（搜索+筛选）和删除；后续再追加 Create/Update/Delete
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// 查询项目列表
    /// </summary>
    /// <param name="filter">查询条件</param>
    /// <param name="cancellationToken">取消令牌，取消上一次加载请求，防止旧请求覆盖新数据</param>
    /// <returns></returns>
    Task<IReadOnlyList<Project>> GetListAsync(ProjectFilter filter, CancellationToken cancellationToken = default);
    /// <summary>
    /// 删除指定项目
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// 新建项目
    /// </summary>
    /// <returns>保存后的项目（此时Id、CreatedAt已被填充）</returns>
    Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default);
}
