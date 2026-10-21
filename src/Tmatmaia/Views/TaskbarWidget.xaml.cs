using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using Tmatmaia.Core;

namespace Tmatmaia.Views;

public partial class TaskbarWidget : UserControl
{
    // Segoe Fluent Icons glyphs
    private const string PlayGlyph = "";
    private const string PauseGlyph = "";

    private readonly DispatcherTimer _refresh = new() { Interval = TimeSpan.FromMilliseconds(250) };

    public TaskbarWidget()
    {
        InitializeComponent();
        ApplyTheme();

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
        ToggleButton.Content = engine.Running ? PauseGlyph : PlayGlyph;
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
        Frame.BorderBrush = new SolidColorBrush(light ? Color.FromArgb(0x40, 0, 0, 0) : Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF));
        Frame.Background = new SolidColorBrush(light ? Color.FromArgb(0x0F, 0, 0, 0) : Color.FromArgb(0x14, 0xFF, 0xFF, 0xFF));
    }

    private void OnToggleClick(object sender, RoutedEventArgs e) => App.Engine.Toggle();

    private void OnResetClick(object sender, RoutedEventArgs e) => App.Engine.Reset();

    private void OnSettingsClick(object sender, RoutedEventArgs e) => App.ShowSettings();
}
