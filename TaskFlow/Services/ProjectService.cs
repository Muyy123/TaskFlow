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

    /// <summary>
    /// 根据任务id删除项目
    /// </summary>
    /// <param name="id">待删除任务的主键ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>删除成功返回true；任务不存在返回false</returns>
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        // 每次操作独立创建 DbContext，await using 保证结束时释放
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        // 根据id查到项目
        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        // 查不到说明已被删除
        if (project is null) 
            return false;
        // 查到了就删除
        db.Projects.Remove(project);
        // SaveChanges 时，EF Core 会按 AppDbContext 里配好的删除行为（SetNull）
        // 所以该项目的任务会保留，只是变成"未分类"。
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default)
    {
        // 每次操作独立创建DbContext
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);

        project.CreatedAt = DateTime.UtcNow; //记录创建时间
        db.Projects.Add(project); //告诉EF Core这个对象是新的，等下保存的时候要执行INSERT
        await db.SaveChangesAsync(cancellationToken); //真正写入数据库
        
        return project; // 返回时Id已被数据库生成并回填
    }
}