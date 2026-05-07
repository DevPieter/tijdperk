using Tijdperk.Common.Core.Enums.Songs;

namespace Tijdperk.Common.Data.Models.DTOs.SharedSongVotes;

public record CreateSharedSongVoteDto(
    ulong UserId,
    SongVoteLevel Level
);