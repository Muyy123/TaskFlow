using System.Windows;
using TaskFlow.Models;
using TaskFlow.Views.Dialogs;

namespace TaskFlow.Services;

/// <summary>
/// 对话框服务实现：用WPF自带的MessageBox弹窗
/// 后面如果要改自定义的，只要改这个类，接口和ViewModel都不用动
/// </summary>
public class DialogService : IDialogService
{
    private readonly IProjectService _projectService; //供任务对话框加载“所属项目”列表

    public DialogService(IProjectService projectService)
    {
        _projectService = projectService;
    }
    public bool Confirm(string message, string title = "确认")
    {
        var result = MessageBox.Show(
            message,//提示内容
            title,//窗口标题
            MessageBoxButton.YesNo,//显示“是否”两个按钮
            MessageBoxImage.Warning);//删除操作用警示图标

        return result == MessageBoxResult.Yes;
    }

    /// <summary>
    /// 弹出新增/编辑项目弹窗，处理弹窗交互并返回结果
    /// </summary>
    /// <param name="existing">传null表示“新建”，传已有项目表示“编辑”</param>
    /// <returns>用户点击确定返回项目对象；取消/关闭弹窗返回null</returns>
    public Project? ShowProjectDialog(Project? existing = null)
    {
        // 创建对话框窗口
        var dialog = new ProjectDialog(existing) { Owner = Application.Current.MainWindow };

        // ShowDialog()模态弹窗：代码会在这里阻塞，直至弹窗关闭
        // 返回值：true=点了“确定”，false/null=取消或者点了×
        var success = dialog.ShowDialog() == true;

        // 用户点确定：取出弹窗ViewModel里组装好的项目数据返回；取消则返回null
        return success ? dialog.ViewModel.Result : null;
    }

    /// <summary>
    /// 弹出新增/编辑任务弹窗，处理弹窗交互并返回结果
    /// </summary>                                                                                                                                                                                                                                                                                                                                                                                                                                                     
    public TaskItem? ShowTaskDialog(TaskItem? existing = null)
    {
        var dialog = new TaskDialog(_projectService, existing) { Owner= Application.Current.MainWindow};

        var success = dialog.ShowDialog() == true;

        return success ? dialog.ViewModel.Result : null;
    }
}
