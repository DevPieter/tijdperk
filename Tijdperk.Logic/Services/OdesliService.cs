using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Logic.Models.Odesli;
using Tijdperk.Logic.Services.Interfaces;

namespace Tijdperk.Logic.Services;

public class OdesliService(
    HttpClient client,
    ILogger<OdesliService> logger
) : IOdesliService
{
    private string GetApiUrl(string songUrl)
        => $"https://api.song.link/v1-alpha.1/links?url={Uri.EscapeDataString(songUrl)}&songIfSingle=true";

    public async Task<ResultObject<OdesliResponse>> GetSongInfoAsync(string songUrl)
    {
        try
        {
            var response = await client.GetAsync(GetApiUrl(songUrl));

            if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest)
            {
                logger.LogWarning(
                    "Odesli does not have data for song {Song}. Response status code: {StatusCode}",
                    songUrl,
                    response.StatusCode
                );
                return ResultObject<OdesliResponse>.NotFound(
                    "Odesli does not have any data for this song."
                );
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError(
                    "Odesli API returned an error for song {Song}. Status code: {StatusCode}",
                    songUrl,
                    response.StatusCode
                );
                return ResultObject<OdesliResponse>.InternalError(
                    "Odesli API returned an error."
                );
            }

            var content = await response.Content.ReadAsStringAsync();
            var odesliResponse = JsonSerializer.Deserialize<OdesliResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (odesliResponse is not null) return ResultObject<OdesliResponse>.Success(odesliResponse);

            logger.LogError(
                "Failed to deserialize Odesli response for song {Song}. Response content: {Content}",
                songUrl,
                content
            );
            return ResultObject<OdesliResponse>.InternalError(
                "Failed to parse Odesli response."
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "An error occurred while fetching song info from Odesli for song {Song}",
                songUrl
            );
            return ResultObject<OdesliResponse>.InternalError(
                "An error occurred while fetching song info from Odesli."
            );
        }
    }
}