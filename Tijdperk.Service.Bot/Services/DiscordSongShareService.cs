using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Tijdperk.Common.Core.Enums.Songs;
using Tijdperk.Common.Data.Models.DTOs.SongVotes;
using Tijdperk.Logic.Services.Interfaces;
using Tijdperk.Service.Bot.Enums;
using Tijdperk.Service.Bot.Extensions;
using Tijdperk.Service.Bot.Options;
using Tijdperk.Service.Bot.Services.Interfaces;

namespace Tijdperk.Service.Bot.Services;

public class DiscordSongShareService(
    ILogger<DiscordSongShareService> logger,
    IOptions<GuildOptions> options,
    DiscordSocketClient client,
    ISongService songService,
    ISongShareService shareService,
    ISongVoteService voteService
) : IDiscordSongShareService
{
    public async Task<bool> ShareToChannelAsync(SocketGuildUser user, string songUrl, string? comment)
    {
        if (client.GetChannel(options.Value.MusicChannelId) is not ITextChannel channel)
        {
            logger.LogError(
                "Failed to find the music channel with ID {ChannelId}.",
                options.Value.MusicChannelId
            );
            return false;
        }

        var songResult = await songService.GetOrCreateSongAsync(songUrl);
        if (songResult.IsError || songResult.Data is null)
        {
            logger.LogError(
                "Failed to get or create song for URL {SongUrl}: {ErrorMessage}",
                songUrl,
                songResult?.Message ?? "Unknown error"
            );
            return false;
        }

        var song = songResult.Data!;

        var sharesResult = await shareService.GetBySongIdAsync(song.Id);
        if (sharesResult.IsError || sharesResult.Data is null)
        {
            logger.LogError(
                "Failed to fetch shares for song ID {SongId}: {ErrorMessage}",
                song.Id,
                sharesResult?.Message ?? "Unknown error"
            );
            return false;
        }

        var songEmbed = new EmbedBuilder();
        songEmbed.WithColor(new Color(0x808080));
        songEmbed.WithUrl(song.OdesliUrl ?? song.Url);

        songEmbed.WithTitle(string.IsNullOrEmpty(song.Title)
            ? song.Url
            : string.IsNullOrEmpty(song.Artist)
                ? song.Title
                : $"{song.Title} - {song.Artist}"
        );

        var shareCount = sharesResult.Data.Count;
        if (shareCount > 0)
        {
            // TODO - Fetch actual stats
            songEmbed.WithDescription("`0 🔥` | `0 👍` | `0 😐` | `0 👎` | `0 💩`");
        }
        else songEmbed.WithDescription("`0 🔥` | `0 👍` | `0 😐` | `0 👎` | `0 💩`");

        if (!string.IsNullOrEmpty(song.AlbumCoverUrl)) songEmbed.WithImageUrl(song.AlbumCoverUrl);

        songEmbed.WithFooter(sharesResult.Data!.Count > 0
                ? $"Shared by {user.DisplayName} and {shareCount} time{(shareCount > 1 ? "s" : "")} before!"
                : $"{user.DisplayName} is the first to share this song!",
            user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()
        );

        // TODO - Place links

        if (!string.IsNullOrEmpty(comment)) songEmbed.AddField("Comment", $"```{comment}```");

        var builder = new ComponentBuilder()
            .WithButton(SongVoteLevel.Love.ToButton(song.Id))
            .WithButton(SongVoteLevel.Like.ToButton(song.Id))
            .WithButton(SongVoteLevel.Neutral.ToButton(song.Id))
            .WithButton(SongVoteLevel.Dislike.ToButton(song.Id))
            .WithButton(SongVoteLevel.Trash.ToButton(song.Id));

        var message = await channel.SendMessageAsync(embed: songEmbed.Build(), components: builder.Build());

        var registerResult = await shareService.RegisterShareAsync(
            song: song,
            userId: user.Id,
            messageId: message.Id,
            comment: comment
        );

        if (registerResult.IsSuccess)
        {
            var messageIds = sharesResult.Data!
                .Where(s => s.MessageId is not null)
                .Select(s => s.MessageId!.Value)
                .ToList();

            await channel.DeleteMessagesAsync(messageIds);
            await shareService.ClearMessageIdsAsync(messageIds);
            return true;
        }

        logger.LogError(
            "Failed to register song share for user {UserId} and song {SongUrl}: {ErrorMessage}",
            user.Id,
            songUrl,
            registerResult.Message
        );

        await message.ModifyAsync(m =>
        {
            m.Components = null;

            var embed = message.Embeds.FirstOrDefault();
            if (embed is null) return;

            var newEmbed = embed.ToEmbedBuilder()
                .WithColor(Color.Red)
                .WithDescription("Failed to register, rating unavailable")
                .WithAuthor("💀 Registration Failed");
            m.Embed = newEmbed.Build();
        });

        return false;
    }

    // var odesli = await odesliService.GetOdesliAsync(songUrl);
    // if (odesli is not null)
    // {
    //     songEmbed.WithUrl(odesli.PageUrl);
    //
    //     var meta = odesli.GetSongMeta();
    //     if (meta is not null)
    //     {
    //         songEmbed.WithTitle(string.IsNullOrEmpty(meta.ArtistName)
    //             ? meta.Title
    //             : $"{meta.Title} - {meta.ArtistName}"
    //         );
    //
    //         if (!string.IsNullOrEmpty(meta.ThumbnailUrl)) songEmbed.WithImageUrl(meta.ThumbnailUrl);
    //     }
    //
    //     var links = odesli.GetFormattedLinks();
    //     songEmbed.AddField("Listen on", links.Count > 0
    //         ? string.Join(" | ", links)
    //         : $"[Odesli]({odesli.PageUrl}) | [Original Link]({songUrl})"
    //     );
    // }

    public async Task<bool> RemoveShareAsync(SocketGuildUser user, ulong messageId)
    {
        throw new NotImplementedException();
        // // TODO - It may be better to use custom IDs instead of relying on the message ID
        // if (!SharedSongs.TryGetValue(messageId, out var song)) return false;
        // if (song.UserId != user.Id) return false;
        //
        // if (client.GetChannel(options.Value.MusicChannelId) is not IMessageChannel channel) return false;
        // if (await channel.GetMessageAsync(messageId) is not IUserMessage message) return false;
        //
        // await message.DeleteAsync();
        // SharedSongs.Remove(messageId);
        //
        // var votesToRemove = Votes.Keys.Where(k => k.MessageId == messageId).ToList();
        // foreach (var key in votesToRemove) Votes.Remove(key);
        //
        // return true;
    }

    public async Task<bool> CastVoteAsync(SocketGuildUser user, Guid songId, SongVoteLevel level)
    {
        var voteResult = await voteService.CreateOrUpdateAsync(songId, new CreateSongVoteDto(
            UserId: user.Id,
            Level: level
        ));

        if (voteResult.IsError)
        {
            logger.LogError(
                "Failed to cast vote for user {UserId} and song ID {SongId}: {ErrorMessage}",
                user.Id,
                songId,
                voteResult.Message
            );
            return false;
        }

        if (client.GetChannel(options.Value.MusicChannelId) is not ITextChannel channel)
        {
            logger.LogError(
                "Failed to find the music channel with ID {ChannelId}.",
                options.Value.MusicChannelId
            );
            return true;
        }

        var messageIdsResult = await shareService.GetLatestMessageIdBySongIdAsync(songId);
        if (messageIdsResult.IsError)
        {
            logger.LogError(
                "Failed to fetch message ID for song ID {SongId}: {ErrorMessage}",
                songId,
                messageIdsResult?.Message
            );
            return true;
        }

        var messageId = messageIdsResult.Data;
        if (messageId == 0)
        {
            logger.LogWarning(
                "No message ID found for song ID {SongId}, cannot update vote stats.",
                songId
            );
            return true;
        }

        logger.LogInformation(
            "User {UserId} cast a {VoteLevel} vote for song ID {SongId} (message ID {MessageId}).",
            user.Id,
            level,
            songId,
            messageId
        );

        return true;
    }

    // private async Task UpdateMessageStatsAsync(ulong messageId)
    // {
    //     if (!SharedSongs.ContainsKey(messageId)) return;
    //
    //     if (client.GetChannel(options.Value.MusicChannelId) is not IMessageChannel channel) return;
    //     if (await channel.GetMessageAsync(messageId) is not IUserMessage message) return;
    //
    //     var embed = message.Embeds.FirstOrDefault();
    //     if (embed == null) return;
    //
    //     var newEmbed = embed.ToEmbedBuilder();
    //     var votes = Votes.Values.Where(v => v.MessageId == messageId).ToList();
    //
    //     var winningLevel = CalculateWinningLevel(votes);
    //     newEmbed.WithColor(winningLevel.ToColor());
    //     newEmbed.WithAuthor($"{winningLevel.ToEmoji()} {winningLevel.ToPastTenseDisplayName()}");
    //
    //     var loveCount = votes.Count(v => v.Level == MusicVoteLevel.Love);
    //     var likeCount = votes.Count(v => v.Level == MusicVoteLevel.Like);
    //     var neutralCount = votes.Count(v => v.Level == MusicVoteLevel.Neutral);
    //     var dislikeCount = votes.Count(v => v.Level == MusicVoteLevel.Dislike);
    //     var trashCount = votes.Count(v => v.Level == MusicVoteLevel.Trash);
    //
    //     newEmbed.WithDescription(
    //         $"`{loveCount} 🔥` | `{likeCount} 👍` | `{neutralCount} 😐` | `{dislikeCount} 👎` | `{trashCount} 💩`"
    //     );
    //     await message.ModifyAsync(m => m.Embed = newEmbed.Build());
    // }
    //
    // private MusicVoteLevel CalculateWinningLevel(List<VoteHolder> votes)
    // {
    //     if (votes.Count == 0) return MusicVoteLevel.Neutral;
    //     var averageScore = votes.Average(v => v.Level.ToWeight());
    //
    //     return averageScore switch
    //     {
    //         >= 1.5 => MusicVoteLevel.Love,
    //         >= 0.5 => MusicVoteLevel.Like,
    //         > -0.5 => MusicVoteLevel.Neutral,
    //         > -1.5 => MusicVoteLevel.Dislike,
    //         _ => MusicVoteLevel.Trash
    //     };
    // }
}