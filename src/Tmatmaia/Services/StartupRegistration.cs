using Microsoft.Win32;

namespace Tmatmaia.Services;

public static class StartupRegistration
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "Tmatmaia";

    // The installer can add the entry too, so read the registry rather than trusting saved settings.
    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey);
        return key?.GetValue(ValueName) is not null;
    }

    public static void Set(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
        if (key is null) return;
        if (enabled && Environment.ProcessPath is { } path) key.SetValue(ValueName, $"\"{path}\"");
        else key.DeleteValue(ValueName, throwOnMissingValue: false);
    }
}
