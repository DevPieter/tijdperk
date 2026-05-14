using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Tijdperk.Common.Core.Extensions;
using Tijdperk.Logic.Services.Interfaces;
using Tijdperk.Service.Bot.Extensions;
using Tijdperk.Service.Bot.Options;
using Tijdperk.Service.Bot.Services.Interfaces;
using Tijdperk.Service.Bot.Utils;

namespace Tijdperk.Service.Bot.Interactions.Commands;

public class ShareMusicCommand(
    ILogger<ShareMusicCommand> logger,
    IOptions<GuildOptions> options,
    IDiscordSongShareService shareService,
    ISongService songService,
    ISongVoteService songVoteService
) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("song-info", "Get information about a previously shared song.")]
    public async Task SongInfo(
        [Summary("link", "The link to the song you want information about.")]
        string link
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

        var platform = SongUtils.GetPlatform(link);
        if (platform is null)
        {
            // TODO - Provide more specific feedback about supported platforms
            await Context.Interaction.RespondAsync(
                "Invalid song URL.",
                ephemeral: true
            );
            return;
        }

        var normalizedUrl = platform.Value.NormalizeUrl(link);
        if (string.IsNullOrEmpty(normalizedUrl))
        {
            logger.LogError(
                "Failed to normalize URL for {SongUrl}. Normalized URL: {NormalizedUrl}",
                link,
                normalizedUrl
            );

            await Context.Interaction.RespondAsync(
                "Failed to process the song URL. Please ensure it's a valid link from a supported platform.",
                ephemeral: true
            );
            return;
        }

        await Context.Interaction.DeferAsync(ephemeral: true);

        var songResult = await songService.GetSongAsync(normalizedUrl);
        if (songResult.IsError || songResult.Data is null)
        {
            logger.LogError(
                "Failed to get song for URL {SongUrl}: {ErrorMessage}",
                normalizedUrl,
                songResult.Message
            );
        }

        var song = songResult.Data!;

        var voteResult = await songVoteService.GetBySongIdAsync(song.Id);
        if (voteResult.IsError || voteResult.Data is null)
        {
            logger.LogError(
                "Failed to get votes for song {SongId}: {ErrorMessage}",
                song.Id,
                voteResult.Message
            );

            await Context.Interaction.FollowupAsync(
                "Failed to retrieve song information. Please try again later.",
                ephemeral: true
            );
            return;
        }

        // TODO - Improve embed formatting
        var votes = voteResult.Data!;
        var embed = new EmbedBuilder();

        var users = votes
            .OrderByDescending(v => v.Level.GetWeight())
            .Aggregate("", (current, userVote) => current + $"<@{userVote.UserId}> {userVote.Level.ToEmoji()}\n");

        embed.WithTitle(song.Title)
            .AddField("Votes", string.IsNullOrEmpty(users) ? "No votes yet" : users)
            .WithUrl(song.Url)
            .WithThumbnailUrl(song.AlbumCoverUrl);

        await Context.Interaction.FollowupAsync(embed: embed.Build(), ephemeral: true);
    }

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

        var isValidUrl = SongUtils.IsValidSongUrl(link);
        if (!isValidUrl)
        {
            // TODO - Provide more specific feedback about supported platforms
            await Context.Interaction.RespondAsync(
                "Invalid song URL.",
                ephemeral: true
            );
            return;
        }

        await Context.Interaction.DeferAsync(ephemeral: true);

        if (!await shareService.ShareToChannelAsync(guildUser, link, comment))
        {
            logger.LogError(
                "Failed to share song for user {User}. Link: {Link}. Comment: {Comment}",
                guildUser.Username,
                link,
                comment
            );

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