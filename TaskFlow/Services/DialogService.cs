using System.Windows;

namespace TaskFlow.Services;

/// <summary>
/// 对话框服务实现：用WPF自带的MessageBox弹窗
/// 后面如果要改自定义的，只要改这个类，接口和ViewModel都不用动
/// </summary>
public class DialogService : IDialogService
{
    public bool Confirm(string message, string title = "确认")
    {
        var result = MessageBox.Show(
            message,//提示内容
            title,//窗口标题
            MessageBoxButton.YesNo,//显示“是否”两个按钮
            MessageBoxImage.Warning);//删除操作用警示图标

        return result == MessageBoxResult.Yes;
    }
}
