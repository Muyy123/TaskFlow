using TaskFlow.Models;
using TaskFlow.Models.Dtos;

namespace TaskFlow.Services;
/// <summary>
/// 项目服务接口
/// 本轮只实现列表查询方法（搜索+筛选）；后续再追加 Create/Update/Delete
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// filter 查询条件
    /// cancellationToken 取消令牌，取消上一次加载请求，防止旧请求覆盖新数据
    /// </summary>
    Task<IReadOnlyList<Project>> GetListAsync(ProjectFilter filter, CancellationToken cancellationToken = default);
}
