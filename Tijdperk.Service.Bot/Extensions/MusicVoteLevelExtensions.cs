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

    public static string ToPastTenseDisplayName(this MusicVoteLevel level) => level switch
    {
        MusicVoteLevel.Love => "Loved",
        MusicVoteLevel.Like => "Liked",
        MusicVoteLevel.Neutral => "Neutral",
        MusicVoteLevel.Dislike => "Disliked",
        MusicVoteLevel.Trash => "Trashed",
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

    public static Color ToColor(this MusicVoteLevel level) => level switch
    {
        MusicVoteLevel.Love => new Color(0xd08ced),
        MusicVoteLevel.Like => new Color(0x87ceeb),
        MusicVoteLevel.Neutral => new Color(0x808080),
        MusicVoteLevel.Dislike => new Color(0xff6347),
        MusicVoteLevel.Trash => new Color(0x8b4513),
        _ => new Color(0x808080)
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

    public static int ToWeight(this MusicVoteLevel level) => level switch
    {
        MusicVoteLevel.Love => 2,
        MusicVoteLevel.Like => 1,
        MusicVoteLevel.Neutral => 0,
        MusicVoteLevel.Dislike => -1,
        MusicVoteLevel.Trash => -2,
        _ => 0
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