using Microsoft.EntityFrameworkCore;
using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Common.Data.Models.DTOs.SharedSongs;
using Tijdperk.Common.Data.Models.Entities.SharedSongs;
using Tijdperk.Common.Data.Repositories;
using Tijdperk.Data.Postgres.Data;

namespace Tijdperk.Data.Postgres.Repositories;

public class SharedSongRepository(NpgsqlDataContext context) : ISharedSongRepository
{
    public async Task<ResultObject<SharedSongEntity>> GetByIdAsync(Guid id)
    {
        var sharedSong = await context.SharedSongs
            .AsNoTracking()
            .FirstOrDefaultAsync(ss => ss.Id == id);

        return sharedSong is null
            ? ResultObject<SharedSongEntity>.NotFound()
            : ResultObject<SharedSongEntity>.Success(sharedSong);
    }

    public async Task<ResultObject<List<SharedSongEntity>>> GetBySongIdAsync(Guid songId)
    {
        var sharedSongs = await context.SharedSongs
            .AsNoTracking()
            .Where(ss => ss.SongId == songId)
            .ToListAsync();

        return ResultObject<List<SharedSongEntity>>.Success(sharedSongs);
    }

    public async Task<ResultObject<List<SharedSongEntity>>> GetByUserIdAsync(ulong userId)
    {
        var sharedSongs = await context.SharedSongs
            .AsNoTracking()
            .Where(ss => ss.UserId == userId)
            .ToListAsync();

        return ResultObject<List<SharedSongEntity>>.Success(sharedSongs);
    }

    public async Task<ResultObject<SharedSongEntity>> CreateAsync(Guid songId, CreateSharedSongDto dto)
    {
        var entity = new SharedSongEntity(
            songId: songId,
            userId: dto.UserId,
            messageId: dto.MessageId,
            comment: dto.Comment
        );

        context.SharedSongs.Add(entity);
        await context.SaveChangesAsync();

        return ResultObject<SharedSongEntity>.Success(entity);
    }
}