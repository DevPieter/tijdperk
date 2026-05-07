using Microsoft.Extensions.Logging;
using Tijdperk.Common.Core.Enums.Common;
using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Common.Data.Models.DTOs.SongVotes;
using Tijdperk.Common.Data.Models.Entities.SongVotes;
using Tijdperk.Common.Data.Repositories;
using Tijdperk.Logic.Services.Interfaces;

namespace Tijdperk.Logic.Services;

public class SongVoteService(
    ILogger<SongVoteService> logger,
    ISongRepository songRepository,
    ISongVoteRepository voteRepository
) : ISongVoteService
{
    public async Task<ResultObject<SongVoteEntity>> CreateOrUpdateAsync(Guid songId, CreateSongVoteDto dto)
    {
        var songResult = await songRepository.GetByIdAsync(songId);
        if (songResult.Type == ResultType.NotFound)
        {
            logger.LogWarning("Attempted to vote for non-existent song with ID {SongId}", songId);
            return ResultObject<SongVoteEntity>.NotFound("Song not found");
        }

        if (songResult.IsSuccess || songResult.Type == ResultType.NotFound)
        {
            return await voteRepository.CreateOrUpdateAsync(songId, dto);
        }

        logger.LogError(
            "Error while fetching song by ID {SongId} for voting: {ErrorMessage}",
            songId,
            songResult.Message
        );
        return ResultObject<SongVoteEntity>.InternalError("Failed to fetch song for voting");
    }
}