using System.Reflection.Metadata;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Media.Animation;
using System.Globalization;
using System.Windows.Interop;
using System.Diagnostics;

namespace MicroTimer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private static Handler? _handler;

    // P/Invoke declarations for getting display refresh rate
    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    [System.Runtime.InteropServices.DllImport("imm32.dll")]
    public static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);

    private const int VREFRESH = 116;

    private bool _spaceKeyIsDown = false;
    private bool _mouseLeftIsDown = false;

    public MainWindow()
    {
        InitializeComponent();

        // 配置高刷新率显示器支持
        ConfigureHighRefreshRate();

        _handler = new Handler();
        this.DataContext = _handler;

        // 设置窗口属性以支持高刷新率
        this.Loaded += MainWindow_Loaded;

        // 设置窗口为焦点，以便接收键盘事件
        this.Focusable = true;
        this.KeyDown += MainWindow_KeyDown;
        this.KeyUp += MainWindow_KeyUp;
        this.MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
        this.MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
        this.MouseRightButtonUp += Window_MouseRightButtonUp;
        this.MouseDown += MainWindow_MouseDown;

        // 确保窗口获得焦点
        this.Activated += (sender, e) => this.Focus();

        this.SourceInitialized += MainWindow_SourceInitialized;
        this.Activated += (sender, e) => DisableInputMethod();
    }

    private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            e.Handled = true;
            if (!_spaceKeyIsDown)
            {
                _spaceKeyIsDown = true;
                _handler?.SingleAction();
            }
        }
        else if (e.Key == Key.R)
        {
            e.Handled = true;
            _handler?.ResetAction();
        }
    }

    private void MainWindow_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            e.Handled = true;
            _spaceKeyIsDown = false;
        }
    }

    private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!_mouseLeftIsDown)
        {
            _mouseLeftIsDown = true;
            _handler?.SingleAction();
        }
    }

    private void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _mouseLeftIsDown = false;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // 窗口加载完成后进行最终配置
        OptimizeForHighRefreshRate();

        // 确保窗口获得焦点
        this.Focus();
    }

    private void ConfigureHighRefreshRate()
    {
        // 设置渲染选项
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
        RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

        // 设置合成模式以支持高刷新率
        this.UseLayoutRounding = true;
        this.SnapsToDevicePixels = true;
    }

    private void OptimizeForHighRefreshRate()
    {
        // 获取显示器刷新率信息
        var refreshRate = GetDisplayRefreshRate();
        string titleZh = $"MicroTimer - 当前刷新率: {refreshRate}Hz - 精度{(1000.0 / refreshRate).ToString("F2")}ms - 空格/左键: 开始/暂停 - R/鼠标中键: 重置 - 右键: 放大毫秒";
        string titleEn = $"MicroTimer - Refresh Rate: {refreshRate}Hz - Precision {(1000.0 / refreshRate).ToString("F2")}ms - Space/LeftClick: Start/Pause - R/MiddleClick: Reset - RightClick: Enlarge ms";
        var culture = CultureInfo.CurrentUICulture;
        if (culture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
        {
            this.Title = titleZh;
        }
        else
        {
            this.Title = titleEn;
        }
        // 根据刷新率调整渲染设置
        if (refreshRate >= 120) // 高刷新率显示器
        {
            // 启用硬件加速
            RenderOptions.SetCachingHint(this, CachingHint.Cache);
            // 设置动画帧率
            Timeline.DesiredFrameRateProperty.OverrideMetadata(
                typeof(Timeline),
                new FrameworkPropertyMetadata(refreshRate));
        }
    }

    private int GetDisplayRefreshRate()
    {
        try
        {
            // 使用Windows API获取显示器刷新率
            IntPtr hdc = GetDC(IntPtr.Zero);
            if (hdc != IntPtr.Zero)
            {
                int refreshRate = GetDeviceCaps(hdc, VREFRESH);
                ReleaseDC(IntPtr.Zero, hdc);
                return refreshRate;
            }

            // 备用方法：使用Windows Forms
            var screen = System.Windows.Forms.Screen.PrimaryScreen;
            return screen?.Bounds.Width > 0 ? 60 : 60;
        }
        catch
        {
            return 60; // 默认值
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        // 清理资源
        _handler?.Dispose();
        base.OnClosed(e);
    }

    private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        _handler?.SwapText();
    }

    private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Middle)
        {
            _handler?.ResetAction();
        }
    }

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        DisableInputMethod();
    }

    private void DisableInputMethod()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        ImmAssociateContext(hwnd, IntPtr.Zero);
    }
}