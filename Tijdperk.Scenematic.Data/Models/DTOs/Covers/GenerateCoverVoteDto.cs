using Tijdperk.Common.Core.Enums.Songs;

namespace Tijdperk.Scenematic.Data.Models.DTOs.Covers;

public record GenerateCoverVoteDto(
    SongVoteLevel VoteLevel,
    string AvatarUrl
);