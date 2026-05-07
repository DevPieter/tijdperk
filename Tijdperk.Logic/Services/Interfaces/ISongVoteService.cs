using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Common.Data.Models.DTOs.SongVotes;
using Tijdperk.Common.Data.Models.Entities.SongVotes;

namespace Tijdperk.Logic.Services.Interfaces;

public interface ISongVoteService
{
    Task<ResultObject<SongVoteEntity>> CreateOrUpdateAsync(Guid songId, CreateSongVoteDto dto);
}