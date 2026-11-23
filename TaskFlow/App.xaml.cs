using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkiaSharp;
using System.IO;
using System.Windows;
using TaskFlow.Data;
using TaskFlow.Services;
using TaskFlow.Theme;
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

        // 全局初始化 LiveCharts2，整个程序生命周期只执行一次
        // AddSkiaSharp()：使用SkiaSharp作为图表渲染后端
        // AddDefaultMappers()：启用内置默认数据映射，直接支持数字、坐标点等基础类型
        // AddLightTheme()：初始使用浅色主题；深色模式后续再做适配切换
        LiveCharts.Configure(config => config
            .AddSkiaSharp()
            .AddDefaultMappers()
            .AddLightTheme()
            // 重点：注册中文，Skia渲染中文就会匹配系统雅黑字体
            .HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('汉')));

        // 构建宿主
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices(ConfigureServices)
            .Build();

        AppHost.StartAsync().GetAwaiter().GetResult();

        // 初始化 默认浅色主题
        var theme = AppHost.Services.GetRequiredService<ThemeService>();
        theme.ApplyTheme(dark: false);

        // 必须在解析任何依赖 DbContext 的对象之前完成建库与种子数据
        InitializeDatabase();
        //打开主窗口
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
        services.AddSingleton<IProjectService, ProjectService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<ThemeService>();

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

        // 应用所有未执行的迁移，把数据库升级到最新 schema（生产级做法）
        db.Database.Migrate();

        DbSeeder.SeedIfEmpty(db);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        AppHost.StopAsync().GetAwaiter().GetResult();
        AppHost.Dispose();
        base.OnExit(e);
    }
}
