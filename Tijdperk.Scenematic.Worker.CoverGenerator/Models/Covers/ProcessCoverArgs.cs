using Tijdperk.Scenematic.Data.Models.DTOs.Covers;

namespace Tijdperk.Scenematic.Worker.CoverGenerator.Models.Covers;

public record ProcessCoverArgs(
    Guid JobId,
    string MetaHash,
    string AlbumTitle,
    string AlbumArtist,
    string AlbumCoverImageUrl,
    List<GenerateCoverVoteDto> CoverVotes
);