namespace Tijdperk.Service.Bot.Models.Odesli;

public record OdesliEntity(
    string Id,
    string Type,
    string? Title,
    string? ArtistName,
    string? ThumbnailUrl,
    int? ThumbnailWidth,
    int? ThumbnailHeight,
    string ApiProvider,
    List<string> Platforms
);