using StackExchange.Redis;
using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Scenematic.Common.Enums;
using Tijdperk.Scenematic.Logic.Services;
using Tijdperk.Scenematic.Queue.Queues;
using ResultType = Tijdperk.Common.Core.Enums.Common.ResultType;

namespace Tijdperk.Scenematic.Worker.CoverGenerator.Workers;

public class GenerateCoverWorker(
    IGenerateCoverJobQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<GenerateCoverWorker> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Cover generation worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            Guid jobId;

            try
            {
                var dequeued = await queue.DequeueAsync(stoppingToken);
                if (dequeued is null) continue;

                jobId = dequeued.Value;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (RedisTimeoutException)
            {
                logger.LogWarning("Redis timeout while dequeuing job, retrying...");
                await Task.Delay(1000, stoppingToken);
                continue;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to dequeue job");
                await Task.Delay(1000, stoppingToken);
                continue;
            }

            using var scope = scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<CoverGeneratorService>();

            try
            {
                await ProcessAsync(jobId, service, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Worker cancellation requested");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Job {JobId} failed", jobId);

                var markResult = await service.MarkFailedAsync(
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

    private async Task ProcessAsync(Guid jobId, CoverGeneratorService service, CancellationToken stoppingToken)
    {
        var jobResult = await service.GetByIdAsync(jobId);

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
        if (job is null)
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

        var markStartedResult = await service.MarkStartedAsync(jobId);
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

        // TODO - Process
        await Task.Delay(5_000, stoppingToken); // Simulate work
        var result = Result.Success();

        if (result.IsSuccess)
        {
            logger.LogInformation("Cover generation job {JobId} completed successfully", jobId);

            var markResult = await service.MarkCompletedAsync(jobId);
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

            var markResult = await service.MarkFailedAsync(
                jobId,
                GenerateCoverJobFailureReason.InternalError,
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