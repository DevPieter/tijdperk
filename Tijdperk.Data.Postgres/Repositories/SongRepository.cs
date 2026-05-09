using Microsoft.EntityFrameworkCore;
using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Common.Data.Models.DTOs.Songs;
using Tijdperk.Common.Data.Models.Entities.Songs;
using Tijdperk.Common.Data.Repositories;
using Tijdperk.Data.Postgres.Data;

namespace Tijdperk.Data.Postgres.Repositories;

public class SongRepository(NpgsqlDataContext context) : ISongRepository
{
    public async Task<ResultObject<SongEntity>> CreateAsync(CreateSongDto dto)
    {
        var entity = new SongEntity(
            url: dto.Url,
            odesliUrl: dto.OdesliUrl,
            title: dto.Title,
            artist: dto.Artist,
            albumCoverUrl: dto.AlbumCoverUrl
        );

        foreach (var pl in dto.PlatformLinks)
        {
            entity.PlatformLinks.Add(new PlatformLinkEntity(entity.Id, pl.Platform, pl.DisplayName, pl.Url));
        }

        context.Songs.Add(entity);
        await context.SaveChangesAsync();

        return ResultObject<SongEntity>.Success(entity);
    }

    public async Task<ResultObject<SongEntity>> GetByIdAsync(Guid id)
    {
        var song = await context.Songs
            .AsNoTracking()
            // .Include(s => s.PlatformLinks)
            .FirstOrDefaultAsync(s => s.Id == id);

        return song is null
            ? ResultObject<SongEntity>.NotFound("Song not found.")
            : ResultObject<SongEntity>.Success(song);
    }

    public async Task<ResultObject<SongEntity>> GetByUrlAsync(string url)
    {
        var song = await context.Songs
            .AsNoTracking()
            .Include(s => s.PlatformLinks)
            .FirstOrDefaultAsync(s => s.Url == url);

        return song is null
            ? ResultObject<SongEntity>.NotFound("Song not found.")
            : ResultObject<SongEntity>.Success(song);
    }
}