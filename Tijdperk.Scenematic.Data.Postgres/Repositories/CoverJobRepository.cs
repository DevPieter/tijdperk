using Microsoft.EntityFrameworkCore;
using Tijdperk.Scenematic.Common.Enums;
using Tijdperk.Scenematic.Data.Models.DTOs;
using Tijdperk.Scenematic.Data.Models.Entities;
using Tijdperk.Scenematic.Data.Postgres.Data;
using Tijdperk.Scenematic.Data.Repositories;

namespace Tijdperk.Scenematic.Data.Postgres.Repositories;

public class CoverJobRepository(ScenematicNpgsqlDataContext context) : ICoverJobRepository
{
    public async Task<GenerateCoverJob> RegisterAsync(RegisterGenerateCoverDto dto)
    {
        var job = new GenerateCoverJob()
        {
            Id = Guid.NewGuid(),

            AlbumTitle = dto.AlbumTitle,
            AlbumArtist = dto.AlbumArtist,
            AlbumCoverImageUrl = dto.AlbumCoverImageUrl,

            State = GenerateCoverJobState.Pending,

            FailureReason = null,
            ErrorMessage = null,

            CreatedAt = DateTime.UtcNow,

            QueuedAt = null,
            StartedAt = null,
            CompletedAt = null,
            FailedAt = null
        };

        await context.GenerateCoverJobs.AddAsync(job);
        await context.SaveChangesAsync();

        return job;
    }

    public async Task<GenerateCoverJob?> GetByIdAsync(Guid id) => await context.GenerateCoverJobs.AsNoTracking()
        .FirstOrDefaultAsync(j => j.Id == id);

    public async Task<bool> UpdateAsync(Guid id, UpdateGenerateCoverDto dto)
    {
        var job = await context.GenerateCoverJobs.FirstOrDefaultAsync(j => j.Id == id);
        if (job is null) return false;

        job.State = dto.State;

        job.FailureReason = dto.FailureReason;
        job.ErrorMessage = dto.ErrorMessage;

        job.QueuedAt ??= dto.QueuedAt;
        job.StartedAt ??= dto.StartedAt;
        job.CompletedAt ??= dto.CompletedAt;
        job.FailedAt ??= dto.FailedAt;

        await context.SaveChangesAsync();
        return true;
    }
}