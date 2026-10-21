using System.Windows;
using Tmatmaia.Core;
using Tmatmaia.Services;
using Tmatmaia.Views;

namespace Tmatmaia;

public partial class App : Application
{
    private static Mutex? s_singleInstance;
    private static MainWindow? s_mainWindow;
    private static SettingsWindow? s_settingsWindow;
    private static System.Windows.Forms.NotifyIcon? s_tray;
    private static Notifier? s_notifier;

    public static PomodoroEngine Engine { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        s_singleInstance = new Mutex(initiallyOwned: true, "Tmatmaia.SingleInstance", out var isFirst);
        if (!isFirst)
        {
            Shutdown();
            return;
        }

        Engine = new PomodoroEngine(Settings.Load());
        CreateTray();
        Engine.PhaseCompleted += finished => s_notifier?.PhaseCompleted(finished, Engine.Settings);

        await InitializeMainWindowAsync();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        s_tray?.Dispose();
        base.OnExit(e);
    }

    public static void ShowSettings()
    {
        if (s_settingsWindow is { IsLoaded: true })
        {
            s_settingsWindow.Activate();
            return;
        }
        s_settingsWindow = new SettingsWindow();
        s_settingsWindow.Closed += (_, _) => s_settingsWindow = null;
        s_settingsWindow.Show();
    }

    public static void ApplySettings(Settings settings)
    {
        settings.Save();
        StartupRegistration.Set(settings.StartWithWindows);
        Engine.ApplySettings(settings);
    }

    private static async Task InitializeMainWindowAsync()
    {
        var window = new MainWindow();
        s_mainWindow = window;
        window.TaskbarContentHost.TaskbarWindowRecreationRequired += async (_, _) => await RecreateMainWindowAsync();

        await window.PrepareAsync();
        window.Show();
    }

    // Explorer restarts, DPI changes and monitor changes tear down the hosted window, so rebuild it.
    private static async Task RecreateMainWindowAsync()
    {
        var old = s_mainWindow;
        s_mainWindow = null;
        old?.TaskbarContentHost.Dispose();

        await Task.Delay(1000);
        await InitializeMainWindowAsync();
        try { old?.Close(); } catch { }
    }

    private static void CreateTray()
    {
        var menu = new System.Windows.Forms.ContextMenuStrip();
        menu.Items.Add("Start / Pause", null, (_, _) => Engine.Toggle());
        menu.Items.Add("Reset", null, (_, _) => Engine.Reset());
        menu.Items.Add("Skip", null, (_, _) => Engine.Skip());
        menu.Items.Add("Settings", null, (_, _) => ShowSettings());
        menu.Items.Add("-");
        menu.Items.Add("Exit", null, (_, _) => Current.Shutdown());

        using var iconStream = GetResourceStream(new Uri("pack://application:,,,/Assets/tmatmaia.ico")).Stream;
        s_tray = new System.Windows.Forms.NotifyIcon
        {
            Icon = new System.Drawing.Icon(iconStream),
            Text = "Tmatmaia",
            ContextMenuStrip = menu,
            Visible = true,
        };
        s_tray.DoubleClick += (_, _) => ShowSettings();
        s_notifier = new Notifier(s_tray);
    }
}
