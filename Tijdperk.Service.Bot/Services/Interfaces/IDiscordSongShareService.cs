using Discord.WebSocket;
using Tijdperk.Common.Core.Enums.Songs;
using Tijdperk.Service.Bot.Enums;

namespace Tijdperk.Service.Bot.Services.Interfaces;

public interface IDiscordSongShareService
{
    Task<bool> ShareToChannelAsync(SocketGuildUser user, string songUrl, string? comment);

    Task<bool> RemoveShareAsync(SocketGuildUser user, ulong messageId);

    Task<bool> CastVoteAsync(SocketGuildUser user, Guid songId, SongVoteLevel level);
}