using Tijdperk.Common.Core.Enums.Songs;

namespace Tijdperk.Common.Data.Models.DTOs.SongVotes;

public record CreateSongVoteDto(
    ulong UserId,
    SongVoteLevel Level
);