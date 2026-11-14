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

        // Windows 11 look that follows the light/dark setting. The Fluent theme ships with .NET,
        // so it adds nothing to the installer. Only this window uses it: the taskbar widget draws itself.
        // ThemeMode is still marked as preview in .NET 10; the app bundles its runtime, so it can't change under users.
#pragma warning disable WPF0001
        ThemeMode = ThemeMode.System;
#pragma warning restore WPF0001

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

        VersionText.Text = $"Version {UpdateChecker.CurrentVersion}";
        ShowUpdate();
        App.UpdateFound += ShowUpdate;
        Closed += (_, _) => { Sounds.Stop(); App.UpdateFound -= ShowUpdate; };
    }

    private void ShowUpdate()
    {
        var update = App.AvailableUpdate;
        UpdatePanel.Visibility = update is null ? Visibility.Collapsed : Visibility.Visible;
        if (update is not null)
            UpdateText.Text = $"Version {update.Version} is out. You have {UpdateChecker.CurrentVersion}.";
    }

    private void OnDownloadUpdateClick(object sender, RoutedEventArgs e) => OpenLink(App.AvailableUpdate?.DownloadUrl ?? UpdateChecker.ReleasesPage);

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

    // IsCancel only closes windows opened with ShowDialog, and Settings is opened with Show, so close it here.
    private void OnCancel(object sender, RoutedEventArgs e) => Close();

    private void OnGitHubClick(object sender, RoutedEventArgs e) => OpenLink(GitHubProfileUrl);

    private static void OpenLink(string url) => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });

    private static bool TryParse(string text, out int value) =>
        int.TryParse(text, out value) && value is >= 1 and <= 999;
}
