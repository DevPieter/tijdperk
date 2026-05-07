using System.ComponentModel.DataAnnotations;

namespace Tijdperk.Common.Data.Models.Entities.SharedSongs;

public class SharedSongEntity
{
    [Key]
    public Guid Id { get; private init; }

    public Guid SongId { get; private init; }

    public ulong UserId { get; private init; }
    public ulong MessageId { get; private init; }

    [MaxLength(300)]
    public string? Comment { get; private init; }

    public DateTime CreatedAt { get; private init; }

    private SharedSongEntity()
    {
    }

    public SharedSongEntity(Guid songId, ulong userId, ulong messageId, string? comment)
    {
        Id = Guid.NewGuid();
        SongId = songId;
        UserId = userId;
        MessageId = messageId;
        Comment = comment;
        CreatedAt = DateTime.UtcNow;
    }
}