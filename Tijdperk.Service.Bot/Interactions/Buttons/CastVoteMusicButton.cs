using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Tijdperk.Service.Bot.Constants;
using Tijdperk.Service.Bot.Extensions;
using Tijdperk.Service.Bot.Options;
using Tijdperk.Service.Bot.Services.Interfaces;

namespace Tijdperk.Service.Bot.Interactions.Buttons;

public class CastVoteMusicButton(
    ILogger<CastVoteMusicButton> logger,
    IOptions<GuildOptions> options,
    IDiscordSongShareService shareService
) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction(MusicVoteConstants.CustomIdCapture)]
    public async Task CastVote(string levelStr, string songIdStr)
    {
        if (Context.Channel.Id != options.Value.MusicChannelId) return;
        if (Context.Interaction is not SocketMessageComponent component) return;
        if (Context.User is not SocketGuildUser guildUser) return;

        var level = levelStr.ToSongVoteLevel();
        if (level is null)
        {
            logger.LogWarning("User {User} provided invalid vote level input: {Input}", guildUser.Username, levelStr);
            await component.RespondAsync("Sorry, I didn't understand that vote level.", ephemeral: true);
            return;
        }

        var songId = Guid.TryParse(songIdStr, out var parsedSongId) ? parsedSongId : (Guid?)null;
        if (songId is null)
        {
            logger.LogWarning("User {User} provided invalid song ID input: {Input}", guildUser.Username, songIdStr);
            await component.RespondAsync("Sorry, I didn't understand the song ID.", ephemeral: true);
            return;
        }

        var success = await shareService.CastVoteAsync(guildUser, songId.Value, level.Value);
        if (!success)
        {
            logger.LogWarning(
                "Failed to cast vote for user {User} on song {SongId} with level {Level}. Message ID: {MessageId}",
                guildUser.Username,
                songId,
                level,
                component.Message.Id
            );

            await component.RespondAsync(
                "Failed to cast vote. This message may not be a shared song.",
                ephemeral: true
            );
            return;
        }

        await component.DeferAsync();
    }
}