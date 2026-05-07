using Tijdperk.Common.Core.Enums.Songs;
using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Common.Data.Models.DTOs.SharedSongVotes;
using Tijdperk.Common.Data.Models.Entities.SharedSongVotes;
using Tijdperk.Common.Data.Repositories;
using Tijdperk.Data.Postgres.Data;

namespace Tijdperk.Data.Postgres.Repositories;

public class SharedSongVoteRepository(NpgsqlDataContext context) : ISharedSongVoteRepository
{
    public Task<ResultObject<SharedSongVoteEntity>> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<ResultObject<SharedSongVoteEntity>> GetByUserIdAndSharedSongIdAsync(ulong userId, Guid sharedSongId)
    {
        throw new NotImplementedException();
    }

    public Task<ResultObject<SharedSongVoteEntity>> CreateAsync(CreateSharedSongVoteDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<ResultObject<SharedSongVoteEntity>> UpdateAsync(Guid id, SongVoteLevel level)
    {
        throw new NotImplementedException();
    }
}