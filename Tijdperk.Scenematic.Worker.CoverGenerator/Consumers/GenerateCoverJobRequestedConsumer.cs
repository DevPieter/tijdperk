using MassTransit;
using Tijdperk.Common.Core.Enums.Common;
using Tijdperk.Scenematic.Common.Enums;
using Tijdperk.Scenematic.Data.Models.DTOs.Covers;
using Tijdperk.Scenematic.Logic.Services;
using Tijdperk.Scenematic.Queue.Contracts.GenerateCoverJobs;
using Tijdperk.Scenematic.Worker.CoverGenerator.Models.Covers;
using Tijdperk.Scenematic.Worker.CoverGenerator.Processors;

namespace Tijdperk.Scenematic.Worker.CoverGenerator.Consumers;

public class GenerateCoverJobRequestedConsumer(
    IServiceScopeFactory scopeFactory,
    ILogger<GenerateCoverJobRequestedConsumer> logger
) : IConsumer<GenerateCoverJobRequested>
{
    private record SuccessObject(string MainCoverPath, string DebounceCoverPath, string ProcessingCoverPath);

    private record FailureObject(GenerateCoverJobFailureReason Reason, string Message);

    private enum ProcessJobOutcome
    {
        Completed,
        Failed,
        Ignored
    }

    private record ProcessJobResult(
        ProcessJobOutcome Outcome,
        SuccessObject? SuccessData = null,
        FailureObject? FailureData = null
    );

    public async Task Consume(ConsumeContext<GenerateCoverJobRequested> context)
    {
        var jobId = context.Message.JobId;

        using var scope = scopeFactory.CreateScope();
        var jobService = scope.ServiceProvider.GetRequiredService<CoverGeneratorJobService>();
        var processor = scope.ServiceProvider.GetRequiredService<CoverProcessor>();

        try
        {
            var result = await ProcessAsync(
                jobId,
                jobService,
                processor,
                context.CancellationToken
            );

            switch (result.Outcome)
            {
                case ProcessJobOutcome.Completed:
                    var successData = result.SuccessData!;

                    await context.Publish(new GenerateCoverJobCompleted(
                        jobId,
                        successData.MainCoverPath,
                        successData.DebounceCoverPath,
                        successData.ProcessingCoverPath
                    ));
                    break;

                case ProcessJobOutcome.Failed:
                    var failureData = result.FailureData!;

                    await context.Publish(new GenerateCoverJobFailed(
                        jobId,
                        failureData.Reason.ToString(),
                        failureData.Message
                    ));
                    break;

                case ProcessJobOutcome.Ignored:
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected failure while processing job {JobId}", jobId);
            throw;
        }
    }

    private async Task<ProcessJobResult> ProcessAsync(
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

            return new ProcessJobResult(
                ProcessJobOutcome.Failed,
                FailureData: new FailureObject(
                    GenerateCoverJobFailureReason.NotFound,
                    jobResult.Message ?? "Job not found"
                )
            );
        }

        if (jobResult.IsError)
        {
            logger.LogError(
                "Cover generation job {JobId} returned with error: {Message}",
                jobId,
                jobResult.Message
            );

            throw new InvalidOperationException(
                jobResult.Message ?? $"Failed to retrieve cover generation job {jobId}"
            );
        }

        var job = jobResult.Data;
        if (job?.CoverMeta is null)
        {
            logger.LogError(
                "Cover generation job {JobId} returned success but data is null",
                jobId
            );

            return await MarkFailedAsync(
                jobId,
                jobService,
                GenerateCoverJobFailureReason.InternalError,
                "Job data is null"
            );
        }

        if (job.State is GenerateCoverJobState.Completed or GenerateCoverJobState.Failed)
        {
            logger.LogInformation(
                "Cover generation job {JobId} is already in terminal state {State}",
                jobId,
                job.State
            );

            return new ProcessJobResult(ProcessJobOutcome.Ignored);
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

            throw new InvalidOperationException(
                markStartedResult.Message ?? $"Failed to mark job {jobId} as started"
            );
        }

        var result = await processor.ProcessAsync(new ProcessCoverArgs(
                JobId: jobId,
                MetaHash: job.CoverMetaHash,
                AlbumTitle: job.CoverMeta.AlbumTitle,
                AlbumArtist: job.CoverMeta.AlbumArtist,
                AlbumCoverImageUrl: job.CoverMeta.AlbumCoverImageUrl,
                CoverVotes: job.CoverMeta.CoverVotes.Select(cv => new GenerateCoverVoteDto(
                    cv.VoteLevel,
                    cv.AvatarUrl
                )).ToList()
            ),
            cancellationToken
        );

        if (result.IsError)
        {
            logger.LogError(
                "Cover generation job {JobId} failed: {Message}",
                jobId,
                result.Message
            );

            return await MarkFailedAsync(
                jobId,
                jobService,
                result.FailureReason,
                result.Message ?? "Unknown error"
            );
        }

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

            return await MarkFailedAsync(
                jobId,
                jobService,
                GenerateCoverJobFailureReason.InternalError,
                "Cover processor returned invalid result paths"
            );
        }

        var markResult = await jobService.MarkCompletedAsync(
            jobId: jobId,
            mainCoverPath: result.MainCoverPath,
            debounceCoverPath: result.DebounceCoverPath,
            processingCoverPath: result.ProcessingCoverPath
        );

        if (markResult.IsError)
        {
            logger.LogError(
                "Failed to mark job {JobId} as completed: {Message}",
                jobId,
                markResult.Message
            );

            throw new InvalidOperationException(
                markResult.Message ?? $"Failed to mark job {jobId} as completed"
            );
        }

        return new ProcessJobResult(
            ProcessJobOutcome.Completed,
            new SuccessObject(
                result.MainCoverPath,
                result.DebounceCoverPath,
                result.ProcessingCoverPath
            )
        );
    }

    private async Task<ProcessJobResult> MarkFailedAsync(
        Guid jobId,
        CoverGeneratorJobService jobService,
        GenerateCoverJobFailureReason reason,
        string message
    )
    {
        var markResult = await jobService.MarkFailedAsync(
            jobId,
            reason,
            message
        );

        if (markResult.IsError)
        {
            logger.LogError(
                "Failed to mark job {JobId} as failed: {Message}",
                jobId,
                markResult.Message
            );

            throw new InvalidOperationException(
                markResult.Message ?? $"Failed to mark job {jobId} as failed"
            );
        }

        return new ProcessJobResult(
            ProcessJobOutcome.Failed,
            FailureData: new FailureObject(reason, message)
        );
    }
}