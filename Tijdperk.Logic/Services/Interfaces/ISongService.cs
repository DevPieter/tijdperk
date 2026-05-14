using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Common.Data.Models.Entities.Songs;

namespace Tijdperk.Logic.Services.Interfaces;

public interface ISongService
{
    Task<ResultObject<SongEntity>> GetSongByIdAsync(Guid songId);
    
    Task<ResultObject<SongEntity>> GetSongAsync(string songUrl);
    Task<ResultObject<SongEntity>> GetOrCreateSongAsync(string songUrl);
}