using Tijdperk.Common.Core.Enums.Songs;
using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Common.Data.Models.DTOs.SharedSongVotes;
using Tijdperk.Common.Data.Models.Entities.SharedSongVotes;

namespace Tijdperk.Common.Data.Repositories;

public interface ISharedSongVoteRepository
{
    // TODO - Check if it is better to return a DTO instead of the entity.

    Task<ResultObject<SharedSongVoteEntity>> GetByIdAsync(Guid id);
    Task<ResultObject<SharedSongVoteEntity>> GetByUserIdAndSharedSongIdAsync(ulong userId, Guid sharedSongId);

    Task<ResultObject<SharedSongVoteEntity>> CreateAsync(CreateSharedSongVoteDto dto);
    Task<ResultObject<SharedSongVoteEntity>> UpdateAsync(Guid id, SongVoteLevel level);
}