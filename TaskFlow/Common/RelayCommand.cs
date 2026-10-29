using System;
using System.Windows.Input;

namespace TaskFlow.Common;

/// <summary>
/// 同步命令实现（ICommand）。
///
/// 注意：CommunityToolkit.Mvvm 已经内置了 RelayCommand / AsyncRelayCommand
/// （命名空间 CommunityToolkit.Mvvm.Input），生产代码通常直接使用内置版本。
/// 本文件是手写参考实现 / 备用，方便理解 ICommand 的工作原理，也便于脱离工具包使用。
/// </summary>
public sealed class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
        : this(_ => execute(), canExecute is null ? null : _ => canExecute())
    {
    }

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => _execute(parameter);

    /// <summary>手动触发 CanExecute 重新查询（CommandManager 会在 UI 交互后自动刷新）。</summary>
    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}
