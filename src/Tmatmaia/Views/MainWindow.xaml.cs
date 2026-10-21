using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Deskband11Lib.Core;
using Deskband11Lib.Wpf;
using Tmatmaia.Core;

namespace Tmatmaia.Views;

public partial class MainWindow : Window
{
    private const int GwlExStyle = -20;
    private const int WsExToolWindow = 0x80;

    public TaskbarContentHost? TaskbarContentHost { get; }

    public MainWindow(PlacementMode placement)
    {
        InitializeComponent();

        if (placement == PlacementMode.Embedded)
        {
            TaskbarContentHost = new TaskbarContentHost(this, (FrameworkElement)Content,
                new TaskbarContentHostOptions { PreferredWidth = Width, PreferredHeight = Height });
        }
        else
        {
            Topmost = true;
            var area = SystemParameters.WorkArea;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            Left = SystemParameters.PrimaryScreenWidth - Width - 340;
            Top = area.Bottom + (screenHeight - area.Bottom - Height) / 2;
            SourceInitialized += (_, _) =>
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                SetWindowLong(hwnd, GwlExStyle, GetWindowLong(hwnd, GwlExStyle) | WsExToolWindow);
            };
        }
    }

    public async Task PrepareAsync()
    {
        if (TaskbarContentHost is not null) await TaskbarContentHost.AttachWhenLayoutReadyAsync();
    }

    protected override void OnClosed(EventArgs e)
    {
        TaskbarContentHost?.Dispose();
        base.OnClosed(e);
    }

    [DllImport("user32.dll")] private static extern int GetWindowLong(nint hwnd, int index);
    [DllImport("user32.dll")] private static extern int SetWindowLong(nint hwnd, int index, int value);
}
