using Microsoft.AspNetCore.Mvc;
using Tijdperk.Common.Core.Enums.Common;
using Tijdperk.Logic.Requests.Scenematic.GenerateCover;
using Tijdperk.Logic.Services;

namespace Tijdperk.Service.Bot.Controllers;

[ApiController, Route("api/v1/scenematic")]
public class ScenematicController(ScenematicService service) : ControllerBase
{
    private const string WebhookSecretHeaderName = "X-Webhook-Secret";

    [HttpPost("callback/generate-cover")]
    public async Task<IActionResult> OnGenerateCoverCallbackAsync([FromBody] GenerateCoverCallbackRequest request)
    {
        var secret = Request.Headers[WebhookSecretHeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(secret)) return Unauthorized("Missing webhook secret.");

        var result = await service.HandleCoverGenerationCallbackAsync(request.JobId, secret, request);
        if (result.Type == ResultType.Forbidden) return Unauthorized("Invalid webhook secret.");
        if (result.IsError) return BadRequest(result.Message);

        return Accepted();
    }
}