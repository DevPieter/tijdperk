using Tijdperk.Common.Core.Enums.Songs;
using Tijdperk.Service.Bot.Extensions;

namespace Tijdperk.Service.Bot.Utils;

public static class SongUtils
{
    private static readonly string[] SupportedDomains =
    [
        "spotify.com",
        "youtube.com",
        "youtu.be",
        "music.apple.com",
        "soundcloud.com",
        "deezer.com",
        "tidal.com",
        "bandcamp.com"
    ];

    public static bool IsValidSongUrl(string input)
    {
        if (!IsUrl(input)) return false;

        var uri = new Uri(input);
        return SupportedDomains.Any(domain => uri.Host.Contains(domain, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsUrl(string input)
    {
        if (!Uri.TryCreate(input, UriKind.Absolute, out var uriResult)) return false;
        return uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;
    }

    public static SongPlatform? GetPlatform(string url)
    {
        if (!IsUrl(url)) return null;

        var uri = new Uri(url);
        var host = uri.Host.ToLowerInvariant();

        foreach (var platform in Enum.GetValues<SongPlatform>())
        {
            if (!platform.GetSupportedDomains().Any(host.Contains)) continue;
            return platform;
        }

        return null;
    }
}