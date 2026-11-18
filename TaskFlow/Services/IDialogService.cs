using TaskFlow.Models;

namespace TaskFlow.Services;

/// <summary>
/// 对话框服务接口
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// 弹出确认框
    /// </summary>
    /// <param name="message">提示内容</param>
    /// <param name="title">窗口标题</param>
    /// 返回true=用户点了“是”，false=“否”
    /// <returns></returns>
    bool Confirm(string message, string title = "确认");
    /// <summary>
    /// 打开"项目编辑"对话框
    /// </summary>
    /// <param name="existing">传null表示“新建”，传已有项目表示“编辑”</param>
    /// <returns>用户填好的项目</returns>
    Project? ShowProjectDialog(Project? existing = null);

    /// <summary>
    /// 打开"任务编辑"对话框
    /// existing：传 null 表示"新建"，传已有任务表示"编辑"
    /// 返回：用户填好的任务（点确定），或 null（取消）
    /// </summary>
    TaskItem? ShowTaskDialog(TaskItem? existing = null);
}
