using Tijdperk.Common.Core.Enums.Songs;

namespace Tijdperk.Logic.Models.Scenematic.GenerateCover;

public record GenerateCoverVote(
    SongVoteLevel VoteLevel,
    string AvatarUrl
);