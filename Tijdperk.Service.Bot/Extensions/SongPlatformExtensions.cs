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
            SongPlatform.YouTube => ["youtube.com", "youtu.be", "music.youtube.com"],
            SongPlatform.SoundCloud => ["soundcloud.com"],
            SongPlatform.AppleMusic => ["music.apple.com"],
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
                => @"https?://(?:www\.)?youtube\.com/watch\?v=(?<id>[a-zA-Z0-9_-]+)|https?://youtu\.be/(?<id>[a-zA-Z0-9_-]+)|https?://music\.youtube\.com/watch\?v=(?<id>[a-zA-Z0-9_-]+)",
            SongPlatform.SoundCloud
                => @"https?://soundcloud\.com/(?<id>[\w-]+/[\w-]+)",
            SongPlatform.AppleMusic
                => @"https?://music\.apple\.com/[\w-]+/album/(?<id>[\w-]+)/(?<id>[\w-]+))", // TODO - Check if correct
            _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null)
        };
    }

    public static string? ExtractId(this SongPlatform platform, string url)
    {
        var pattern = platform.GetIdRegexPattern();
        var match = Regex.Match(url, pattern);

        return match.Success ? match.Groups["id"].Value : null;
    }

    public static string? NormalizeUrl(this SongPlatform platform, string url)
    {
        var id = platform.ExtractId(url);
        if (id is null) return null;

        return platform switch
        {
            SongPlatform.Spotify => $"https://open.spotify.com/track/{id}",
            SongPlatform.YouTube => $"https://www.youtube.com/watch?v={id}",
            SongPlatform.SoundCloud => $"https://soundcloud.com/{id}",
            SongPlatform.AppleMusic => $"https://music.apple.com/album/{id}", // TODO - Check if correct
            _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null)
        };
    }
}