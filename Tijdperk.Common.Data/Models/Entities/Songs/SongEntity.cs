namespace Tijdperk.Common.Data.Models.Entities.Songs;

public class SongEntity(string url, string? odesliUrl, string? title, string? artist, string? albumCoverUrl)
{
    public Guid Id { get; } = Guid.NewGuid();

    public string Url { get; } = url;
    public string? OdesliUrl { get; } = odesliUrl;

    public string? Title { get; } = title;
    public string? Artist { get; } = artist;
    public string? AlbumCoverUrl { get; } = albumCoverUrl;

    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}