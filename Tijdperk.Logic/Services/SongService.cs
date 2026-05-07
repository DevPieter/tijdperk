using Microsoft.Extensions.Logging;
using Tijdperk.Common.Core.Enums.Common;
using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Common.Data.Models.DTOs.Songs;
using Tijdperk.Common.Data.Models.Entities.Songs;
using Tijdperk.Common.Data.Repositories;
using Tijdperk.Logic.Extensions;
using Tijdperk.Logic.Services.Interfaces;

namespace Tijdperk.Logic.Services;

public class SongService(
    ILogger<SongService> logger,
    IOdesliService odesli,
    ISongsRepository songsRepository
) : ISongService
{
    public async Task<ResultObject<SongEntity>?> GetOrCreateSongAsync(string songUrl)
    {
        var existing = await songsRepository.GetByUrlAsync(songUrl);
        if (existing.Type != ResultType.NotFound && existing.Data is not null) return existing;

        if (existing.Type != ResultType.NotFound && existing.IsError)
        {
            logger.LogError(
                "Error while fetching song by url {SongUrl}: {ErrorMessage}",
                songUrl,
                existing.Message
            );
            return null;
        }

        var odesliResult = await odesli.GetSongInfoAsync(songUrl);
        if (odesliResult.IsError && odesliResult.Type != ResultType.NotFound)
        {
            logger.LogError(
                "Error while fetching Odesli info for song {SongUrl}: {ErrorMessage}",
                songUrl,
                odesliResult.Message
            );
            return null;
        }

        var odesliData = odesliResult.Data;
        var meta = odesliData?.ExtractSongMeta();

        var createDto = new CreateSongDto(
            Url: songUrl,
            OdesliUrl: odesliData?.PageUrl,
            Title: meta?.Title ?? null,
            Artist: meta?.ArtistName ?? null,
            AlbumCoverUrl: meta?.ThumbnailUrl ?? null
        );

        var createResult = await songsRepository.CreateAsync(createDto);
        if (createResult is { IsSuccess: true, Data: not null }) return createResult;

        logger.LogError(
            "Error while creating song for url {SongUrl}: {ErrorMessage}",
            songUrl,
            createResult.Message
        );

        return null;
    }
}