using Tijdperk.Common.Data.Models.DTOs.Songs;
using Tijdperk.Common.Data.Models.Entities.Songs;
using Tijdperk.Logic.Models.Odesli;

namespace Tijdperk.Logic.Extensions;

public static class OdesliResponseExtensions
{
    public static List<string> GetFormattedLinks(this SongEntity song, int maxLinks = 5)
    {
        var formattedLinks = new List<string>();
        var priorityPlatforms = new[]
        {
            "spotify",
            "appleMusic",
            "youtube",
            "soundcloud"
        };

        foreach (var platform in priorityPlatforms)
        {
            var link = song.PlatformLinks.FirstOrDefault(l => l.Platform.Equals(platform));
            if (link is null) continue;

            formattedLinks.Add($"[{link.DisplayName}]({link.Url})");
            if (formattedLinks.Count >= maxLinks) return formattedLinks;
        }

        formattedLinks.AddRange(song.PlatformLinks
            .Where(l => !priorityPlatforms.Contains(l.Platform))
            .Take(maxLinks - formattedLinks.Count)
            .Select(l => $"[{l.DisplayName}]({l.Url})"));

        return formattedLinks;
    }

    public static OdesliSongMeta? ExtractSongMeta(this OdesliResponse response)
    {
        var songEntity = response.EntitiesByUniqueId.Values.FirstOrDefault(e =>
            e.Type == "song" &&
            !string.IsNullOrEmpty(e.Title)
        );

        if (songEntity is null || string.IsNullOrEmpty(songEntity.Title)) return null;

        return new OdesliSongMeta(
            songEntity.Title,
            songEntity.ArtistName,
            songEntity.ThumbnailUrl
        );
    }

    public static List<CreatePlatformLinkDto> ExtractLinks(this OdesliResponse response)
        => response.LinksByPlatform.Select(valuePair =>
            new CreatePlatformLinkDto(
                Platform: valuePair.Key,
                DisplayName: GetPlatformDisplayName(valuePair.Key),
                Url: valuePair.Value.Url
            )
        ).ToList();

    private static string GetPlatformDisplayName(string platform)
    {
        return platform switch
        {
            "spotify" => "Spotify",
            "itunes" => "iTunes",
            "appleMusic" => "Apple Music",
            "youtube" => "YouTube",
            "youtubeMusic" => "YouTube Music",
            "google" => "Google Play Music",
            "googleStore" => "Google Play Store",
            "pandora" => "Pandora",
            "deezer" => "Deezer",
            "tidal" => "Tidal",
            "amazonStore" => "Amazon Store",
            "amazonMusic" => "Amazon Music",
            "soundcloud" => "SoundCloud",
            "napster" => "Napster",
            "yandex" => "Yandex Music",
            "spinrilla" => "Spinrilla",
            "audius" => "Audius",
            "audiomack" => "Audiomack",
            "anghami" => "Anghami",
            "boomplay" => "Boomplay",
            "bandcamp" => "Bandcamp",
            _ => platform
        };
    }
}