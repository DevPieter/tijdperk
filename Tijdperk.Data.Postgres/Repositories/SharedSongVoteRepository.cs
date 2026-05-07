using Microsoft.EntityFrameworkCore;
using Tijdperk.Common.Core.Enums.Songs;
using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Common.Data.Models.DTOs.SharedSongVotes;
using Tijdperk.Common.Data.Models.Entities.SharedSongVotes;
using Tijdperk.Common.Data.Repositories;
using Tijdperk.Data.Postgres.Data;

namespace Tijdperk.Data.Postgres.Repositories;

public class SharedSongVoteRepository(NpgsqlDataContext context) : ISharedSongVoteRepository
{
    public async Task<ResultObject<SharedSongVoteEntity>> GetByIdAsync(Guid id)
    {
        var vote = await context.SharedSongVotes
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);

        return vote is null
            ? ResultObject<SharedSongVoteEntity>.NotFound()
            : ResultObject<SharedSongVoteEntity>.Success(vote);
    }

    public async Task<ResultObject<SharedSongVoteEntity>> GetByUserIdAndSharedSongIdAsync(
        ulong userId,
        Guid sharedSongId
    )
    {
        var vote = await context.SharedSongVotes
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.UserId == userId && v.SharedSongId == sharedSongId);

        return vote is null
            ? ResultObject<SharedSongVoteEntity>.NotFound()
            : ResultObject<SharedSongVoteEntity>.Success(vote);
    }

    public async Task<ResultObject<SharedSongVoteEntity>> CreateAsync(Guid sharedSongId, CreateSharedSongVoteDto dto)
    {
        var entity = new SharedSongVoteEntity(
            sharedSongId: sharedSongId,
            userId: dto.UserId,
            level: dto.Level
        );

        context.SharedSongVotes.Add(entity);
        await context.SaveChangesAsync();

        return ResultObject<SharedSongVoteEntity>.Success(entity);
    }

    public async Task<ResultObject<SharedSongVoteEntity>> UpdateAsync(Guid id, SongVoteLevel level)
    {
        var vote = await context.SharedSongVotes.FirstOrDefaultAsync(v => v.Id == id);
        if (vote is null) return ResultObject<SharedSongVoteEntity>.NotFound();

        vote.UpdateLevel(level);
        await context.SaveChangesAsync();

        return ResultObject<SharedSongVoteEntity>.Success(vote);
    }
}