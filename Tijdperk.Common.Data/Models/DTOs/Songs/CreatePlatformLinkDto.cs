namespace Tijdperk.Common.Data.Models.DTOs.Songs;

public record CreatePlatformLinkDto(
    string Platform,
    string DisplayName,
    string Url
);