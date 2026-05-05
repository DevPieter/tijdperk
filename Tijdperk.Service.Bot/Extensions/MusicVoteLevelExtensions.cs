using Discord;
using Tijdperk.Service.Bot.Constants;
using Tijdperk.Service.Bot.Enums;

namespace Tijdperk.Service.Bot.Extensions;

public static class MusicVoteLevelExtensions
{
    public static string ToDisplayName(this MusicVoteLevel level) => level switch
    {
        MusicVoteLevel.Love => "Love",
        MusicVoteLevel.Like => "Like",
        MusicVoteLevel.Neutral => "Neutral",
        MusicVoteLevel.Dislike => "Dislike",
        MusicVoteLevel.Trash => "Trash",
        _ => ""
    };

    public static string ToEmoji(this MusicVoteLevel level) => level switch
    {
        MusicVoteLevel.Love => "🔥",
        MusicVoteLevel.Like => "👍",
        MusicVoteLevel.Neutral => "😐",
        MusicVoteLevel.Dislike => "👎",
        MusicVoteLevel.Trash => "💩",
        _ => ""
    };

    public static string ToCustomId(this MusicVoteLevel level) => level switch
    {
        MusicVoteLevel.Love => MusicVoteConstants.CustomIdPrefix + "love",
        MusicVoteLevel.Like => MusicVoteConstants.CustomIdPrefix + "like",
        MusicVoteLevel.Neutral => MusicVoteConstants.CustomIdPrefix + "neutral",
        MusicVoteLevel.Dislike => MusicVoteConstants.CustomIdPrefix + "dislike",
        MusicVoteLevel.Trash => MusicVoteConstants.CustomIdPrefix + "trash",
        _ => ""
    };

    public static MusicVoteLevel? ToMusicVoteLevel(this string emoji) => emoji switch
    {
        "🔥" or "love" => MusicVoteLevel.Love,
        "👍" or "like" => MusicVoteLevel.Like,
        "😐" or "neutral" => MusicVoteLevel.Neutral,
        "👎" or "dislike" => MusicVoteLevel.Dislike,
        "💩" or "trash" => MusicVoteLevel.Trash,
        _ => null
    };

    public static ButtonBuilder ToButton(this MusicVoteLevel level)
    {
        var label = level.ToDisplayName();
        var emoji = new Emoji(level.ToEmoji());
        var customId = level.ToCustomId();

        var style = level switch
        {
            MusicVoteLevel.Love => ButtonStyle.Success,
            MusicVoteLevel.Like => ButtonStyle.Primary,
            MusicVoteLevel.Neutral => ButtonStyle.Secondary,
            MusicVoteLevel.Dislike => ButtonStyle.Danger,
            MusicVoteLevel.Trash => ButtonStyle.Danger,
            _ => ButtonStyle.Secondary
        };

        return new ButtonBuilder(label, customId, style, null, emoji);
    }
}