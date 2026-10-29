using TaskFlow.Models;
using TaskFlow.Models.Dtos;

namespace TaskFlow.Services;

/// <summary>
/// 任务服务契约。ViewModel 只依赖本接口，不依赖具体实现或 EF Core，
/// 便于单元测试时用 Mock 替换。
/// 所有异步方法都接受 CancellationToken，用于在发起新请求或关闭页面时取消进行中的查询。
/// </summary>
public interface ITaskService
{
    Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PagedResult<TaskItem>> GetPagedAsync(TaskFilter filter, CancellationToken cancellationToken = default);

    Task<TaskItem> CreateAsync(TaskItem task, CancellationToken cancellationToken = default);

    Task<TaskItem> UpdateAsync(TaskItem task, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
