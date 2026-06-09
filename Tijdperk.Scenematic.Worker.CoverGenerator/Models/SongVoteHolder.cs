using Tijdperk.Common.Core.Enums.Songs;

namespace Tijdperk.Scenematic.Worker.CoverGenerator.Models;

public record SongVoteHolder(
    SongVoteLevel Level,
    string AvatarUrl
);