using System.Windows;

// 告诉 WPF 通用主题资源（themes/generic.xaml）位于本程序集内。
// 自定义控件的默认样式（DefaultStyleKey）依赖此特性才能被找到。
[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,            // 无特定主题（如 Aero/Classic）资源
    ResourceDictionaryLocation.SourceAssembly)] // 通用资源在程序集内
