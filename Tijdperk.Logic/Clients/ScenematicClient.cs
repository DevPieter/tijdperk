using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Logic.Models.Scenematic.GenerateCover;
using Tijdperk.Logic.Requests.Scenematic.GenerateCover;

namespace Tijdperk.Logic.Clients;

public class ScenematicClient(
    HttpClient client,
    ILogger<ScenematicClient> logger
)
{
    public async Task<ResultObject<GenerateCoverResponse>> GenerateCoverAsync(
        GenerateCoverArgs args,
        string callbackUrl
    )
    {
        var request = new GenerateCoverRequest(
            args.Title,
            args.Artist,
            args.CoverImageUrl,
            callbackUrl,
            args.CoverVotes
        );

        var response = await client.PostAsJsonAsync("generate-cover", request);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();

            logger.LogError(
                "Failed to generate cover. Status Code: {StatusCode}, Response: {Response}",
                response.StatusCode,
                errorContent
            );
            return ResultObject<GenerateCoverResponse>.InternalError(
                $"Failed to generate cover: {response.StatusCode}, {errorContent}"
            );
        }

        var result = await response.Content.ReadFromJsonAsync<GenerateCoverResponse>();
        if (result is not null) return ResultObject<GenerateCoverResponse>.Success(result);

        logger.LogError("Failed to deserialize response from Scenematic API.");
        return ResultObject<GenerateCoverResponse>.InternalError("Failed to deserialize response from Scenematic API.");
    }
}