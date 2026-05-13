using System.Text.RegularExpressions;
using Tijdperk.Common.Core.Enums.Songs;

namespace Tijdperk.Service.Bot.Extensions;

public static class SongPlatformExtensions
{
    public static List<string> GetSupportedDomains(this SongPlatform platform)
    {
        return platform switch
        {
            SongPlatform.Spotify => ["spotify.com"],
            SongPlatform.YouTube => ["youtube.com", "youtu.be"],
            _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null)
        };
    }

    public static string GetIdRegexPattern(this SongPlatform platform)
    {
        return platform switch
        {
            SongPlatform.Spotify
                => @"https?://open\.spotify\.com/track/(?<id>[a-zA-Z0-9]+)",
            SongPlatform.YouTube
                => @"https?://(?:www\.)?youtube\.com/watch\?v=(?<id>[a-zA-Z0-9_-]+)|https?://youtu\.be/(?<id>[a-zA-Z0-9_-]+)",
            _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null)
        };
    }

    public static string? ExtractId(this SongPlatform platform, string url)
    {
        var pattern = platform.GetIdRegexPattern();
        var match = Regex.Match(url, pattern);

        return match.Success ? match.Groups["id"].Value : null;
    }
}