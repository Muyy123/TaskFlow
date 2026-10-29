using System.Windows;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskFlow.Data;
using TaskFlow.Services;
using TaskFlow.ViewModels;
using TaskFlow.ViewModels.Pages;

namespace TaskFlow;

/// <summary>
/// 应用入口：使用泛型 Host（Generic Host）构建依赖注入容器，负责启动主窗口，
/// 并在显示窗口前初始化数据库（建库 + 示例数据）。
/// </summary>
public partial class App : Application
{
    /// <summary>全局 DI 容器宿主，供全局静态访问（如获取服务）。</summary>
    public static IHost AppHost { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices(ConfigureServices)
            .Build();

        AppHost.StartAsync().GetAwaiter().GetResult();

        // 必须在解析任何依赖 DbContext 的对象之前完成建库与种子数据
        InitializeDatabase();

        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // ── Data ──
        // 桌面应用推荐用 DbContextFactory：每个操作创建短生命周期 DbContext，
        // 避免单例长驻导致实体跟踪堆积、数据陈旧。
        var dbPath = Path.Combine(AppContext.BaseDirectory, "taskflow.db");
        services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));

        // ── Services ──
        services.AddSingleton<ITaskService, TaskService>();

        // ── ViewModels ──
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<ProjectsViewModel>();
        services.AddSingleton<TaskListViewModel>();
        services.AddSingleton<SettingsViewModel>();

        // ── Views ──
        services.AddSingleton<MainWindow>();
    }

    /// <summary>建库并写入示例数据。</summary>
    private static void InitializeDatabase()
    {
        var factory = AppHost.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
        using var db = factory.CreateDbContext();

        // 开发期快捷建库：按当前模型直接创建表。
        // 正式做 schema 版本管理时，改用 db.Database.Migrate() + dotnet ef migrations。
        db.Database.EnsureCreated();

        DbSeeder.SeedIfEmpty(db);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        AppHost.StopAsync().GetAwaiter().GetResult();
        AppHost.Dispose();
        base.OnExit(e);
    }
}
