using System.IO;
using System.Text.Json;

namespace Tmatmaia.Core;

public sealed class Settings
{
    public int FocusMinutes { get; set; } = 25;
    public int ShortBreakMinutes { get; set; } = 5;
    public int LongBreakMinutes { get; set; } = 15;
    public int LongBreakEvery { get; set; } = 4;
    public bool AutoStartNext { get; set; } = true;

    // A file name in %WINDIR%\Media, a full path to a .wav file, or empty for no sound.
    public string Sound { get; set; } = "Alarm01.wav";

    public bool ShowNotification { get; set; } = true;
    public bool StartWithWindows { get; set; }

    private static readonly string s_path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tmatmaia", "settings.json");

    public static Settings Load()
    {
        try
        {
            if (File.Exists(s_path))
                return JsonSerializer.Deserialize<Settings>(File.ReadAllText(s_path)) ?? new Settings();
        }
        catch { }
        return new Settings();
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(s_path)!);
        File.WriteAllText(s_path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}
