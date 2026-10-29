using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TaskFlow.Common;

/// <summary>
/// 异步命令实现（ICommand）。执行期间会自动禁用按钮，防止重复提交。
///
/// 注意：CommunityToolkit.Mvvm 已内置 AsyncRelayCommand（支持 CancellationToken、异常处理等），
/// 生产代码推荐直接使用内置版本。本文件是手写参考实现 / 备用。
/// </summary>
public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<object?, Task> _execute;
    private readonly Predicate<object?>? _canExecute;
    private bool _isRunning;

    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        : this(_ => execute(), canExecute is null ? null : _ => canExecute())
    {
    }

    public AsyncRelayCommand(Func<object?, Task> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <summary>命令是否正在执行中。</summary>
    public bool IsRunning => _isRunning;

    public bool CanExecute(object? parameter) => !_isRunning && (_canExecute?.Invoke(parameter) ?? true);

    // async void 是 ICommand.Execute 的签名要求；异常必须在传入的委托内部自行捕获，
    // 否则会沿 Dispatcher 抛出导致应用崩溃（工具包内置版本已做了异常透传处理）。
    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
            return;

        try
        {
            _isRunning = true;
            RaiseCanExecuteChanged();
            await _execute(parameter);
        }
        finally
        {
            _isRunning = false;
            RaiseCanExecuteChanged();
        }
    }

    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}
