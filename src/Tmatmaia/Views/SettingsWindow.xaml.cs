using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
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
        NotifyBox.IsChecked = s.ShowNotification;
        StartupBox.IsChecked = StartupRegistration.IsEnabled();

        AddSound("");
        foreach (var sound in Sounds.BuiltIn) AddSound(sound);
        SelectSound(s.Sound);

        Closed += (_, _) => Sounds.Stop();
    }

    private string SelectedSound => (SoundBox.SelectedItem as ComboBoxItem)?.Tag as string ?? "";

    private void AddSound(string sound) =>
        SoundBox.Items.Add(new ComboBoxItem { Content = Sounds.DisplayName(sound), Tag = sound, ToolTip = sound });

    // Selects the sound, adding it first if it's a custom file not in the list.
    private void SelectSound(string sound)
    {
        var item = SoundBox.Items.OfType<ComboBoxItem>()
            .FirstOrDefault(i => string.Equals((string)i.Tag, sound, StringComparison.OrdinalIgnoreCase));
        if (item is null)
        {
            AddSound(sound);
            item = (ComboBoxItem)SoundBox.Items[^1];
        }
        SoundBox.SelectedItem = item;
    }

    private void OnPreviewSound(object sender, RoutedEventArgs e) => Sounds.Play(SelectedSound);

    private void OnBrowseSound(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Title = "Choose a sound", Filter = "WAV sounds (*.wav)|*.wav" };
        if (dialog.ShowDialog(this) == true) SelectSound(dialog.FileName);
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        if (!TryParse(FocusBox.Text, out var focus) || !TryParse(ShortBox.Text, out var shortBreak) ||
            !TryParse(LongBox.Text, out var longBreak) || !TryParse(EveryBox.Text, out var every))
        {
            MessageBox.Show(this, "Enter whole numbers from 1 to 999.", "Tmatmaia Settings");
            return;
        }

        App.ApplySettings(new Settings
        {
            FocusMinutes = focus,
            ShortBreakMinutes = shortBreak,
            LongBreakMinutes = longBreak,
            LongBreakEvery = every,
            AutoStartNext = AutoStartBox.IsChecked == true,
            Sound = SelectedSound,
            ShowNotification = NotifyBox.IsChecked == true,
            StartWithWindows = StartupBox.IsChecked == true,
        });
        Close();
    }

    private void OnGitHubClick(object sender, RoutedEventArgs e) =>
        Process.Start(new ProcessStartInfo(GitHubProfileUrl) { UseShellExecute = true });

    private static bool TryParse(string text, out int value) =>
        int.TryParse(text, out value) && value is >= 1 and <= 999;
}
