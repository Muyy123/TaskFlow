using TaskFlow.Models;
using TaskFlow.Models.Enums;

namespace TaskFlow.Data;

/// <summary>
/// 开发期示例数据：数据库为空时写入几条演示任务，方便直观看到界面效果。
/// 正式环境可移除，或替换为真实的初始化数据。
/// </summary>
public static class DbSeeder
{
    public static void SeedIfEmpty(AppDbContext db)
    {
        if (db.Tasks.Any())
            return; // 已有数据则跳过，避免重复插入

        var project = new Project { Name = "示例项目", Description = "用于演示的项目", Color = "#4A90D9" };
        var importantTag = new Tag { Name = "重要", Color = "#E74C3C" };

        db.Projects.Add(project);
        db.Tags.Add(importantTag);

        db.Tasks.AddRange(
            new TaskItem
            {
                Title = "完成 TaskFlow 项目骨架",
                Status = TaskItemStatus.Done,
                Priority = Priority.High,
                Project = project,
                Tags = { importantTag },
            },
            new TaskItem
            {
                Title = "实现数据层（EF Core + SQLite）",
                Status = TaskItemStatus.Done,
                Priority = Priority.High,
                Project = project,
            },
            new TaskItem
            {
                Title = "实现任务列表页",
                Status = TaskItemStatus.InProgress,
                Priority = Priority.Medium,
                Project = project,
                Tags = { importantTag },
            },
            new TaskItem
            {
                Title = "实现仪表盘统计图表",
                Status = TaskItemStatus.Todo,
                Priority = Priority.Low,
                DueDate = DateTime.UtcNow.AddDays(3),
            },
            new TaskItem
            {
                Title = "实现浅色/深色主题切换",
                Status = TaskItemStatus.Todo,
                Priority = Priority.Medium,
                DueDate = DateTime.UtcNow.AddDays(7),
            },
            new TaskItem
            {
                Title = "实现 CSV 导入导出",
                Status = TaskItemStatus.Todo,
                Priority = Priority.Low,
                DueDate = DateTime.UtcNow.AddDays(14),
            }
        );

        db.SaveChanges();
    }
}
