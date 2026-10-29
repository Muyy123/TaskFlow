using System.Windows.Controls;

namespace TaskFlow.Views;

/// <summary>
/// 任务列表视图。DataContext（TaskListViewModel）由 MainWindow 的 DataTemplate 自动注入，
/// 这里只保留 InitializeComponent，不写业务逻辑。
/// </summary>
public partial class TaskListView : UserControl
{
    public TaskListView()
    {
        InitializeComponent();
    }
}
