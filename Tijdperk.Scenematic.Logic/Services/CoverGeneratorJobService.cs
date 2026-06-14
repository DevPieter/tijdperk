using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Scenematic.Common.Enums;
using Tijdperk.Scenematic.Data.Models.DTOs.Covers;
using Tijdperk.Scenematic.Data.Models.Entities.Covers;
using Tijdperk.Scenematic.Data.Repositories;

namespace Tijdperk.Scenematic.Logic.Services;

public class CoverGeneratorJobService(
    ICoverJobRepository repository,
    ILogger<CoverGeneratorJobService> logger
)
{
    public static string GenerateCoverMetaHash(
        string albumTitle,
        string albumArtist,
        string albumCoverImageUrl,
        List<GenerateCoverVoteDto> coverVotes
    )
    {
        var voteStr = string.Join("|",
            coverVotes.OrderBy(cv => cv.AvatarUrl).Select(cv => $"{cv.VoteLevel}:{cv.AvatarUrl}")
        );

        var str = $"{albumTitle}|{albumArtist}|{albumCoverImageUrl}|{voteStr}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(str));
        return Convert.ToBase64String(hashBytes);
    }

    public async Task<ResultObject<GenerateCoverJob>> RegisterAsync(RegisterGenerateCoverDto dto)
    {
        try
        {
            var job = await repository.RegisterAsync(dto);
            return ResultObject<GenerateCoverJob>.Success(job);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to register cover generation job");
            return ResultObject<GenerateCoverJob>.InternalError("Failed to register cover generation job");
        }
    }

    public async Task<ResultObject<GenerateCoverJob>> GetByIdAsync(
        Guid jobId,
        bool includeMetadata = false,
        bool includeResults = false
    )
    {
        try
        {
            var job = await repository.GetByIdAsync(jobId, includeMetadata, includeResults);

            return job is null
                ? ResultObject<GenerateCoverJob>.NotFound("Job not found")
                : ResultObject<GenerateCoverJob>.Success(job);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get cover generation job by id {JobId}", jobId);
            return ResultObject<GenerateCoverJob>.InternalError("Failed to get cover generation job");
        }
    }

    public async Task<ResultObject<GenerateCoverJob>> GetByMetaHashAsync(
        string metaHash,
        bool includeMetadata = false,
        bool includeResults = false
    )
    {
        try
        {
            var job = await repository.GetByMetaHashAsync(metaHash, includeMetadata, includeResults);

            return job is null
                ? ResultObject<GenerateCoverJob>.NotFound("Job not found")
                : ResultObject<GenerateCoverJob>.Success(job);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get cover generation job by meta hash {MetaHash}", metaHash);
            return ResultObject<GenerateCoverJob>.InternalError("Failed to get cover generation job");
        }
    }

    public async Task<Result> MarkQueuedAsync(Guid jobId)
    {
        try
        {
            var job = await repository.GetByIdAsync(jobId);
            if (job is null) return Result.NotFound("Job not found");

            var dto = ToUpdateDto(job);
            dto.State = GenerateCoverJobState.Queued;

            dto.QueuedAt = DateTime.UtcNow;
            if (dto.StartedAt is not null) dto.StartedAt = null;
            if (dto.CompletedAt is not null) dto.CompletedAt = null;
            if (dto.FailedAt is not null) dto.FailedAt = null;

            var success = await repository.UpdateAsync(jobId, dto);
            return success
                ? Result.Success()
                : Result.InternalError("Failed to mark job as queued");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to mark job as queued");
            return Result.InternalError("Failed to mark job as queued");
        }
    }

    public async Task<Result> MarkStartedAsync(Guid jobId)
    {
        try
        {
            var job = await repository.GetByIdAsync(jobId);
            if (job is null) return Result.NotFound("Job not found");

            var dto = ToUpdateDto(job);
            dto.State = GenerateCoverJobState.Started;

            dto.StartedAt = DateTime.UtcNow;
            dto.QueuedAt ??= DateTime.UtcNow;
            if (dto.CompletedAt is not null) dto.CompletedAt = null;
            if (dto.FailedAt is not null) dto.FailedAt = null;

            var success = await repository.UpdateAsync(jobId, dto);
            return success
                ? Result.Success()
                : Result.InternalError("Failed to mark job as started");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to mark job as started");
            return Result.InternalError("Failed to mark job as started");
        }
    }

    public async Task<Result> MarkCompletedAsync(Guid jobId,
        // TODO - Get as args object, not params
        string mainCoverPath,
        string debounceCoverPath,
        string processingCoverPath
    )
    {
        try
        {
            var job = await repository.GetByIdAsync(jobId);
            if (job is null) return Result.NotFound("Job not found");

            var result = await repository.SetSuccessAsync(jobId, new SetGenerateCoverJobSuccessDto(
                MainCoverPath: mainCoverPath,
                DebounceCoverPath: debounceCoverPath,
                ProcessingCoverPath: processingCoverPath
            ));

            if (result is null)
            {
                logger.LogError("Failed to set success result for job id {JobId}", jobId);
                return Result.InternalError("Failed to set success result for job");
            }

            var dto = ToUpdateDto(job);
            dto.State = GenerateCoverJobState.Completed;

            dto.CompletedAt = DateTime.UtcNow;
            dto.QueuedAt ??= DateTime.UtcNow;
            dto.StartedAt ??= DateTime.UtcNow;
            if (dto.FailedAt is not null) dto.FailedAt = null;

            var success = await repository.UpdateAsync(jobId, dto);
            if (!success)
            {
                logger.LogError("Failed to mark job as completed for job id {JobId}", jobId);
                return Result.InternalError("Failed to mark job as completed");
            }

            var deleteSuccess = await repository.DeleteMetaAsync(jobId);
            if (!deleteSuccess)
            {
                logger.LogError("Failed to delete meta for completed job id {JobId}", jobId);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to mark job as completed");
            return Result.InternalError("Failed to mark job as completed");
        }
    }

    public async Task<Result> MarkFailedAsync(
        Guid jobId,
        GenerateCoverJobFailureReason reason,
        string errorMessage
    )
    {
        try
        {
            var job = await repository.GetByIdAsync(jobId);
            if (job is null) return Result.NotFound("Job not found");

            var result = await repository.SetFailureAsync(jobId, new SetGenerateCoverJobFailureDto(
                FailureReason: reason,
                ErrorMessage: errorMessage
            ));

            if (result is null)
            {
                logger.LogError(
                    "Failed to set failure result for job id {JobId}, continuing to mark job as failed",
                    jobId
                );
            }

            var dto = ToUpdateDto(job);
            dto.State = GenerateCoverJobState.Failed;

            dto.FailedAt = DateTime.UtcNow;
            dto.QueuedAt ??= DateTime.UtcNow;
            dto.StartedAt ??= DateTime.UtcNow;
            if (dto.CompletedAt is not null) dto.CompletedAt = null;

            var success = await repository.UpdateAsync(jobId, dto);

            return success
                ? Result.Success()
                : Result.InternalError("Failed to mark job as failed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to mark job as failed");
            return Result.InternalError("Failed to mark job as failed");
        }
    }

    private static UpdateGenerateCoverDto ToUpdateDto(GenerateCoverJob job) => new(
        state: job.State,
        queuedAt: job.QueuedAt,
        startedAt: job.StartedAt,
        completedAt: job.CompletedAt,
        failedAt: job.FailedAt
    );
}