using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ReportEngine.Designer.Wpf;

/// <summary>
/// 修复 WPF 在混合 DPI / 多显示器下最大化时标题栏（最小化/最大化/关闭按钮）
/// 被推到屏幕外的问题：处理 WM_GETMINMAXINFO，把最大化尺寸/位置钉到
/// 当前显示器的工作区（不含任务栏），而非虚拟屏幕。
/// </summary>
public partial class MainWindow
{
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var src = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
        src?.AddHook(MaximizeBoundsHook);
    }

    private const int WM_GETMINMAXINFO = 0x0024;
    private const int MONITOR_DEFAULTTONEAREST = 0x00000002;

    private static IntPtr MaximizeBoundsHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_GETMINMAXINFO)
        {
            ApplyMonitorMaximizeBounds(hwnd, lParam);
            handled = true;
        }
        return IntPtr.Zero;
    }

    private static void ApplyMonitorMaximizeBounds(IntPtr hwnd, IntPtr lParam)
    {
        var mmi = Marshal.PtrToStructure<MINMAXINFO>(lParam);

        IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
        if (monitor != IntPtr.Zero)
        {
            var mi = new MONITORINFO { cbSize = Marshal.SizeOf(typeof(MONITORINFO)) };
            if (GetMonitorInfo(monitor, ref mi))
            {
                RECT work = mi.rcWork;
                RECT mon = mi.rcMonitor;
                mmi.ptMaxPosition.X = work.left - mon.left;
                mmi.ptMaxPosition.Y = work.top - mon.top;
                mmi.ptMaxSize.X = work.right - work.left;
                mmi.ptMaxSize.Y = work.bottom - work.top;
                // 最小跟踪尺寸：给个合理下限，避免窗口缩太小
                mmi.ptMinTrackSize.X = 640;
                mmi.ptMinTrackSize.Y = 480;
            }
        }

        Marshal.StructureToPtr(mmi, lParam, true);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X; public int Y; }

    [StructLayout(LayoutKind.Sequential)]
    private struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int left; public int top; public int right; public int bottom; }

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public int dwFlags;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
}
