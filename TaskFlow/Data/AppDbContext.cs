using Microsoft.EntityFrameworkCore;
using TaskFlow.Models;
using TaskFlow.Models.Enums;

namespace TaskFlow.Data;

/// <summary>
/// 应用数据库上下文（EF Core + SQLite）。
/// 通过构造函数注入 DbContextOptions，运行时连接字符串由 DI 容器配置；
/// 生成迁移时由 <see cref="AppDbContextFactory"/> 提供设计时实例。
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // 枚举统一以字符串存储：数据库值可读，且后续增删枚举值不影响已有数据
        configurationBuilder.Properties<TaskItemStatus>().HaveConversion<string>();
        configurationBuilder.Properties<Priority>().HaveConversion<string>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Project>(entity =>
        {
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Description).HasMaxLength(500);
            entity.Property(p => p.Color).HasMaxLength(20);
            entity.HasIndex(p => p.Name);
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Description).HasMaxLength(1000);

            // 常用筛选/查询字段建立索引
            entity.HasIndex(t => t.Status);
            entity.HasIndex(t => t.Priority);
            entity.HasIndex(t => t.DueDate);
            entity.HasIndex(t => t.ProjectId);

            // 项目 → 任务：一对多；删除项目时任务保留（ProjectId 置空）
            entity.HasOne(t => t.Project)
                  .WithMany(p => p.Tasks)
                  .HasForeignKey(t => t.ProjectId)
                  .OnDelete(DeleteBehavior.SetNull);

            // 任务 ↔ 标签：多对多（EF Core 自动生成中间表，显式命名为 TaskTags）
            entity.HasMany(t => t.Tags)
                  .WithMany(tag => tag.Tasks)
                  .UsingEntity(j => j.ToTable("TaskTags"));
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.Property(t => t.Name).IsRequired().HasMaxLength(50);
            entity.Property(t => t.Color).HasMaxLength(20);
            entity.HasIndex(t => t.Name).IsUnique();
        });
    }
}
