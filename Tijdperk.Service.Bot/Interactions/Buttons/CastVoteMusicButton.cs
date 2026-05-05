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
    IMusicShareService shareService
) : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction(MusicVoteConstants.CustomIdCapture)]
    public async Task CastVote(string input)
    {
        if (Context.Channel.Id != options.Value.MusicChannelId) return;
        if (Context.Interaction is not SocketMessageComponent component) return;

        if (Context.User is not SocketGuildUser guildUser)
        {
            logger.LogWarning("Received a cast vote interaction from a non-guild user: {User}.", Context.User.Username);
            return;
        }

        var level = input.ToMusicVoteLevel();
        if (level is null)
        {
            logger.LogWarning("Invalid vote level input: {Input}", input);
            await component.RespondAsync("Invalid vote level.", ephemeral: true);
            return;
        }

        var success = await shareService.CastVoteAsync(guildUser, component.Message, level.Value);
        if (!success)
        {
            logger.LogWarning(
                "Failed to cast vote for user {User} on message {MessageId}. Message may not be a shared song.",
                guildUser.Username,
                component.Message.Id
            );

            await component.RespondAsync("Failed to cast vote. This message may not be a shared song.", ephemeral: true);
            return;
        }

        await component.DeferAsync();
    }
}