using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskFlow.Data;

/// <summary>
/// 设计时工厂：让 dotnet ef 命令行工具在生成迁移时无需启动应用即可创建 DbContext。
/// 注意：这里的连接字符串仅供命令行工具使用；运行时真正的连接字符串将在 DI 中配置
/// （通常指向用户数据目录，而非当前工作目录）。
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=taskflow.db")
            .Options;

        return new AppDbContext(options);
    }
}
