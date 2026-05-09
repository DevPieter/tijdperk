namespace Tijdperk.Common.Data.Models.DTOs.SharedSongs;

public record CreateSharedSongDto(
    ulong UserId,
    ulong MessageId,
    string? Comment
);