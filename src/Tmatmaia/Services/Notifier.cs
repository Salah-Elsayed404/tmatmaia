using Tmatmaia.Core;

namespace Tmatmaia.Services;

public sealed class Notifier
{
    private readonly System.Windows.Forms.NotifyIcon _tray;

    public Notifier(System.Windows.Forms.NotifyIcon tray) => _tray = tray;

    public void PhaseCompleted(Phase finished, Settings settings)
    {
        Sounds.Play(settings.Sound);
        if (!settings.ShowNotification) return;

        var (title, text) = finished == Phase.Focus
            ? ("Focus done", "Time for a break.")
            : ("Break over", "Back to focus.");
        _tray.ShowBalloonTip(4000, title, text, System.Windows.Forms.ToolTipIcon.Info);
    }
}
