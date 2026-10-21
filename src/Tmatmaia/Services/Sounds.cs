using System.IO;
using System.Media;

namespace Tmatmaia.Services;

public static class Sounds
{
    private static readonly string s_mediaDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Media");

    private static readonly string[] s_candidates =
    [
        "Alarm01.wav", "Alarm02.wav", "Alarm03.wav", "Alarm04.wav", "Alarm05.wav",
        "Alarm06.wav", "Alarm07.wav", "Alarm08.wav", "Alarm09.wav", "Alarm10.wav",
        "Ring01.wav", "Ring02.wav", "Ring03.wav", "Ring04.wav", "Ring05.wav",
        "Ring06.wav", "Ring07.wav", "Ring08.wav", "Ring09.wav", "Ring10.wav",
        "chimes.wav", "chord.wav", "ding.wav", "notify.wav", "tada.wav",
    ];

    private static SoundPlayer? s_player;

    // Built-in Windows sounds that exist on this machine.
    public static IEnumerable<string> BuiltIn => s_candidates.Where(name => File.Exists(Path.Combine(s_mediaDir, name)));

    public static string DisplayName(string sound) =>
        string.IsNullOrEmpty(sound) ? "None" : Path.GetFileNameWithoutExtension(sound);

    public static void Play(string sound)
    {
        Stop();
        if (string.IsNullOrEmpty(sound)) return;

        var path = Path.IsPathRooted(sound) ? sound : Path.Combine(s_mediaDir, sound);
        try
        {
            s_player = new SoundPlayer(path);
            s_player.Play();
        }
        catch
        {
            // Missing or unreadable file: still make some noise.
            SystemSounds.Exclamation.Play();
        }
    }

    public static void Stop() => s_player?.Stop();
}
