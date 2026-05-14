using Microsoft.Extensions.Logging;
using Tijdperk.Common.Core.Enums.Common;
using Tijdperk.Common.Core.Extensions;
using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Common.Data.Models.DTOs.Songs;
using Tijdperk.Common.Data.Models.Entities.Songs;
using Tijdperk.Common.Data.Repositories;
using Tijdperk.Logic.Extensions;
using Tijdperk.Logic.Models.Odesli;
using Tijdperk.Logic.Services.Interfaces;

namespace Tijdperk.Logic.Services;

public class SongService(
    ILogger<SongService> logger,
    IOdesliService odesli,
    ISongRepository songRepository
) : ISongService
{
    public async Task<ResultObject<SongEntity>> GetSongByIdAsync(Guid songId)
    {
        var result = await songRepository.GetByIdAsync(songId);
        if (result.IsError)
        {
            logger.LogError(
                "Error while fetching song by ID {SongId}: {ErrorMessage}",
                songId,
                result.Message
            );
        }

        return result;
    }

    public async Task<ResultObject<SongEntity>> GetSongAsync(string songUrl)
    {
        var result = await songRepository.GetByUrlAsync(songUrl);
        if (result.IsError)
        {
            logger.LogError(
                "Error while fetching song by url {SongUrl}: {ErrorMessage}",
                songUrl,
                result.Message
            );
        }

        return result;
    }

    public async Task<ResultObject<SongEntity>> GetOrCreateSongAsync(string songUrl)
    {
        var existing = await songRepository.GetByUrlAsync(songUrl);
        if (existing.Type != ResultType.NotFound && existing.Data is not null) return existing;

        if (existing.Type != ResultType.NotFound && existing.IsError)
        {
            logger.LogError(
                "Error while fetching song by url {SongUrl}: {ErrorMessage}",
                songUrl,
                existing.Message
            );
            return existing.Clone<SongEntity, SongEntity>();
        }

        var odesliResult = await odesli.GetSongInfoAsync(songUrl);
        if (odesliResult.IsError && odesliResult.Type != ResultType.NotFound)
        {
            logger.LogError(
                "Error while fetching song info from Odesli for url {SongUrl}: {ErrorMessage}",
                songUrl,
                odesliResult.Message
            );
            return odesliResult.Clone<OdesliResponse, SongEntity>();
        }

        var odesliData = odesliResult.Data;

        var meta = odesliData?.ExtractSongMeta();
        var links = odesliData?.ExtractLinks();

        var createDto = new CreateSongDto(
            Url: songUrl,
            OdesliUrl: odesliData?.PageUrl,
            Title: meta?.Title ?? null,
            Artist: meta?.ArtistName ?? null,
            AlbumCoverUrl: meta?.ThumbnailUrl ?? null,
            PlatformLinks: links ?? []
        );

        var createResult = await songRepository.CreateAsync(createDto);
        if (createResult is { IsSuccess: true, Data: not null }) return createResult;

        logger.LogError(
            "Error while creating song for url {SongUrl}: {ErrorMessage}",
            songUrl,
            createResult.Message
        );

        return createResult;
    }
}