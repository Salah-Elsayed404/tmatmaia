using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace Tmatmaia.Services;

public sealed record UpdateInfo(Version Version, string DownloadUrl);

// Asks GitHub for the latest release and compares it with the running version.
public static class UpdateChecker
{
    public const string ReleasesPage = "https://github.com/Salah-Elsayed404/tmatmaia/releases/latest";
    private const string LatestReleaseApi = "https://api.github.com/repos/Salah-Elsayed404/tmatmaia/releases/latest";

    public static Version CurrentVersion { get; } =
        Normalize(Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0));

    // Returns the newer release, or null when this is already the latest. Throws if GitHub can't be reached.
    public static async Task<UpdateInfo?> CheckAsync()
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        http.DefaultRequestHeaders.UserAgent.ParseAdd($"Tmatmaia/{CurrentVersion}");
        http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");

        using var stream = await http.GetStreamAsync(LatestReleaseApi).ConfigureAwait(false);
        using var json = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);
        var release = json.RootElement;

        var tag = release.GetProperty("tag_name").GetString() ?? "";
        if (!Version.TryParse(tag.TrimStart('v', 'V'), out var latest) || Normalize(latest) <= CurrentVersion)
            return null;

        // Prefer a direct link to the installer, so one click starts the download.
        var url = release.TryGetProperty("html_url", out var page) ? page.GetString() : null;
        if (release.TryGetProperty("assets", out var assets))
        {
            foreach (var asset in assets.EnumerateArray())
            {
                if (asset.GetProperty("name").GetString()?.EndsWith("-setup.exe", StringComparison.OrdinalIgnoreCase) == true)
                {
                    url = asset.GetProperty("browser_download_url").GetString();
                    break;
                }
            }
        }

        return new UpdateInfo(Normalize(latest), IsGitHubLink(url) ? url! : ReleasesPage);
    }

    // The link gets opened with the shell, so only accept https links on github.com.
    private static bool IsGitHubLink(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps && uri.Host == "github.com";

    // 1.2.0 and 1.2.0.0 should count as the same version.
    private static Version Normalize(Version v) => new(v.Major, v.Minor, Math.Max(v.Build, 0));
}
