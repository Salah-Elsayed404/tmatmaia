using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using Tmatmaia.Core;

namespace Tmatmaia.Views;

public partial class TaskbarWidget : UserControl
{
    private readonly DispatcherTimer _refresh = new() { Interval = TimeSpan.FromMilliseconds(250) };
    private readonly DispatcherTimer _singleClick = new();

    public TaskbarWidget()
    {
        InitializeComponent();
        ApplyTheme();

        _singleClick.Interval = TimeSpan.FromMilliseconds(GetDoubleClickTime());
        _singleClick.Tick += (_, _) =>
        {
            _singleClick.Stop();
            App.Engine.Toggle();
        };

        _refresh.Tick += (_, _) => App.Engine.Tick();
        App.Engine.Changed += Render;
        Loaded += (_, _) => { _refresh.Start(); Render(); };
        Unloaded += (_, _) => { _refresh.Stop(); App.Engine.Changed -= Render; };
        SystemEvents.UserPreferenceChanged += (_, _) => Dispatcher.Invoke(ApplyTheme);
    }

    private void Render()
    {
        var engine = App.Engine;
        var remaining = engine.Remaining;
        TimeText.Text = $"{(int)remaining.TotalMinutes:00}:{remaining.Seconds:00}";
        PhaseText.Text = engine.Phase switch
        {
            Phase.Focus => $"Focus  #{engine.Completed + 1}",
            Phase.ShortBreak => "Break",
            _ => "Long break",
        };
        ToggleButton.Content = engine.Running ? "⏸" : "▶";
        Bar.Value = engine.Progress;
        Bar.Foreground = new SolidColorBrush(engine.Phase == Phase.Focus
            ? Color.FromRgb(0xE5, 0x48, 0x4D)
            : Color.FromRgb(0x30, 0xA4, 0x6C));
    }

    private void ApplyTheme()
    {
        var light = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
            "SystemUsesLightTheme", 0) is int value && value == 1;
        Foreground = light ? Brushes.Black : Brushes.White;
    }

    private void OnTimeMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount >= 2)
        {
            _singleClick.Stop();
            App.ShowSettings();
        }
        else
        {
            _singleClick.Stop();
            _singleClick.Start();
        }
        e.Handled = true;
    }

    private void OnToggleClick(object sender, RoutedEventArgs e) => App.Engine.Toggle();

    private void OnResetClick(object sender, RoutedEventArgs e) => App.Engine.Reset();

    [DllImport("user32.dll")] private static extern uint GetDoubleClickTime();
}
