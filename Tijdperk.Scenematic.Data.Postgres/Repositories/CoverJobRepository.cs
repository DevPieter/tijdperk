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
    public static string GenerateCoverMetaHash(RegisterGenerateCoverDto dto)
    {
        var voteStr = string.Join("|",
            dto.CoverVotes.OrderBy(cv => cv.AvatarUrl).Select(cv => $"{cv.VoteLevel}:{cv.AvatarUrl}")
        );

        var str = $"{dto.AlbumTitle}|{dto.AlbumArtist}|{dto.AlbumCoverImageUrl}|{voteStr}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(str));
        return Convert.ToBase64String(hashBytes);
    }

    public async Task<GenerateCoverJob> RegisterAsync(RegisterGenerateCoverDto dto)
    {
        var job = new GenerateCoverJob()
        {
            CoverMetaHash = GenerateCoverMetaHash(dto),
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

        job.FailureReason = dto.FailureReason;
        job.ErrorMessage = dto.ErrorMessage;

        job.QueuedAt ??= dto.QueuedAt;
        job.StartedAt ??= dto.StartedAt;
        job.CompletedAt ??= dto.CompletedAt;
        job.FailedAt ??= dto.FailedAt;

        await context.SaveChangesAsync();
        return true;
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