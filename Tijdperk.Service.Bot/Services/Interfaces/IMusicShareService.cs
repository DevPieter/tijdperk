using Discord.WebSocket;
using Tijdperk.Service.Bot.Enums;

namespace Tijdperk.Service.Bot.Services.Interfaces;

public interface IMusicShareService
{
    Task<bool> ShareAsync(SocketGuildUser user, string songUrl, string? comment);

    Task<bool> CastVoteAsync(SocketGuildUser user, SocketMessage message, MusicVoteLevel level);
}