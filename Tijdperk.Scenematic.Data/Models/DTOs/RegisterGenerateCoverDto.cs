namespace Tijdperk.Scenematic.Data.Models.DTOs;

public record RegisterGenerateCoverDto(
    string AlbumTitle,
    string AlbumArtist,
    string AlbumCoverImageUrl
);