using System.ComponentModel.DataAnnotations;
using Tijdperk.Common.Core.Enums.Songs;

namespace Tijdperk.Common.Data.Models.Entities.SharedSongVotes;

public class SharedSongVoteEntity
{
    [Key]
    public Guid Id { get; private init; }

    public Guid SharedSongId { get; private init; }

    public ulong UserId { get; private init; }
    public SongVoteLevel Level { get; private set; }

    public DateTime CreatedAt { get; private init; }
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private SharedSongVoteEntity()
    {
    }

    public SharedSongVoteEntity(Guid sharedSongId, ulong userId, SongVoteLevel level)
    {
        Id = Guid.NewGuid();
        SharedSongId = sharedSongId;
        UserId = userId;
        Level = level;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLevel(SongVoteLevel newLevel)
    {
        Level = newLevel;
        UpdatedAt = DateTime.UtcNow;
    }
}