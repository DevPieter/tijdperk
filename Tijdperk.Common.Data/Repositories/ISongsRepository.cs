using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Common.Data.Models.DTOs.Songs;
using Tijdperk.Common.Data.Models.Entities.Songs;

namespace Tijdperk.Common.Data.Repositories;

public interface ISongsRepository
{
    // TODO - Check if it is better to return a DTO instead of the entity.

    Task<ResultObject<SongEntity>> GetByIdAsync(Guid id);
    Task<ResultObject<SongEntity>> GetByUrlAsync(string url);

    Task<ResultObject<SongEntity>> CreateAsync(CreateSongDto dto);
}