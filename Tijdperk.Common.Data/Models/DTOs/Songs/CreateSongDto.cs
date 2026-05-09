namespace Tijdperk.Common.Data.Models.DTOs.Songs;

public record CreateSongDto(
    string Url,
    string? OdesliUrl,
    string? Title,
    string? Artist,
    string? AlbumCoverUrl,
    List<CreatePlatformLinkDto> PlatformLinks
);