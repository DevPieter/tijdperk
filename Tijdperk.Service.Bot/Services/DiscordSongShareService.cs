using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Tijdperk.Common.Core.Enums.Songs;
using Tijdperk.Common.Core.Extensions;
using Tijdperk.Common.Data.Models.DTOs.SongVotes;
using Tijdperk.Common.Data.Models.Entities.SongVotes;
using Tijdperk.Logic.Extensions;
using Tijdperk.Logic.Services.Interfaces;
using Tijdperk.Service.Bot.Constants;
using Tijdperk.Service.Bot.Extensions;
using Tijdperk.Service.Bot.Options;
using Tijdperk.Service.Bot.Services.Interfaces;
using Tijdperk.Service.Bot.Utils;

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
    public async Task<bool> ShareToChannelAsync(SocketGuildUser user, string rawSongUrl, string? comment)
    {
        var platform = SongUtils.GetPlatform(rawSongUrl);
        if (platform is null)
        {
            logger.LogError(
                "Failed to determine platform for URL {SongUrl}.",
                rawSongUrl
            );
            return false;
        }

        var songId = platform.Value.ExtractId(rawSongUrl);
        var normalizedUrl = platform.Value.NormalizeUrl(rawSongUrl);

        if (string.IsNullOrEmpty(songId) || string.IsNullOrEmpty(normalizedUrl))
        {
            logger.LogError(
                "Failed to extract song ID or normalize URL for {SongUrl}. Extracted ID: {SongId}, Normalized URL: {NormalizedUrl}",
                rawSongUrl,
                songId,
                normalizedUrl
            );
            return false;
        }

        if (client.GetChannel(options.Value.MusicChannelId) is not ITextChannel channel)
        {
            logger.LogError(
                "Failed to find the music channel with ID {ChannelId}.",
                options.Value.MusicChannelId
            );
            return false;
        }

        var songResult = await songService.GetOrCreateSongAsync(normalizedUrl);
        if (songResult.IsError || songResult.Data is null)
        {
            logger.LogError(
                "Failed to get or create song for URL {SongUrl}: {ErrorMessage}",
                normalizedUrl,
                songResult.Message
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
                sharesResult.Message
            );
            return false;
        }

        var songEmbed = new EmbedBuilder();
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
            var voteStatsResult = await voteService.GetBySongIdAsync(song.Id);
            if (voteStatsResult.IsError || voteStatsResult.Data is null)
            {
                logger.LogError(
                    "Failed to fetch vote stats for song ID {SongId}: {ErrorMessage}",
                    song.Id,
                    voteStatsResult.Message
                );
                UpdateEmbed(songEmbed);
            }

            var votes = voteStatsResult.Data;
            UpdateEmbed(songEmbed, votes!);
        }
        else UpdateEmbed(songEmbed);

        if (!string.IsNullOrEmpty(song.AlbumCoverUrl)) songEmbed.WithImageUrl(song.AlbumCoverUrl);

        songEmbed.WithFooter(sharesResult.Data!.Count > 0
                ? $"Shared by {user.DisplayName} and {shareCount} time{(shareCount > 1 ? "s" : "")} before!"
                : $"{user.DisplayName} is the first to share this song!",
            user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()
        );

        var links = song.GetFormattedLinks();

        songEmbed.AddField("Listen on", links.Count > 0
            ? string.Join(" | ", links)
            : $"[Original Link]({song.Url})"
        );

        if (!string.IsNullOrEmpty(comment)) songEmbed.AddField("Comment", $"```{comment}```");

        var builder = new ComponentBuilder()
            .WithButton(SongVoteLevel.Love.ToButton(song.Id))
            .WithButton(SongVoteLevel.Like.ToButton(song.Id))
            .WithButton(SongVoteLevel.Neutral.ToButton(song.Id))
            .WithButton(SongVoteLevel.Dislike.ToButton(song.Id))
            .WithButton(SongVoteLevel.Trash.ToButton(song.Id))
            .WithButton("Preview", $"{SongVoteConstants.SongEmbedPrefix}{song.Id}", row: 1);

        var message = await channel.SendMessageAsync(
            embed: songEmbed.Build(),
            components: builder.Build()
        );

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
            normalizedUrl,
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

        var message = await channel.GetMessageAsync(messageId);
        if (message is not IUserMessage userMessage)
        {
            logger.LogError(
                "Failed to fetch message with ID {MessageId} for song ID {SongId}.",
                messageId,
                songId
            );
            return true;
        }

        var embed = message.Embeds.FirstOrDefault();
        if (embed is null)
        {
            logger.LogError(
                "No embed found in message ID {MessageId} for song ID {SongId}.",
                messageId,
                songId
            );
            return true;
        }

        var votesResult = await voteService.GetBySongIdAsync(songId);
        if (votesResult.IsError || votesResult.Data is null)
        {
            logger.LogError(
                "Failed to fetch votes for song ID {SongId}: {ErrorMessage}",
                songId,
                votesResult.Message
            );
            return true;
        }

        var newEmbed = embed.ToEmbedBuilder();
        UpdateEmbed(newEmbed, votesResult.Data);

        await userMessage.ModifyAsync(m => m.Embed = newEmbed.Build());
        return true;
    }

    private static void UpdateEmbed(EmbedBuilder embed, List<SongVoteEntity>? votes = null)
    {
        votes ??= [];

        var levels = votes.Select(v => v.Level).ToList();
        var winningLevel = levels.CalculateLevel();

        embed.WithColor(winningLevel.ToColor());
        embed.WithAuthor(votes.Count > 0
            ? $"{winningLevel.ToEmoji()} {winningLevel.ToPastTenseDisplayName()}"
            : "🫣 No votes yet"
        );

        var loveCount = votes.Count(v => v.Level == SongVoteLevel.Love);
        var likeCount = votes.Count(v => v.Level == SongVoteLevel.Like);
        var neutralCount = votes.Count(v => v.Level == SongVoteLevel.Neutral);
        var dislikeCount = votes.Count(v => v.Level == SongVoteLevel.Dislike);
        var trashCount = votes.Count(v => v.Level == SongVoteLevel.Trash);

        embed.WithDescription(
            $"`{loveCount} 🔥` | `{likeCount} 👍` | `{neutralCount} 😐` | `{dislikeCount} 👎` | `{trashCount} 💩`"
        );
    }
}