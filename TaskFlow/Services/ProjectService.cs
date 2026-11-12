using Microsoft.EntityFrameworkCore;
using TaskFlow.Data;
using TaskFlow.Models;
using TaskFlow.Models.Dtos;

namespace TaskFlow.Services;

/// <summary>
/// 项目服务实现类
/// 所有数据库查询写在这里，ViewModel不直接EF和DbContext
/// </summary>
public class ProjectService : IProjectService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ProjectService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<Project>> GetListAsync(ProjectFilter filter, CancellationToken cancellationToken = default)
    {
        // 每次操作独立创建DbContext，await using保证结束时释放
        await using var db =  await _factory.CreateDbContextAsync(cancellationToken);

        var query = db.Projects
            .Include(p => p.Tasks) // 预加载每个项目关联的任务
            .AsNoTracking()        // 只读列表，不跟踪实体变化
            .AsQueryable();


        // 搜索: 名称或者描述包含关键字（模糊匹配）
        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var keyword = filter.SearchText.Trim();
            query = query.Where(p => p.Name.Contains(keyword)
                || (p.Description != null && p.Description.Contains(keyword)));
        }

        // 筛选：有任务/无任务的项目
        if (filter.HasTasks.HasValue)
            query = query.Where(p => p.Tasks.Any() == filter.HasTasks.Value);

        // 按项目创建时间倒序排序
        query = query.OrderByDescending(p => p.CreatedAt);

        // 执行SQL查询 cancellationToken
        return await query.ToListAsync(cancellationToken);
    }
}