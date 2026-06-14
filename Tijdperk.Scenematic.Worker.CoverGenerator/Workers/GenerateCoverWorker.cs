using Tijdperk.Common.Core.Enums.Common;
using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Scenematic.Common.Enums;
using Tijdperk.Scenematic.Data.Models.DTOs.Covers;
using Tijdperk.Scenematic.Logic.Services;
using Tijdperk.Scenematic.Queue.Queues;
using Tijdperk.Scenematic.Worker.CoverGenerator.Models;
using Tijdperk.Scenematic.Worker.CoverGenerator.Models.Covers;
using Tijdperk.Scenematic.Worker.CoverGenerator.Processors;
using Tijdperk.Scenematic.Worker.CoverGenerator.Rendering.Overlays;
using Tijdperk.Scenematic.Worker.CoverGenerator.Rendering.Scenes;

namespace Tijdperk.Scenematic.Worker.CoverGenerator.Workers;

public class GenerateCoverWorker(
    IGenerateCoverJobQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<GenerateCoverWorker> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Cover generation worker started");

        while (!cancellationToken.IsCancellationRequested)
        {
            Guid jobId;

            try
            {
                var dequeued = await queue.DequeueAsync(cancellationToken);
                if (dequeued is null) continue;

                jobId = dequeued.Value;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (StackExchange.Redis.RedisTimeoutException)
            {
                logger.LogWarning("Redis timeout while dequeuing job, retrying...");
                await Task.Delay(1000, cancellationToken);
                continue;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to dequeue job");
                await Task.Delay(1000, cancellationToken);
                continue;
            }

            using var scope = scopeFactory.CreateScope();
            var jobService = scope.ServiceProvider.GetRequiredService<CoverGeneratorJobService>();
            var processorService = scope.ServiceProvider.GetRequiredService<CoverProcessor>();

            try
            {
                await ProcessAsync(
                    jobId,
                    jobService,
                    processorService,
                    cancellationToken
                );
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Worker cancellation requested");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Job {JobId} failed", jobId);

                var markResult = await jobService.MarkFailedAsync(
                    jobId,
                    GenerateCoverJobFailureReason.InternalError,
                    ex.Message
                );

                if (markResult.IsError)
                {
                    logger.LogError(
                        "Failed to mark job {JobId} as failed: {Message}",
                        jobId,
                        markResult.Message
                    );
                }

                await queue.AcknowledgeAsync(jobId);
            }
        }

        logger.LogInformation("Cover generation worker stopped");
    }

    private async Task ProcessAsync(
        Guid jobId,
        CoverGeneratorJobService jobService,
        CoverProcessor processor,
        CancellationToken cancellationToken
    )
    {
        var jobResult = await jobService.GetByIdAsync(jobId, includeMetadata: true);

        if (jobResult.Type == ResultType.NotFound)
        {
            logger.LogWarning(
                "Cover generation job {JobId} could not be found: {Message}",
                jobId,
                jobResult.Message
            );
            await queue.AcknowledgeAsync(jobId);
            return;
        }

        if (jobResult.IsError)
        {
            logger.LogError(
                "Cover generation job {JobId} returned with error: {Message}",
                jobId,
                jobResult.Message
            );
            await queue.AcknowledgeAsync(jobId);
            return;
        }

        var job = jobResult.Data;
        if (job?.CoverMeta is null)
        {
            logger.LogError(
                "Cover generation job {JobId} returned success but data is null",
                jobId
            );
            await queue.AcknowledgeAsync(jobId);
            return;
        }

        if (job.State is GenerateCoverJobState.Completed or GenerateCoverJobState.Failed)
        {
            logger.LogWarning(
                "Cover generation job {JobId} is already in terminal state {State}",
                jobId,
                job.State
            );
            await queue.AcknowledgeAsync(jobId);
            return;
        }

        logger.LogInformation("Processing cover generation job {JobId}", jobId);

        var markStartedResult = await jobService.MarkStartedAsync(jobId);
        if (markStartedResult.IsError)
        {
            logger.LogError(
                "Failed to mark job {JobId} as started: {Message}",
                jobId,
                markStartedResult.Message
            );
            await queue.AcknowledgeAsync(jobId);
            return;
        }

        var result = await processor.ProcessAsync(new ProcessCoverArgs(
                jobId,
                job.CoverMetaHash,
                job.CoverMeta.AlbumTitle,
                job.CoverMeta.AlbumArtist,
                job.CoverMeta.AlbumCoverImageUrl,
                job.CoverMeta.CoverVotes.Select(cv => new GenerateCoverVoteDto(
                    cv.VoteLevel,
                    cv.AvatarUrl
                )).ToList()
            ),
            cancellationToken
        );

        if (result.IsSuccess)
        {
            logger.LogInformation("Cover generation job {JobId} completed successfully", jobId);

            if (
                string.IsNullOrEmpty(result.MainCoverPath)
                || string.IsNullOrEmpty(result.DebounceCoverPath)
                || string.IsNullOrEmpty(result.ProcessingCoverPath)
            )
            {
                logger.LogError(
                    "Cover generation job {JobId} completed successfully but result paths are invalid",
                    jobId
                );

                // TODO - Handle this case
            }

            var markResult = await jobService.MarkCompletedAsync(
                jobId: jobId,
                mainCoverPath: result.MainCoverPath!,
                debounceCoverPath: result.DebounceCoverPath!,
                processingCoverPath: result.ProcessingCoverPath!
            );

            if (markResult.IsError)
            {
                logger.LogError(
                    "Failed to mark job {JobId} as completed: {Message}",
                    jobId,
                    markResult.Message
                );
            }
        }
        else
        {
            logger.LogError(
                "Cover generation job {JobId} failed: {Message}",
                jobId,
                result.Message
            );

            var markResult = await jobService.MarkFailedAsync(
                jobId,
                result.FailureReason,
                result.Message ?? "Unknown error"
            );

            if (markResult.IsError)
            {
                logger.LogError(
                    "Failed to mark job {JobId} as failed: {Message}",
                    jobId,
                    markResult.Message
                );
            }
        }

        await queue.AcknowledgeAsync(jobId);
    }
}