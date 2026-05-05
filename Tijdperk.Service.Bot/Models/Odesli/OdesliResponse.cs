namespace Tijdperk.Service.Bot.Models.Odesli;

public record OdesliResponse(
    string EntityUniqueId,
    string UserCountry,
    string PageUrl,
    Dictionary<string, OdesliPlatformLink> LinksByPlatform,
    Dictionary<string, OdesliEntity> EntitiesByUniqueId
);

public static class OdesliResponseExtensions
{
    public static List<string> GetFormattedLinks(this OdesliResponse response, int maxLinks = 5)
    {
        var formattedLinks = new List<string>();
        var priorityPlatforms = new[]
        {
            "spotify",
            "appleMusic",
            "youtube",
            "soundcloud",
            "deezer",
            "tidal",
            "amazonMusic",
            "pandora"
        };

        foreach (var platform in priorityPlatforms)
        {
            if (!response.LinksByPlatform.TryGetValue(platform, out var link)) continue;

            formattedLinks.Add($"[{GetPlatformDisplayName(platform)}]({link.Url})");
            if (formattedLinks.Count >= maxLinks) return formattedLinks;
        }

        formattedLinks.AddRange(response.LinksByPlatform
            .Where(kvp => !priorityPlatforms.Contains(kvp.Key))
            .Take(maxLinks - formattedLinks.Count)
            .Select(kvp => $"[{GetPlatformDisplayName(kvp.Key)}]({kvp.Value.Url})"));

        return formattedLinks;
    }

    public static OdesliSongMeta? GetSongMeta(this OdesliResponse response)
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