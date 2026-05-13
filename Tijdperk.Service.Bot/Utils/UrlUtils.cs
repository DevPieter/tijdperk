namespace Tijdperk.Service.Bot.Utils;

public static class UrlUtils
{
    public static bool IsValidHttpUrl(string input)
    {
        if (!Uri.TryCreate(input, UriKind.Absolute, out var uriResult)) return false;
        return uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;
    }

    public static string? ExtractNormalizedDomain(string url)
        => Uri.TryCreate(url, UriKind.Absolute, out var uriResult) ? uriResult.Host.ToLowerInvariant() : null;
}