using Tijdperk.Common.Core.Enums.Songs;
using Tijdperk.Service.Bot.Extensions;

namespace Tijdperk.Service.Bot.Utils;

public static class SongUtils
{
    public static bool IsValidSongUrl(string input)
    {
        if (!UrlUtils.IsValidHttpUrl(input)) return false;

        var domain = UrlUtils.ExtractNormalizedDomain(input);
        if (domain is null) return false;

        return Enum.GetValues<SongPlatform>()
            .Any(p => p.GetSupportedDomains().Any(domain.Contains));
    }

    public static SongPlatform? GetPlatform(string input)
    {
        if (!UrlUtils.IsValidHttpUrl(input)) return null;

        var domain = UrlUtils.ExtractNormalizedDomain(input);
        if (domain is null) return null;

        foreach (var platform in Enum.GetValues<SongPlatform>())
        {
            if (!platform.GetSupportedDomains().Any(domain.Contains)) continue;
            return platform;
        }

        return null;
    }
}