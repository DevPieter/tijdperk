using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Common.Data.Models.Entities.SharedSongs;
using Tijdperk.Common.Data.Models.Entities.Songs;

namespace Tijdperk.Logic.Services.Interfaces;

public interface ISongShareService
{
    Task<Result> RegisterShareAsync(SongEntity song, ulong userId, ulong messageId, string? comment);

    Task<ResultObject<List<SharedSongEntity>>> GetBySongIdAsync(Guid songId);

    Task<Result> ClearMessageIdsAsync(List<ulong> messageIds);

    // Task<bool> RemoveShareAsync(ulong userId, ulong messageId);

    // Task<bool> CastVoteAsync(ulong userId, ulong messageId, SongVoteLevel level);
}