namespace Tijdperk.Common.Data.Models.Entities.SharedSongs;

public class SharedSongEntity(Guid songId, ulong userId, ulong messageId, string? comment)
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid SongId { get; } = songId;

    public ulong UserId { get; } = userId;
    public ulong MessageId { get; } = messageId;

    public string? Comment { get; } = comment;

    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}