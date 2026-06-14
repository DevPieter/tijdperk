using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Tijdperk.Scenematic.Common.Enums;
using Tijdperk.Scenematic.Data.Models.DTOs.Covers;
using Tijdperk.Scenematic.Data.Models.Entities.Covers;
using Tijdperk.Scenematic.Data.Postgres.Data;
using Tijdperk.Scenematic.Data.Repositories;

namespace Tijdperk.Scenematic.Data.Postgres.Repositories;

public class CoverJobRepository(ScenematicNpgsqlDataContext context) : ICoverJobRepository
{
    public async Task<GenerateCoverJob> RegisterAsync(RegisterGenerateCoverDto dto)
    {
        var job = new GenerateCoverJob()
        {
            CoverMetaHash = dto.MetaHash,
            State = GenerateCoverJobState.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        await context.GenerateCoverJobs.AddAsync(job);
        await context.SaveChangesAsync();

        var meta = new CoverMeta()
        {
            AlbumTitle = dto.AlbumTitle,
            AlbumArtist = dto.AlbumArtist,
            AlbumCoverImageUrl = dto.AlbumCoverImageUrl,
            CoverVotes = dto.CoverVotes.ConvertAll(cv => new CoverVoteMeta()
            {
                VoteLevel = cv.VoteLevel,
                AvatarUrl = cv.AvatarUrl
            })
        };

        job.CoverMeta = meta;
        await context.SaveChangesAsync();

        return job;
    }

    public async Task<GenerateCoverJob?> GetByIdAsync(Guid id) => await context.GenerateCoverJobs
        .AsNoTracking()
        .FirstOrDefaultAsync(j => j.Id == id);

    public async Task<GenerateCoverJob?> GetByMetaHashAsync(string metaHash) => await context.GenerateCoverJobs
        .AsNoTracking()
        .FirstOrDefaultAsync(j => j.CoverMetaHash == metaHash);

    public async Task<bool> UpdateAsync(Guid id, UpdateGenerateCoverDto dto)
    {
        var job = await context.GenerateCoverJobs.FirstOrDefaultAsync(j => j.Id == id);
        if (job is null) return false;

        job.State = dto.State;

        job.QueuedAt ??= dto.QueuedAt;
        job.StartedAt ??= dto.StartedAt;
        job.CompletedAt ??= dto.CompletedAt;
        job.FailedAt ??= dto.FailedAt;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasSuccessAsync(Guid id)
    {
        var result = await context.GenerateCoverJobSuccessResults.AsNoTracking()
            .FirstOrDefaultAsync(r => r.JobId == id);

        return result is not null;
    }

    public async Task<GenerateCoverJobSuccessResult?> SetSuccessAsync(Guid id, SetGenerateCoverJobSuccessDto dto)
    {
        var job = await context.GenerateCoverJobs.FirstOrDefaultAsync(j => j.Id == id);
        if (job is null) return null;

        var result = new GenerateCoverJobSuccessResult()
        {
            JobId = id,
            MainCoverPath = dto.MainCoverPath,
            DebounceCoverPath = dto.DebounceCoverPath,
            ProcessingCoverPath = dto.ProcessingCoverPath
        };

        job.SuccessResult = result;

        await context.SaveChangesAsync();
        return result;
    }

    public async Task<bool> HasFailureAsync(Guid id)
    {
        var result = await context.GenerateCoverJobFailureResults.AsNoTracking()
            .FirstOrDefaultAsync(r => r.JobId == id);

        return result is not null;
    }

    public async Task<GenerateCoverJobFailureResult?> SetFailureAsync(Guid id, SetGenerateCoverJobFailureDto dto)
    {
        var job = await context.GenerateCoverJobs.FirstOrDefaultAsync(j => j.Id == id);
        if (job is null) return null;

        var result = new GenerateCoverJobFailureResult()
        {
            JobId = id,
            FailureReason = dto.FailureReason,
            ErrorMessage = dto.ErrorMessage
        };

        job.FailureResult = result;

        await context.SaveChangesAsync();
        return result;
    }

    public async Task<bool> DeleteMetaAsync(Guid id)
    {
        var job = await context.GenerateCoverJobs
            .Include(j => j.CoverMeta)
            .ThenInclude(m => m.CoverVotes)
            .FirstOrDefaultAsync(j => j.Id == id);
        if (job is null) return false;

        if (job.CoverMeta is not null)
        {
            context.CoverVoteMetas.RemoveRange(job.CoverMeta.CoverVotes);
            context.CoverMetas.Remove(job.CoverMeta);
        }

        job.CoverMetaId = null;

        await context.SaveChangesAsync();
        return true;
    }
}