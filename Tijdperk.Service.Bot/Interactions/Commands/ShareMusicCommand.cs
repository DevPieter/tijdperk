using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Tijdperk.Service.Bot.Options;
using Tijdperk.Service.Bot.Services.Interfaces;
using Tijdperk.Service.Bot.Utils;

namespace Tijdperk.Service.Bot.Interactions.Commands;

public class ShareMusicCommand(
    IOptions<GuildOptions> options,
    IMusicShareService shareService
) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("share-music", "Share a song you like or want to recommend to others.")]
    [Alias("share-song")]
    public async Task ShareMusic(
        [Discord.Interactions.Summary("link", "The link to the song.")]
        string link
    )
    {
        if (Context.User is not SocketGuildUser guildUser)
        {
            await Context.Interaction.RespondAsync(
                "This command can only be used within a server.",
                ephemeral: true
            );

            return;
        }

        if (!SongUtils.IsValidSongUrl(link))
        {
            await Context.Interaction.RespondAsync(
                "Please provide a valid song URL from.",
                ephemeral: true
            );

            return;
        }

        await Context.Interaction.DeferAsync(ephemeral: true);

        if (!await shareService.ShareAsync(guildUser, link))
        {
            await Context.Interaction.FollowupAsync(
                "Failed to share the song. Please try again later.",
                ephemeral: true
            );

            return;
        }

        await Context.Interaction.FollowupAsync(
            $"Song shared successfully! It has been posted in <#{options.Value.MusicChannelId}> for everyone to enjoy.",
            ephemeral: true
        );
    }
}