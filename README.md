# TaskFlow 任务管理系统

一个基于 **.NET 8 + WPF** 的桌面任务管理应用，采用 **MVVM** 架构与 **Generic Host 依赖注入**，支持任务的项目化管理、搜索分页、数据统计等能力。

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=.net&logoColor=white)
![WPF](https://img.shields.io/badge/WPF-Windows-0078D4?logo=windows&logoColor=white)
![SQLite](https://img.shields.io/badge/EF_Core-SQLite-003B57?logo=sqlite&logoColor=white)
![License](https://img.shields.io/badge/license-MIT-green)

> 项目持续开发中，已完成功能与规划功能见下文「功能特性」。

## 项目简介

TaskFlow 是一个桌面端任务管理系统，用于集中管理项目与任务，跟踪任务状态、优先级、截止日期与标签，并通过统计仪表盘直观呈现任务分布。项目旨在实践 WPF 桌面开发中的现代工程实践：MVVM 解耦、依赖注入、异步编程与自定义控件。

## 功能特性

### 已实现
- ✅ 项目骨架：Generic Host 依赖注入、分层架构
- ✅ 数据层：`TaskItem` / `Project` / `Tag` 实体，EF Core + SQLite
- ✅ 任务服务：CRUD、分页、关键字搜索（`ITaskService` / `TaskService`）
- ✅ 任务列表页：搜索、分页、新增、删除选中任务
- ✅ 异步加载与取消：`CancellationTokenSource` 处理快速搜索/翻页的竞态
- ✅ 自定义分页控件 `PaginationControl`：依赖属性、`GoToPageCommand`、ControlTemplate 默认样式

### 规划中
- ⬜ 项目管理页（CRUD、搜索、筛选）
- ⬜ 仪表盘统计图表（LiveCharts2）
- ⬜ 浅色 / 深色主题切换
- ⬜ 任务编辑对话框与表单验证
- ⬜ 列排序、CSV 导入导出
- ⬜ 单元测试

## 技术栈

| 分类 | 技术 |
| --- | --- |
| 框架 | .NET 8、WPF |
| 架构 | MVVM（CommunityToolkit.Mvvm） |
| 依赖注入 | Microsoft.Extensions.DependencyInjection / Generic Host |
| 数据访问 | EF Core + SQLite |
| 图表 | LiveCharts2 |
| 日志 | Serilog |
| 序列化 | CsvHelper |

## 架构分层

```
Views        UI 层，XAML 视图与 Code-behind
ViewModels   视图模型层，绑定逻辑与命令
Models       领域实体与 DTO
Services     业务服务层（不引用任何 WPF 命名空间）
Data         数据访问层，DbContext 与数据库初始化
Controls     自定义控件
Converters   值转换器
Behaviors    附加行为
Themes       主题资源（含 Generic.xaml）
```

各层之间的依赖关系：`Views → ViewModels → Services → Data / Models`，Service 层保持对 WPF 的零引用，便于测试与复用。

## 目录结构

```
TaskFlow/
├── App.xaml / App.xaml.cs        # 应用入口，Generic Host 配置与启动
├── MainWindow.xaml               # 主窗口：左侧导航 + 顶部工具栏 + 内容区
├── ViewModels/
│   ├── MainViewModel.cs          # 导航与页面切换
│   ├── ViewModelBase.cs          # ViewModel 基类
│   └── Pages/                    # 各页面 ViewModel
├── Views/                        # 各页面视图
├── Models/                       # 实体与 DTO
├── Services/                     # 业务服务
├── Data/                         # DbContext、工厂、种子数据
├── Controls/                     # 自定义控件
├── Converters/ Behaviors/        # 转换器与行为（预留）
└── Themes/                       # 主题资源
```

## 快速开始

### 环境要求
- Windows 10 及以上
- .NET 8 SDK
- Visual Studio 2022（含「.NET 桌面开发」工作负载）

### 运行步骤
```bash
git clone https://github.com/<你的用户名>/TaskFlow.git
cd TaskFlow
dotnet run --project TaskFlow/TaskFlow.csproj
```

或在 Visual Studio 中打开 `TaskFlow.sln`，按 F5 运行。

### 数据库说明
- 使用 SQLite，数据库文件 `taskflow.db` 在首次启动时自动创建于程序目录。
- 首次启动会自动写入示例数据（项目、标签与若干任务），便于直接预览界面。
- 开发阶段使用 `EnsureCreated()` 建库；生产环境将迁移到 EF Core Migrations。

## 项目亮点

- **短生命周期 DbContext**：通过 `IDbContextFactory<AppDbContext>` 在每次操作中创建独立上下文，避免桌面应用中实体长期被跟踪导致的累积问题。
- **异步竞态处理**：加载任务时使用 `CancellationTokenSource` 取消上一次未完成的请求，避免快速搜索/翻页时旧结果覆盖新结果。
- **自定义控件**：`PaginationControl` 继承 `Control`，通过依赖属性、只读计算属性与 `ControlTemplate` 实现，外观与逻辑分离，默认样式经 `ThemeInfo` 定位。

## Roadmap

- [ ] 项目管理页
- [ ] 仪表盘（LiveCharts2 图表）
- [ ] 主题切换
- [ ] 表单验证与编辑对话框
- [ ] 排序与 CSV 导入导出
- [ ] xUnit 单元测试
- [ ] EF Core Migrations

## 许可证

[MIT License](LICENSE)
