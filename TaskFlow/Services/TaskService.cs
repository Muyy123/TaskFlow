using Microsoft.EntityFrameworkCore;
using TaskFlow.Data;
using TaskFlow.Models;
using TaskFlow.Models.Dtos;

namespace TaskFlow.Services;

/// <summary>
/// 任务服务实现。
/// 通过 IDbContextFactory 在每个操作内创建短生命周期 DbContext，避免桌面应用里
/// 长驻上下文导致的实体跟踪堆积与数据陈旧。不引用任何 WPF 命名空间。
/// </summary>
public class TaskService : ITaskService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public TaskService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        return await db.Tasks
            .Include(t => t.Project)
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<PagedResult<TaskItem>> GetPagedAsync(TaskFilter filter, CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);

        var query = db.Tasks
            .Include(t => t.Project)
            .Include(t => t.Tags)
            .AsNoTracking()
            .AsQueryable();

        // 搜索：标题或描述包含关键字
        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var keyword = filter.SearchText.Trim();
            query = query.Where(t => t.Title.Contains(keyword)
                                     || (t.Description != null && t.Description.Contains(keyword)));
        }

        if (filter.Status.HasValue)
            query = query.Where(t => t.Status == filter.Status.Value);

        if (filter.Priority.HasValue)
            query = query.Where(t => t.Priority == filter.Priority.Value);

        if (filter.ProjectId.HasValue)
            query = query.Where(t => t.ProjectId == filter.ProjectId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        // 默认按创建时间倒序；如需多字段排序，扩展 TaskFilter 后在此加 switch
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((filter.PageIndex - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TaskItem>
        {
            Items = items,
            TotalCount = totalCount,
            PageIndex = filter.PageIndex,
            PageSize = filter.PageSize
        };
    }

    public async Task<TaskItem> CreateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        task.CreatedAt = DateTime.UtcNow;
        db.Tasks.Add(task);
        await db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<TaskItem> UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        db.Tasks.Update(task);
        await db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (task is null)
            return false;

        db.Tasks.Remove(task);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
