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
        logger.LogInformation(
            "Received cast vote interaction from user {User} with level {Level} for song ID {SongId}.",
            Context.User.Username,
            levelStr,
            songIdStr
        );

        if (Context.Channel.Id != options.Value.MusicChannelId) return;
        if (Context.Interaction is not SocketMessageComponent component) return;

        if (Context.User is not SocketGuildUser guildUser)
        {
            logger.LogWarning("Received a cast vote interaction from a non-guild user: {User}.", Context.User.Username);
            return;
        }

        var level = levelStr.ToSongVoteLevel();
        if (level is null)
        {
            logger.LogWarning("Invalid vote level input: {Input}", levelStr);
            await component.RespondAsync("Invalid vote level.", ephemeral: true);
            return;
        }

        var songId = Guid.TryParse(songIdStr, out var parsedSongId) ? parsedSongId : (Guid?)null;
        if (songId is null)
        {
            logger.LogWarning("Invalid song ID input: {Input}", songIdStr);
            await component.RespondAsync("Invalid song ID.", ephemeral: true);
            return;
        }

        var success = await shareService.CastVoteAsync(guildUser, songId.Value, level.Value);
        if (!success)
        {
            logger.LogWarning(
                "Failed to cast vote for user {User} on message {MessageId}. Message may not be a shared song.",
                guildUser.Username,
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