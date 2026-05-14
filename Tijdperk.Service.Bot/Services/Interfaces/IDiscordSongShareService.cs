using Discord.WebSocket;
using Tijdperk.Common.Core.Enums.Songs;

namespace Tijdperk.Service.Bot.Services.Interfaces;

public interface IDiscordSongShareService
{
    Task<bool> ShareToChannelAsync(SocketGuildUser user, string rawSongUrl, string? comment);

    Task<bool> CastVoteAsync(SocketGuildUser user, Guid songId, SongVoteLevel level);
}