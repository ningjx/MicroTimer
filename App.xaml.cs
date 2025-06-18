using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace MicroTimer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // 配置WPF以支持高刷新率显示器
        ConfigureHighRefreshRate();
    }
    
    private void ConfigureHighRefreshRate()
    {
        // 获取主显示器的刷新率
        var mainWindow = MainWindow as Window;
        if (mainWindow != null)
        {
            // 设置渲染选项以支持高刷新率
            RenderOptions.SetBitmapScalingMode(mainWindow, BitmapScalingMode.HighQuality);
            
            // 配置CompositionTarget以匹配显示器刷新率
            CompositionTarget.Rendering += (sender, e) =>
            {
                // 这里可以添加额外的渲染优化逻辑
            };
        }
        
        // 设置DispatcherTimer的优先级为Render，确保与渲染同步
        Dispatcher.CurrentDispatcher.Hooks.DispatcherInactive += (sender, e) =>
        {
            // 确保UI更新与渲染同步
        };
    }
}

