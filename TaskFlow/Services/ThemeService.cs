using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TaskFlow.Theme;
public partial class ThemeService : ObservableObject
{
    private static readonly Uri LightUri = new("pack://application:,,,/TaskFlow;component/Themes/LightTheme.xaml");
    private static readonly Uri DarkUri = new("pack://application:,,,/TaskFlow;component/Themes/DarkTheme.xaml");

    private ResourceDictionary? _currentTheme;//记录当前加载的theme，方便移除

    public bool IsDark { get; private set; }
    public void ApplyTheme(bool dark)
    {
        var dicts = Application.Current.Resources.MergedDictionaries;
        //移除旧主题
        if (_currentTheme is not null)
            dicts.Remove(_currentTheme);

        //加载新主题
        _currentTheme = new ResourceDictionary { Source = dark ? DarkUri : LightUri };
        dicts.Insert(0, _currentTheme);//保证优先级最高，不会被项目里其他资源字典覆盖掉

        IsDark = dark;
    }
    public void ToggleTheme() => ApplyTheme(!IsDark);
}
