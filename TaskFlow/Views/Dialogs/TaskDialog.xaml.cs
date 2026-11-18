using System.Windows;
using TaskFlow.Models;
using TaskFlow.Services;
using TaskFlow.ViewModels.Dialogs;

namespace TaskFlow.Views.Dialogs;

/// <summary>
/// 任务编辑对话框窗口。
/// 代码后置只负责：接收依赖、创建 ViewModel、接上关闭回调。
/// 其余逻辑（输入、校验、结果、项目加载）全在 TaskDialogViewModel 里。
/// </summary>
public partial class TaskDialog : Window
{
    // 对外暴露ViewModel
    public TaskDialogViewModel ViewModel { get; }

    /// <summary>
    /// 任务弹窗构造函数
    /// </summary>
    /// <param name="projectService">项目服务，用来加载所属项目下拉选项列表</param>
    /// <param name="existing">传入null = 新建任务；传入TaskItem实例 = 编辑已有任务（预填表单）</param>
    public TaskDialog(IProjectService projectService, TaskItem? existing = null)
    {
        InitializeComponent();
        // 根据是新建还是编辑，自动修改窗口标题
        Title = existing is null ? "新建任务" : "编辑任务";

        ViewModel = new TaskDialogViewModel(projectService, success =>
        {
            DialogResult = success;
            Close();
        }, existing);

        // 将当前窗口的数据上下文绑定到ViewModel，XAML才能绑定里面的属性和命令
        DataContext = ViewModel;
    }
}
