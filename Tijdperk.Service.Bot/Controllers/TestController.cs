using Microsoft.AspNetCore.Mvc;

namespace Tijdperk.Service.Bot.Controllers;

[ApiController, Route("api/test")]
public class TestController(
    ILogger<TestController> logger
) : ControllerBase
{
    public record Req(
        Guid JobId,
        string MainCoverPath,
        string DebounceCoverPath,
        string ProcessingCoverPath
    );

    [HttpPost]
    public IActionResult PostAsync([FromBody] Req request)
    {
        var secret = Request.Headers["X-Webhook-Secret"].FirstOrDefault();

        logger.LogInformation(
            "Received test webhook: JobId: {JobId}, MainCoverPath: {MainCoverPath}, DebounceCoverPath: {DebounceCoverPath}, ProcessingCoverPath: {ProcessingCoverPath}, X-Webhook-Secret: {XWebhookSecret}",
            request.JobId,
            request.MainCoverPath,
            request.DebounceCoverPath,
            request.ProcessingCoverPath,
            secret
        );

        return Accepted();
    }
}