using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Common.Data.Models.DTOs.Songs;
using Tijdperk.Common.Data.Models.Entities.Songs;
using Tijdperk.Common.Data.Repositories;
using Tijdperk.Data.Postgres.Data;

namespace Tijdperk.Data.Postgres.Repositories;

public class SongsRepository(NpgsqlDataContext context) : ISongsRepository
{
    public Task<ResultObject<SongEntity>> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<ResultObject<SongEntity>> GetByUrlAsync(string url)
    {
        throw new NotImplementedException();
    }

    public Task<ResultObject<SongEntity>> CreateAsync(CreateSongDto dto)
    {
        throw new NotImplementedException();
    }
}