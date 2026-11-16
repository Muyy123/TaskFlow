using System.Windows;
using TaskFlow.Models;
using TaskFlow.ViewModels.Dialogs;

namespace TaskFlow.Views.Dialogs;

/// <summary>
/// 项目编辑对话框窗口
/// 代码后置只做一件事：创建ViewModel并把它接上“关闭窗口”的回调
/// 其余逻辑（输入、校验、结果）全在ProjectDialogViewModel里。
/// </summary>
public partial class ProjectDialog : Window
{
    /// <summary>
    /// 对外暴露ViewModel属性
    /// 弹窗关闭之后，外部（DialogService）可以读取 ViewModel.Result，拿到新建/编辑后的项目数据
    /// </summary>
    public ProjectDialogViewModel ViewModel { get; }

    /// <summary>
    /// existing：传 null 表示"新建"；传已有项目表示"编辑"（会预填字段）
    /// </summary>
    public ProjectDialog(Project? existing = null)
    {
        InitializeComponent();//加载XAML定义的界面元素

        Title = existing is null ? "新建项目" : "编辑项目";

        // 实例化对话框ViewModel，传入一个回调委托（Action<bool>）
        // ok = true：代表用户点了【确定】；ok = false：代表【取消】
        ViewModel = new ProjectDialogViewModel(ok =>
        {
            DialogResult = ok;
            Close();
        }, existing);

        DataContext = ViewModel;
    }
}
