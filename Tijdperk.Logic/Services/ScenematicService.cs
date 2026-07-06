using MediatR;
using Microsoft.Extensions.Logging;
using Tijdperk.Common.Core.Extensions;
using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Logic.Clients;
using Tijdperk.Logic.Models.Scenematic.GenerateCover;
using Tijdperk.Logic.Requests.Scenematic.GenerateCover;

namespace Tijdperk.Logic.Services;

public class ScenematicService(
    ScenematicClient client,
    IMediator mediator,
    ILogger<ScenematicService> logger
)
{
    public record CoverGeneratedNotification(
        Guid JobId,
        ulong ChannelId,
        ulong MessageId,
        string Title,
        string Artist,
        string MainCoverPath,
        string DebounceCoverPath,
        string ProcessingCoverPath
    ) : INotification;

    // TODO - Get from config
    private const string CallbackUrl = "http://localhost:5111/api/v1/scenematic/callback/generate-cover";

    // TODO - Move to DB
    private static readonly Dictionary<Guid, string> JobCallbacks = new();
    // private static readonly Dictionary<Guid, ulong> JobChannelMapping = new();
    private static readonly Dictionary<Guid, (string Title, string Artist, ulong ChannelId, ulong MessageId)> JobChannelMapping = new();

    private bool ConsumeSecret(Guid jobId, string secret)
    {
        if (!JobCallbacks.TryGetValue(jobId, out var storedSecret)) return false;
        if (storedSecret != secret) return false;

        JobCallbacks.Remove(jobId);
        return true;
    }

    public async Task<Result> GenerateCoverAsync(
        ulong channelId,
        ulong messageId,
        GenerateCoverArgs args
    )
    {
        var result = await client.GenerateCoverAsync(args, CallbackUrl);
        if (result.IsError || result.Data?.Callback is null)
        {
            logger.LogError(
                "Failed to generate cover for title {Title} and artist {Artist}. Error: {Error}",
                args.Title,
                args.Artist,
                result.Message
            );
            return result.ToResult();
        }

        JobCallbacks.Add(result.Data.Id, result.Data.Callback.Secret);
        JobChannelMapping.Add(result.Data.Id, (args.Title, args.Artist, channelId, messageId));

        logger.LogInformation(
            "Successfully initiated cover generation for title {Title} and artist {Artist}. JobId: {JobId}, Callback Secret: {Secret}",
            args.Title,
            args.Artist,
            result.Data.Id,
            result.Data.Callback.Secret
        );

        return Result.Success("Cover generation initiated successfully");
    }

    public async Task<Result> HandleCoverGenerationCallbackAsync(
        Guid jobId,
        string secret,
        GenerateCoverCallbackRequest request
    )
    {
        if (!ConsumeSecret(jobId, secret))
        {
            logger.LogWarning("Invalid callback attempt for JobId: {JobId} with Secret: {Secret}", jobId, secret);
            return Result.Forbidden("Invalid callback attempt.");
        }

        var mapping = JobChannelMapping.GetValueOrDefault(jobId);
        
        var notification = new CoverGeneratedNotification(
            jobId,
            mapping.ChannelId,
            mapping.MessageId,
            mapping.Title,
            mapping.Artist,
            request.MainCoverPath,
            request.DebounceCoverPath,
            request.ProcessingCoverPath
        );

        await mediator.Publish(notification);

        logger.LogInformation(
            "Processed cover generation callback for JobId: {JobId}. MainCoverPath: {MainCoverPath}, DebounceCoverPath: {DebounceCoverPath}, ProcessingCoverPath: {ProcessingCoverPath}",
            jobId,
            request.MainCoverPath,
            request.DebounceCoverPath,
            request.ProcessingCoverPath
        );

        return Result.Success("Callback processed successfully");
    }
}