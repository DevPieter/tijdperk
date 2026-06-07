using System.ComponentModel.DataAnnotations;
using Tijdperk.Common.Core.Enums.Songs;

namespace Tijdperk.Scenematic.Data.Models.Entities.Covers;

public class CoverVoteMeta
{
    public Guid Id { get; init; }

    public Guid CoverMetaId { get; init; }
    public CoverMeta CoverMeta { get; init; } = null!;

    [Required]
    public SongVoteLevel VoteLevel { get; init; }

    [Required]
    public string AvatarUrl { get; init; } = null!;
}