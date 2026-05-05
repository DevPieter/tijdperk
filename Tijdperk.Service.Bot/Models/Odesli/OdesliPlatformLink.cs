namespace Tijdperk.Service.Bot.Models.Odesli;

public record OdesliPlatformLink(
    string EntityUniqueId,
    string Url,
    string? NativeAppUriMobile,
    string? NativeAppUriDesktop
);