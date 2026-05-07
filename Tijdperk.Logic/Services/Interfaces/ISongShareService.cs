using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Common.Data.Models.Entities.SharedSongs;
using Tijdperk.Common.Data.Models.Entities.Songs;

namespace Tijdperk.Logic.Services.Interfaces;

public interface ISongShareService
{
    Task<Result> RegisterShareAsync(SongEntity song, ulong userId, ulong messageId, string? comment);

    Task<ResultObject<List<SharedSongEntity>>> GetBySongIdAsync(Guid songId);

    Task<ResultObject<ulong>> GetLatestMessageIdBySongIdAsync(Guid songId);

    Task<Result> ClearMessageIdsAsync(List<ulong> messageIds);
}