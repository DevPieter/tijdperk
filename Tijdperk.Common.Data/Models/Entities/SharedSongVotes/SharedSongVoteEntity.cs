using Tijdperk.Common.Core.Enums.Songs;

namespace Tijdperk.Common.Data.Models.Entities.SharedSongVotes;

public class SharedSongVoteEntity(Guid sharedSongId, ulong userId, SongVoteLevel level)
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid SharedSongId { get; } = sharedSongId;

    public ulong UserId { get; } = userId;
    public SongVoteLevel Level { get; private set; } = level;

    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public void UpdateLevel(SongVoteLevel newLevel)
    {
        Level = newLevel;
        UpdatedAt = DateTime.UtcNow;
    }
}