using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Common.Data.Models.DTOs.SongVotes;
using Tijdperk.Common.Data.Models.Entities.SongVotes;

namespace Tijdperk.Common.Data.Repositories;

public interface ISongVoteRepository
{
    // TODO - Check if it is better to return a DTO instead of the entity.
    Task<ResultObject<SongVoteEntity>> CreateOrUpdateAsync(Guid songId, CreateSongVoteDto dto);

    // Task<ResultObject<SongVoteEntity>> GetByIdAsync(Guid id);
    // Task<ResultObject<SongVoteEntity>> GetByUserIdAndSongIdAsync(ulong userId, Guid songId);
}