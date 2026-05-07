using Microsoft.EntityFrameworkCore;
using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Common.Data.Models.DTOs.SongVotes;
using Tijdperk.Common.Data.Models.Entities.SongVotes;
using Tijdperk.Common.Data.Repositories;
using Tijdperk.Data.Postgres.Data;

namespace Tijdperk.Data.Postgres.Repositories;

public class SongVoteRepository(NpgsqlDataContext context) : ISongVoteRepository
{
    public async Task<ResultObject<SongVoteEntity>> CreateOrUpdateAsync(Guid songId, CreateSongVoteDto dto)
    {
        var existingVote = await context.SongVotes
            .FirstOrDefaultAsync(v => v.UserId == dto.UserId && v.SongId == songId);

        if (existingVote is not null)
        {
            existingVote.UpdateLevel(dto.Level);
            await context.SaveChangesAsync();

            return ResultObject<SongVoteEntity>.Success(existingVote);
        }

        var newVote = new SongVoteEntity(
            songId: songId,
            userId: dto.UserId,
            level: dto.Level
        );

        context.SongVotes.Add(newVote);
        await context.SaveChangesAsync();

        return ResultObject<SongVoteEntity>.Success(newVote);
    }

    // public async Task<ResultObject<SongVoteEntity>> GetByIdAsync(Guid id)
    // {
    //     var vote = await context.SongVotes
    //         .AsNoTracking()
    //         .FirstOrDefaultAsync(v => v.Id == id);
    //
    //     return vote is null
    //         ? ResultObject<SongVoteEntity>.NotFound()
    //         : ResultObject<SongVoteEntity>.Success(vote);
    // }
    //
    // public async Task<ResultObject<SongVoteEntity>> GetByUserIdAndSongIdAsync(ulong userId, Guid songId)
    // {
    //     var vote = await context.SongVotes
    //         .AsNoTracking()
    //         .FirstOrDefaultAsync(v => v.UserId == userId && v.SongId == songId);
    //
    //     return vote is null
    //         ? ResultObject<SongVoteEntity>.NotFound()
    //         : ResultObject<SongVoteEntity>.Success(vote);
    // }
}