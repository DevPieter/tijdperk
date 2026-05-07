using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Common.Data.Models.DTOs.SharedSongs;
using Tijdperk.Common.Data.Models.Entities.SharedSongs;
using Tijdperk.Common.Data.Repositories;
using Tijdperk.Data.Postgres.Data;

namespace Tijdperk.Data.Postgres.Repositories;

public class SharedSongRepository(NpgsqlDataContext context) : ISharedSongRepository
{
    public Task<ResultObject<SharedSongEntity>> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<ResultObject<List<SharedSongEntity>>> GetBySongIdAsync(Guid songId)
    {
        throw new NotImplementedException();
    }

    public Task<ResultObject<List<SharedSongEntity>>> GetByUserIdAsync(ulong userId)
    {
        throw new NotImplementedException();
    }

    public Task<ResultObject<SharedSongEntity>> CreateAsync(Guid songId, CreateSharedSongDto dto)
    {
        throw new NotImplementedException();
    }
}