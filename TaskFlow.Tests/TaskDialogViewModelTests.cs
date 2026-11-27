using TaskFlow.Models;
using TaskFlow.Models.Dtos;
using TaskFlow.Models.Enums;
using TaskFlow.Services;
using TaskFlow.ViewModels.Dialogs;

namespace TaskFlow.Tests;
public class TaskDialogViewModelTests
{
    /// <summary>
    /// 伪造的IProjectService，用来给ViewModel注入依赖
    /// 对话框打开时会加载项目下拉列表，测试场景我们直接返回空项目列表，不依赖真实数据库
    /// 其他接口方法只是占位，本测试用不到，随便返回默认值即可
    /// </summary>
    private sealed class FakeProjectService : IProjectService
    {
        public Task<IReadOnlyList<Project>> GetListAsync(ProjectFilter filter, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Project>>(new List<Project>());

        public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default)
            => Task.FromResult(project);

        public Task<Project> UpdateAsync(Project project, CancellationToken cancellationToken = default)
            => Task.FromResult(project);
    }

    /// <summary>
    /// 辅助方法：快速构造一个TaskDialogViewModel实例
    /// 传入伪造的项目服务，关闭回调用空委托（测试不需要真正关闭弹窗）
    /// existing：null=新建任务，传入TaskItem=编辑任务
    /// </summary>
    private static TaskDialogViewModel CreateViewModel(TaskItem? existing = null)
        => new(new FakeProjectService(), _=> { }, existing);

    [Fact(DisplayName = "标题为空时，不能保存")]
    public void Save_WhenTitleIsEmpty_IsDisabled()
    {
        var vm = CreateViewModel();
        vm.Title = "   ";//全是空格

        Assert.False(vm.SaveCommand.CanExecute(null));
    }

    [Fact(DisplayName = "标题非空时，可以保存")]
    public void Save_WhenTitleHasValue_IsEnabled()
    {
        var vm = CreateViewModel();
        vm.Title = "写代码";

        Assert.True(vm.SaveCommand.CanExecute(null));
    }

    [Fact(DisplayName = "保存时，标题会被去掉首位空格")]
    public void Save_TrimsTitle()
    {
        var vm = CreateViewModel();
        vm.Title = "  写代码  ";

        vm.SaveCommand.Execute(null);

        Assert.Equal("写代码", vm.Result!.Title);
    }

    [Fact(DisplayName = "状态为已完成时，保存后完成时间被填充")]
    public void Save_WhenStatusDone_SetsCompletedAt()
    {
        var vm = CreateViewModel();
        vm.Title = "任务";
        vm.Status = TaskItemStatus.Done;

        vm.SaveCommand.Execute(null);

        Assert.NotNull(vm.Result!.CompletedAt);
    }

    [Fact(DisplayName = "状态为待办时，保存后完成时间为空")]
    public void Save_WhenStatusTodo_ClearsCompletedAt()
    {
        var vm = CreateViewModel();
        vm.Title = "任务";
        vm.Status = TaskItemStatus.Todo;

        vm.SaveCommand.Execute(null);

        Assert.Null(vm.Result!.CompletedAt);
    }
}
