namespace Tijdperk.Logic.Models.Odesli;

public record OdesliResponse(
    string EntityUniqueId,
    string UserCountry,
    string PageUrl,
    Dictionary<string, OdesliPlatformLink> LinksByPlatform,
    Dictionary<string, OdesliEntity> EntitiesByUniqueId
);