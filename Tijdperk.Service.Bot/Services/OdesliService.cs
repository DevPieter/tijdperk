using System.Text.Json;
using Tijdperk.Service.Bot.Models.Odesli;
using Tijdperk.Service.Bot.Services.Interfaces;

namespace Tijdperk.Service.Bot.Services;

public class OdesliService(HttpClient client, ILogger<OdesliService> logger ) : IOdesliService
{
    public async Task<OdesliResponse?> GetOdesliAsync(string songUrl)
    {
        try
        {
            var requestUrl = $"https://api.song.link/v1-alpha.1/links?url={Uri.EscapeDataString(songUrl)}&songIfSingle=true";
            var response = await client.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError(
                    "Failed to get Odesli for song {Song}. Response status code: {StatusCode}",
                    songUrl,
                    response.StatusCode
                );
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var odesliResponse = JsonSerializer.Deserialize<OdesliResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (odesliResponse is not null) return odesliResponse;

            logger.LogError(
                "Failed to deserialize Odesli response for song {Song}. Response content: {Content}",
                songUrl,
                content
            );
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while getting Odesli for song {Song}.", songUrl);
            return null;
        }
    }
}