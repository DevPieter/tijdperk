using System.ComponentModel.DataAnnotations;

namespace Tijdperk.Common.Data.Models.Entities.Songs;

public class SongEntity
{
    [Key]
    public Guid Id { get; private init; }

    [MaxLength(500)]
    public string Url { get; private init; } = null!;

    [MaxLength(500)]
    public string? OdesliUrl { get; private init; }

    [MaxLength(256)]
    public string? Title { get; private init; }

    [MaxLength(256)]
    public string? Artist { get; private init; }

    [MaxLength(500)]
    public string? AlbumCoverUrl { get; private init; }

    // public List<

    // public record PlatformLink(
    //     string Platform,
    //     string Url
    // );

    public DateTime CreatedAt { get; private init; }

    private SongEntity()
    {
    }

    public SongEntity(string url, string? odesliUrl, string? title, string? artist, string? albumCoverUrl)
    {
        Id = Guid.NewGuid();
        Url = url;
        OdesliUrl = odesliUrl;
        Title = title;
        Artist = artist;
        AlbumCoverUrl = albumCoverUrl;
        CreatedAt = DateTime.UtcNow;
    }
}