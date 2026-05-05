using System.Text.Json.Serialization;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Tijdperk.Service.Bot.Enums;
using Tijdperk.Service.Bot.Extensions;
using Tijdperk.Service.Bot.Models.Odesli;
using Tijdperk.Service.Bot.Options;
using Tijdperk.Service.Bot.Services.Interfaces;

namespace Tijdperk.Service.Bot.Services;

public class MusicShareService(
    ILogger<MusicShareService> logger,
    IOptions<GuildOptions> options,
    DiscordSocketClient client,
    IOdesliService odesliService
) : IMusicShareService
{
    private static Dictionary<ulong, SharedSongHolder> SharedSongs { get; } = new();
    private static Dictionary<VoteKey, VoteHolder> Votes { get; } = new();

    public record SharedSongHolder(
        ulong UserId,
        ulong MessageId,
        string Song
    );

    public record VoteKey(
        ulong UserId,
        ulong MessageId
    );

    public record VoteHolder(
        ulong UserId,
        ulong MessageId,
        MusicVoteLevel Level
    );

    public async Task<bool> ShareAsync(SocketGuildUser user, string songUrl)
    {
        if (client.GetChannel(options.Value.MusicChannelId) is not IMessageChannel channel)
        {
            logger.LogError(
                "Failed to find the music channel with ID {ChannelId}.",
                options.Value.MusicChannelId
            );
            return false;
        }

        var songEmbed = new EmbedBuilder()
            .WithTitle(songUrl)
            .WithUrl(songUrl)
            .WithDescription("`0 🔥` | `0 👍` | `0 😐` | `0 👎` | `0 💩`")
            .WithColor(new Color(0x808080));

        songEmbed.WithFooter($"Shared by {user.DisplayName}", user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());

        var odesli = await odesliService.GetOdesliAsync(songUrl);
        if (odesli is not null)
        {
            songEmbed.WithUrl(odesli.PageUrl);

            var meta = odesli.GetSongMeta();
            if (meta is not null)
            {
                songEmbed.WithTitle(string.IsNullOrEmpty(meta.ArtistName)
                    ? meta.Title
                    : $"{meta.Title} - {meta.ArtistName}"
                );

                if (!string.IsNullOrEmpty(meta.ThumbnailUrl)) songEmbed.WithImageUrl(meta.ThumbnailUrl);
            }

            var links = odesli.GetFormattedLinks();
            songEmbed.AddField("Listen on", links.Count > 0
                ? string.Join(" | ", links)
                : $"[Odesli]({odesli.PageUrl}) | [Original Link]({songUrl})"
            );
        }

        var builder = new ComponentBuilder()
            .WithButton(MusicVoteLevel.Love.ToButton())
            .WithButton(MusicVoteLevel.Like.ToButton())
            .WithButton(MusicVoteLevel.Neutral.ToButton())
            .WithButton(MusicVoteLevel.Dislike.ToButton())
            .WithButton(MusicVoteLevel.Trash.ToButton());

        var message = await channel.SendMessageAsync(embed: songEmbed.Build(), components: builder.Build());
        SharedSongs[message.Id] = new SharedSongHolder(user.Id, message.Id, songUrl);

        return true;
    }

    public async Task<bool> CastVoteAsync(SocketGuildUser user, SocketMessage message, MusicVoteLevel level)
    {
        if (!SharedSongs.ContainsKey(message.Id)) return false;

        var voteKey = new VoteKey(user.Id, message.Id);
        Votes[voteKey] = new VoteHolder(user.Id, message.Id, level);

        await UpdateMessageStatsAsync(message.Id);
        return true;
    }

    private async Task UpdateMessageStatsAsync(ulong messageId)
    {
        if (!SharedSongs.ContainsKey(messageId)) return;

        if (client.GetChannel(options.Value.MusicChannelId) is not IMessageChannel channel) return;
        if (await channel.GetMessageAsync(messageId) is not IUserMessage message) return;

        var embed = message.Embeds.FirstOrDefault();
        if (embed == null) return;

        var newEmbed = embed.ToEmbedBuilder();

        var votesForMessage = Votes.Values.Where(v => v.MessageId == messageId).ToList();
        var loveCount = votesForMessage.Count(v => v.Level == MusicVoteLevel.Love);
        var likeCount = votesForMessage.Count(v => v.Level == MusicVoteLevel.Like);
        var neutralCount = votesForMessage.Count(v => v.Level == MusicVoteLevel.Neutral);
        var dislikeCount = votesForMessage.Count(v => v.Level == MusicVoteLevel.Dislike);
        var trashCount = votesForMessage.Count(v => v.Level == MusicVoteLevel.Trash);

        var winningLevel = votesForMessage
            .GroupBy(v => v.Level)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key ?? MusicVoteLevel.Neutral;

        newEmbed.WithColor(winningLevel.ToColor());
        newEmbed.WithAuthor($"{winningLevel.ToEmoji()} {winningLevel.ToPastTenseDisplayName()}");
        newEmbed.WithDescription($"`{loveCount} 🔥` | `{likeCount} 👍` | `{neutralCount} 😐` | `{dislikeCount} 👎` | `{trashCount} 💩`");

        await message.ModifyAsync(m => m.Embed = newEmbed.Build());
    }
}