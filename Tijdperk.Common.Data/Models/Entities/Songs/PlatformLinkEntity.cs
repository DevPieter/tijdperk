using System.ComponentModel.DataAnnotations;

namespace Tijdperk.Common.Data.Models.Entities.Songs;

public class PlatformLinkEntity
{
    [Key]
    public Guid Id { get; private init; }

    public Guid SongId { get; private init; }

    [MaxLength(100)]
    public string Platform { get; private init; } = null!;

    [MaxLength(100)]
    public string DisplayName { get; private init; } = null!;

    [MaxLength(500)]
    public string Url { get; private init; } = null!;

    private PlatformLinkEntity()
    {
    }

    public PlatformLinkEntity(Guid songId, string platform, string displayName, string url)
    {
        Id = Guid.NewGuid();

        SongId = songId;
        Platform = platform;
        DisplayName = displayName;
        Url = url;
    }
}