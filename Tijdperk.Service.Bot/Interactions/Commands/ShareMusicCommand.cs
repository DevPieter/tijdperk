using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Tijdperk.Service.Bot.Options;
using Tijdperk.Service.Bot.Services.Interfaces;
using Tijdperk.Service.Bot.Utils;

namespace Tijdperk.Service.Bot.Interactions.Commands;

public class ShareMusicCommand(
    IOptions<GuildOptions> options,
    IDiscordSongShareService shareService
) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("share-music", "Share a song you like or want to recommend to others.")]
    public async Task ShareMusic(
        [Summary("link", "The link to the song.")]
        string link,
        [Summary("comment", "An optional comment about the song.")]
        string? comment = null
    )
    {
        if (Context.Guild is null || Context.User is not SocketGuildUser guildUser)
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
                "Please provide a valid song URL.",
                ephemeral: true
            );

            return;
        }

        await Context.Interaction.DeferAsync(ephemeral: true);

        if (!await shareService.ShareToChannelAsync(guildUser, link, comment))
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

    // [SlashCommand("remove-shared-music", "Remove a song you previously shared.")]
    // public async Task RemoveSharedMusic(
    //     [Summary("message-id", "The ID of the message containing the shared song.")]
    //     string messageId
    // )
    // {
    //     if (Context.User is not SocketGuildUser guildUser)
    //     {
    //         await Context.Interaction.RespondAsync(
    //             "This command can only be used within a server.",
    //             ephemeral: true
    //         );
    //         return;
    //     }
    //
    //     // TODO - Find a better way of using IDs
    //     if (!ulong.TryParse(messageId, out var parsedMessageId))
    //     {
    //         await Context.Interaction.RespondAsync(
    //             "Please provide a valid message ID.",
    //             ephemeral: true
    //         );
    //         return;
    //     }
    //
    //     if (!await shareService.RemoveShareAsync(guildUser, parsedMessageId))
    //     {
    //         await Context.Interaction.RespondAsync(
    //             "Failed to remove the shared song. Please ensure you provided the correct message ID and try again.",
    //             ephemeral: true
    //         );
    //         return;
    //     }
    //
    //     await Context.Interaction.RespondAsync(
    //         "Shared song removed successfully.",
    //         ephemeral: true
    //     );
    // }
}