using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TaskFlow.Controls;

/// <summary>
/// 自定义分页控件：上一页 / 页码按钮 / 下一页。
/// 控件自身不修改页码，而是通过 <see cref="GoToPageCommand"/> 把目标页码通知外部（ViewModel），
/// 由外部更新 CurrentPage（通常用 TwoWay 绑定回传）。
/// </summary>
public class PaginationControl : Control
{
    static PaginationControl()
    {
        // 指定默认样式键为控件自身类型，WPF 会在 Themes/Generic.xaml 中查找该样式
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(PaginationControl),
            new FrameworkPropertyMetadata(typeof(PaginationControl)));
    }

    public PaginationControl()
    {
        // 构造时先按默认值计算一次，避免绑定值恰好等于默认值时不触发回调导致初始状态为空
        Refresh();
    }

    // ───────────────── 可写依赖属性 ─────────────────

    public static readonly DependencyProperty CurrentPageProperty =
        DependencyProperty.Register(
            nameof(CurrentPage), typeof(int), typeof(PaginationControl),
            new FrameworkPropertyMetadata(1, OnPaginationChanged));

    public static readonly DependencyProperty TotalPagesProperty =
        DependencyProperty.Register(
            nameof(TotalPages), typeof(int), typeof(PaginationControl),
            new FrameworkPropertyMetadata(1, OnPaginationChanged));

    public static readonly DependencyProperty PageSizeProperty =
        DependencyProperty.Register(
            nameof(PageSize), typeof(int), typeof(PaginationControl),
            new FrameworkPropertyMetadata(20));

    public static readonly DependencyProperty GoToPageCommandProperty =
        DependencyProperty.Register(
            nameof(GoToPageCommand), typeof(ICommand), typeof(PaginationControl),
            new PropertyMetadata(null));

    // ───────────────── 只读计算属性（供模板绑定） ─────────────────

    private static readonly DependencyPropertyKey PageNumbersPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(PageNumbers), typeof(IReadOnlyList<PageItem>), typeof(PaginationControl),
            new PropertyMetadata(Array.Empty<PageItem>()));
    public static readonly DependencyProperty PageNumbersProperty = PageNumbersPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey PreviousPagePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(PreviousPage), typeof(int), typeof(PaginationControl),
            new PropertyMetadata(1));
    public static readonly DependencyProperty PreviousPageProperty = PreviousPagePropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey NextPagePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(NextPage), typeof(int), typeof(PaginationControl),
            new PropertyMetadata(1));
    public static readonly DependencyProperty NextPageProperty = NextPagePropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey IsPreviousEnabledPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsPreviousEnabled), typeof(bool), typeof(PaginationControl),
            new PropertyMetadata(false));
    public static readonly DependencyProperty IsPreviousEnabledProperty = IsPreviousEnabledPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey IsNextEnabledPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsNextEnabled), typeof(bool), typeof(PaginationControl),
            new PropertyMetadata(false));
    public static readonly DependencyProperty IsNextEnabledProperty = IsNextEnabledPropertyKey.DependencyProperty;

    // ───────────────── CLR 包装属性 ─────────────────

    public int CurrentPage
    {
        get => (int)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public int TotalPages
    {
        get => (int)GetValue(TotalPagesProperty);
        set => SetValue(TotalPagesProperty, value);
    }

    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    public ICommand GoToPageCommand
    {
        get => (ICommand)GetValue(GoToPageCommandProperty);
        set => SetValue(GoToPageCommandProperty, value);
    }

    public IReadOnlyList<PageItem> PageNumbers => (IReadOnlyList<PageItem>)GetValue(PageNumbersProperty);
    public int PreviousPage => (int)GetValue(PreviousPageProperty);
    public int NextPage => (int)GetValue(NextPageProperty);
    public bool IsPreviousEnabled => (bool)GetValue(IsPreviousEnabledProperty);
    public bool IsNextEnabled => (bool)GetValue(IsNextEnabledProperty);

    // ───────────────── 计算逻辑 ─────────────────

    private static void OnPaginationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((PaginationControl)d).Refresh();
    }

    /// <summary>页码或总页数变化时，重新计算页码列表、上一页/下一页及其可用状态。</summary>
    private void Refresh()
    {
        var total = Math.Max(1, TotalPages);
        var current = Math.Clamp(CurrentPage, 1, total);

        var pages = new List<PageItem>(total);
        for (var i = 1; i <= total; i++)
            pages.Add(new PageItem(i, i == current));

        SetValue(PageNumbersPropertyKey, pages);
        SetValue(PreviousPagePropertyKey, Math.Max(1, current - 1));
        SetValue(NextPagePropertyKey, Math.Min(total, current + 1));
        SetValue(IsPreviousEnabledPropertyKey, current > 1);
        SetValue(IsNextEnabledPropertyKey, current < total);
    }
}

/// <summary>页码按钮的数据载体：页码 + 是否为当前页（用于高亮）。</summary>
public readonly record struct PageItem(int Number, bool IsCurrent);
