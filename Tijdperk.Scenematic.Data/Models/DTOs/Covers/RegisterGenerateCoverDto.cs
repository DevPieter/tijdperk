namespace Tijdperk.Scenematic.Data.Models.DTOs.Covers;

public record RegisterGenerateCoverDto(
    string MetaHash,
    string AlbumTitle,
    string AlbumArtist,
    string AlbumCoverImageUrl,
    List<GenerateCoverVoteDto> CoverVotes
);