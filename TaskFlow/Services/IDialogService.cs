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
}
