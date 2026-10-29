using System.Windows;
using TaskFlow.ViewModels;

namespace TaskFlow;

/// <summary>
/// 主窗口：布局见 MainWindow.xaml（左侧导航栏 + 顶部工具栏 + 内容区）。
/// ViewModel 通过构造函数注入，code-behind 不包含任何业务逻辑。
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
