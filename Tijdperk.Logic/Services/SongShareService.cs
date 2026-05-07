using Microsoft.Extensions.Logging;
using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Common.Data.Models.DTOs.SharedSongs;
using Tijdperk.Common.Data.Models.Entities.SharedSongs;
using Tijdperk.Common.Data.Models.Entities.Songs;
using Tijdperk.Common.Data.Repositories;
using Tijdperk.Logic.Services.Interfaces;

namespace Tijdperk.Logic.Services;

public class SongShareService(
    ILogger<SongShareService> logger,
    ISharedSongRepository shareRepository
) : ISongShareService
{
    public async Task<Result> RegisterShareAsync(SongEntity song, ulong userId, ulong messageId, string? comment)
    {
        var createShareResult = await shareRepository.CreateAsync(song.Id, new CreateSharedSongDto(
            UserId: userId,
            MessageId: messageId,
            Comment: comment
        ));

        if (createShareResult is { IsSuccess: true, Data: not null }) return Result.Success();

        logger.LogError(
            "Error while creating shared song entry for song {SongId}, user {UserId}, message {MessageId}: {ErrorMessage}",
            song.Id,
            userId,
            messageId,
            createShareResult.Message
        );
        return Result.InternalError("Failed to create shared song entry");
    }

    public async Task<ResultObject<List<SharedSongEntity>>> GetBySongIdAsync(Guid songId)
        => await shareRepository.GetBySongIdAsync(songId);

    public async Task<ResultObject<ulong>> GetLatestMessageIdBySongIdAsync(Guid songId)
        => await shareRepository.GetLatestMessageIdBySongIdAsync(songId);

    public async Task<Result> ClearMessageIdsAsync(List<ulong> messageIds)
        => await shareRepository.ClearMessageIdsAsync(messageIds);
}