using System.Diagnostics;
using System.Windows;
using Tmatmaia.Core;
using Tmatmaia.Services;

namespace Tmatmaia.Views;

public partial class SettingsWindow : Window
{
    private const string GitHubProfileUrl = "https://github.com/Salah-Elsayed404";

    public SettingsWindow()
    {
        InitializeComponent();
        var s = App.Engine.Settings;
        FocusBox.Text = s.FocusMinutes.ToString();
        ShortBox.Text = s.ShortBreakMinutes.ToString();
        LongBox.Text = s.LongBreakMinutes.ToString();
        EveryBox.Text = s.LongBreakEvery.ToString();
        AutoStartBox.IsChecked = s.AutoStartNext;
        SoundBox.IsChecked = s.PlaySound;
        NotifyBox.IsChecked = s.ShowNotification;
        StartupBox.IsChecked = StartupRegistration.IsEnabled();
        PlacementBox.SelectedIndex = s.Placement == PlacementMode.Embedded ? 0 : 1;
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        if (!TryParse(FocusBox.Text, out var focus) || !TryParse(ShortBox.Text, out var shortBreak) ||
            !TryParse(LongBox.Text, out var longBreak) || !TryParse(EveryBox.Text, out var every))
        {
            MessageBox.Show(this, "Enter whole numbers from 1 to 999.", "Tmatmaia Settings");
            return;
        }

        var previousPlacement = App.Engine.Settings.Placement;
        var settings = new Settings
        {
            FocusMinutes = focus,
            ShortBreakMinutes = shortBreak,
            LongBreakMinutes = longBreak,
            LongBreakEvery = every,
            AutoStartNext = AutoStartBox.IsChecked == true,
            PlaySound = SoundBox.IsChecked == true,
            ShowNotification = NotifyBox.IsChecked == true,
            StartWithWindows = StartupBox.IsChecked == true,
            Placement = PlacementBox.SelectedIndex == 0 ? PlacementMode.Embedded : PlacementMode.Overlay,
        };
        App.ApplySettings(settings, placementChanged: settings.Placement != previousPlacement);
        Close();
    }

    private void OnGitHubClick(object sender, RoutedEventArgs e) =>
        Process.Start(new ProcessStartInfo(GitHubProfileUrl) { UseShellExecute = true });

    private static bool TryParse(string text, out int value) =>
        int.TryParse(text, out value) && value is >= 1 and <= 999;
}
