using System.ComponentModel.DataAnnotations;
using Tijdperk.Common.Core.Enums.Songs;

namespace Tijdperk.Common.Data.Models.Entities.SongVotes;

public class SongVoteEntity
{
    [Key]
    public Guid Id { get; private init; }

    public Guid SongId { get; private init; }

    public ulong UserId { get; private init; }
    public SongVoteLevel Level { get; private set; }

    public DateTime CreatedAt { get; private init; }
    public DateTime UpdatedAt { get; private set; }

    private SongVoteEntity()
    {
    }

    public SongVoteEntity(Guid songId, ulong userId, SongVoteLevel level)
    {
        Id = Guid.NewGuid();

        SongId = songId;
        UserId = userId;
        Level = level;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void UpdateLevel(SongVoteLevel newLevel)
    {
        Level = newLevel;
        UpdatedAt = DateTime.UtcNow;
    }
}