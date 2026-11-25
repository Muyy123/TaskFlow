using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Data;
using TaskFlow.Models;
using TaskFlow.Models.Dtos;
using TaskFlow.Models.Enums;

namespace TaskFlow.Services;

/// <summary>
/// 任务服务实现。
/// 通过 IDbContextFactory 在每个操作内创建短生命周期 DbContext，避免桌面应用里
/// 长驻上下文导致的实体跟踪堆积与数据陈旧。不引用任何 WPF 命名空间。
/// </summary>
public class TaskService : ITaskService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly ILogger<TaskService> _logger; //结构化日志

    public TaskService(IDbContextFactory<AppDbContext> factory, ILogger<TaskService> logger)
    {
        _factory = factory;
        _logger = logger;

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

        _logger.LogInformation("创建任务：{Title}（Id={Id}）", task.Title, task.Id);
        return task;
    }

    public async Task<TaskItem> UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);

        // 先从数据库取出原纪录（而不是直接db.Update整个实体）
        var existing = await db.Tasks.FirstOrDefaultAsync(t => t.Id == task.Id, cancellationToken);
        if (existing is null)
            throw new InvalidOperationException($"要编辑的任务(Id={task.Id})不存在");

        // 只更新允许用户编辑的字段
        // CreatedAt 是创建时间，不允许修改；导航属性Tags、Project也不在这里覆盖
        existing.Title = task.Title;
        existing.Description = task.Description;
        existing.Status = task.Status;
        existing.Priority = task.Priority;
        existing.DueDate = task.DueDate;
        // CompletedAt 的联动逻辑已经在 TaskDialogViewModel 里处理好了，这里直接赋值结果
        existing.CompletedAt = task.CompletedAt;
        existing.ProjectId = task.ProjectId;

        // 提交改动到数据库
        await db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("更新任务：Id={Id}", task.Id);
        // 返回数据库里最新的这条任务
        return existing;
    }

    /// <summary>
    /// 根据任务id删除任务
    /// </summary>
    /// <param name="id">待删除任务的主键ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>删除成功返回true；任务不存在返回false</returns>
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        // 每次操作独立创建 DbContext，await using 保证结束时释放
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);//根据id查询对应的任务记录
        if (task is null)//没找到，直接返回false
            return false;

        db.Tasks.Remove(task);//找到了 就删除
        await db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("删除任务：Id={Id}", id);
        return true;
    }

    /// <summary>
    /// 聚合统计任务数量，供仪表盘展示。
    /// 用多个 CountAsync 分别统计（SQLite 本地查询很快，可读性优先，不做复杂 GroupBy）。
    /// </summary>
    public async Task<DashboardStats> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);

        var now = DateTime.UtcNow;

        // 分别统计三种状态的任务数量，每条CountAsync会生成一条SQL count语句
        var todoCount = await db.Tasks.CountAsync(t => t.Status == TaskItemStatus.Todo, cancellationToken);
        var inProgressCount = await db.Tasks.CountAsync(t => t.Status == TaskItemStatus.InProgress, cancellationToken);
        var doneCount = await db.Tasks.CountAsync(t => t.Status == TaskItemStatus.Done, cancellationToken);

        // 逾期：有截止日期、已经过了、且还没完成
        var overdueCount = await db.Tasks.CountAsync(
            t => t.DueDate != null && t.DueDate < now && t.Status != TaskItemStatus.Done, cancellationToken);

        // 汇总数据返回DTO
        return new DashboardStats
        {
            TotalCount = todoCount + inProgressCount + doneCount,
            TodoCount = todoCount,
            InProgressCount = inProgressCount,
            DoneCount = doneCount,
            OverdueCount = overdueCount,
        };
    }
}
