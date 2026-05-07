using Microsoft.EntityFrameworkCore;
using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Common.Data.Models.DTOs.SharedSongs;
using Tijdperk.Common.Data.Models.Entities.SharedSongs;
using Tijdperk.Common.Data.Repositories;
using Tijdperk.Data.Postgres.Data;

namespace Tijdperk.Data.Postgres.Repositories;

public class SharedSongRepository(NpgsqlDataContext context) : ISharedSongRepository
{
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

    public async Task<ResultObject<List<SharedSongEntity>>> GetBySongIdAsync(Guid songId)
    {
        var sharedSongs = await context.SharedSongs
            .AsNoTracking()
            .Where(ss => ss.SongId == songId)
            .ToListAsync();

        return ResultObject<List<SharedSongEntity>>.Success(sharedSongs);
    }

    public async Task<ResultObject<ulong>> GetLatestMessageIdBySongIdAsync(Guid songId)
    {
        var latestMessageId = await context.SharedSongs
            .Where(ss => ss.SongId == songId && ss.MessageId.HasValue)
            .OrderByDescending(ss => ss.CreatedAt)
            .Select(ss => ss.MessageId!.Value)
            .FirstOrDefaultAsync();

        return latestMessageId == 0
            ? ResultObject<ulong>.NotFound("No message ID found for this song")
            : ResultObject<ulong>.Success(latestMessageId);
    }

    public async Task<Result> ClearMessageIdsAsync(List<ulong> messageIds)
    {
        var sharedSongs = await context.SharedSongs
            .Where(ss => ss.MessageId.HasValue && messageIds.Contains(ss.MessageId.Value))
            .ToListAsync();

        if (sharedSongs.Count == 0) return Result.Success();

        foreach (var sharedSong in sharedSongs)
        {
            sharedSong.ClearMessageId();
        }

        await context.SaveChangesAsync();
        return Result.Success();
    }
}