using TaskFlow.Models;
using TaskFlow.Models.Enums;

namespace TaskFlow.Data;

/// <summary>
/// 开发期示例数据：数据库为空时写入几条演示任务，方便直观看到界面效果。
/// 注意：只会在“任务表为空”时写入一次；数据被删后不会自动补齐
///       想重新生成演示数据，删除数据库文件taskflow.db再运行
/// 正式环境可移除，或替换为真实的初始化数据。
/// </summary>
public static class DbSeeder
{
    public static void SeedIfEmpty(AppDbContext db)
    {
        if (db.Tasks.Any())
            return; // 已有数据则跳过，避免重复插入

        // 1.项目
        // 4 个项目：3 个有任务，1 个空项目（用来演示"仅显示有任务"筛选）
        var projectDemo = new Project { Name = "示例项目", Description = "用于演示核心功能", Color = "#4A90D9" };
        var projectUi = new Project { Name = "界面与体验", Description = "主题、图表等界面相关", Color = "#2ECC71" };
        var projectData = new Project { Name = "数据功能", Description = "导入导出等数据相关", Color = "#F39C12" };
        var projectFuture = new Project { Name = "未来规划", Description = "暂未分配任务的规划事项", Color = "#9B59B6" }; // 空项目：不挂任何任务

        db.Projects.AddRange(projectDemo, projectUi, projectData, projectFuture);

        // 2.标签
        var importantTag = new Tag { Name = "重要", Color = "#E74C3C" };
        db.Tags.Add(importantTag);

        // 3.任务
        // 通过Project=xxx把任务挂到对应项目下，EF Core会自动填ProjectId外键
        db.Tasks.AddRange(
            // 示例项目下的任务
            new TaskItem
            {
                Title = "完成 TaskFlow 项目骨架",
                Status = TaskItemStatus.Done,
                Priority = Priority.High,
                Project = projectDemo,
                Tags = { importantTag },
            },
            new TaskItem
            {
                Title = "实现数据层（EF Core + SQLite）",
                Status = TaskItemStatus.Done,
                Priority = Priority.High,
                Project = projectDemo,
            },
            new TaskItem
            {
                Title = "实现任务列表页",
                Status = TaskItemStatus.InProgress,
                Priority = Priority.Medium,
                Project = projectDemo,
                Tags = { importantTag },
            },

            // 界面与体验
            new TaskItem
            {
                Title = "实现仪表盘统计图表",
                Status = TaskItemStatus.Todo,
                Priority = Priority.Low,
                DueDate = DateTime.UtcNow.AddDays(3),
                Project = projectUi,
            },
            new TaskItem
            {
                Title = "实现浅色/深色主题切换",
                Status = TaskItemStatus.Todo,
                Priority = Priority.Medium,
                DueDate = DateTime.UtcNow.AddDays(7),
                Project = projectUi,
            },

            // 数据功能项目
            new TaskItem
            {
                Title = "实现 CSV 导入导出",
                Status = TaskItemStatus.Todo,
                Priority = Priority.Low,
                DueDate = DateTime.UtcNow.AddDays(14),
                Project = projectData,
            }
        );

        db.SaveChanges();
    }
}
