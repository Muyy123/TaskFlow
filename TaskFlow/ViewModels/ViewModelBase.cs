using CommunityToolkit.Mvvm.ComponentModel;

namespace TaskFlow.ViewModels;

/// <summary>
/// 所有 ViewModel 的基类。
/// 继承 CommunityToolkit.Mvvm 的 ObservableObject，自动获得 INotifyPropertyChanged 支持。
///
/// 后续可在此加入各页面共用的能力，例如：
///   - [ObservableProperty] bool isBusy —— 加载状态（控制进度条 / 禁用按钮）
///   - Title 页面标题
///   - OnNavigatedTo / OnNavigatedFrom 导航生命周期钩子
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
}
