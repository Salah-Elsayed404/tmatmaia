using System.Windows;
using Deskband11Lib.Core;
using Deskband11Lib.Wpf;

namespace Tmatmaia.Views;

public partial class MainWindow : Window
{
    public TaskbarContentHost TaskbarContentHost { get; }

    public MainWindow()
    {
        InitializeComponent();
        TaskbarContentHost = new TaskbarContentHost(this, (FrameworkElement)Content,
            new TaskbarContentHostOptions { PreferredWidth = Width, PreferredHeight = Height });
    }

    public async Task PrepareAsync() => await TaskbarContentHost.AttachWhenLayoutReadyAsync();

    protected override void OnClosed(EventArgs e)
    {
        TaskbarContentHost.Dispose();
        base.OnClosed(e);
    }
}
