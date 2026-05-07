using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Common.Data.Models.DTOs.SharedSongs;
using Tijdperk.Common.Data.Models.Entities.SharedSongs;

namespace Tijdperk.Common.Data.Repositories;

public interface ISharedSongRepository
{
    // TODO - Check if it is better to return a DTO instead of the entity.

    Task<ResultObject<SharedSongEntity>> GetByIdAsync(Guid id);
    Task<ResultObject<List<SharedSongEntity>>> GetBySongIdAsync(Guid songId);
    Task<ResultObject<List<SharedSongEntity>>> GetByUserIdAsync(ulong userId);

    Task<ResultObject<SharedSongEntity>> CreateAsync(Guid songId, CreateSharedSongDto dto);

    Task<Result> ClearMessageIdsAsync(List<ulong> messageIds);
}