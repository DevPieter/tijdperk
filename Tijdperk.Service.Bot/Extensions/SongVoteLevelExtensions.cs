using Discord;
using Tijdperk.Common.Core.Enums.Songs;
using Tijdperk.Service.Bot.Constants;

namespace Tijdperk.Service.Bot.Extensions;

public static class SongVoteLevelExtensions
{
    public static string ToDisplayName(this SongVoteLevel level) => level switch
    {
        SongVoteLevel.Love => "Love",
        SongVoteLevel.Like => "Like",
        SongVoteLevel.Neutral => "Neutral",
        SongVoteLevel.Dislike => "Dislike",
        SongVoteLevel.Trash => "Trash",
        _ => ""
    };

    public static string ToPastTenseDisplayName(this SongVoteLevel level) => level switch
    {
        SongVoteLevel.Love => "Loved",
        SongVoteLevel.Like => "Liked",
        SongVoteLevel.Neutral => "Neutral",
        SongVoteLevel.Dislike => "Disliked",
        SongVoteLevel.Trash => "Trashed",
        _ => ""
    };

    public static string ToEmoji(this SongVoteLevel level) => level switch
    {
        SongVoteLevel.Love => "🔥",
        SongVoteLevel.Like => "👍",
        SongVoteLevel.Neutral => "😐",
        SongVoteLevel.Dislike => "👎",
        SongVoteLevel.Trash => "💩",
        _ => ""
    };

    public static Color ToColor(this SongVoteLevel level) => level switch
    {
        SongVoteLevel.Love => new Color(0xd08ced),
        SongVoteLevel.Like => new Color(0x87ceeb),
        SongVoteLevel.Neutral => new Color(0x808080),
        SongVoteLevel.Dislike => new Color(0xff6347),
        SongVoteLevel.Trash => new Color(0x8b4513),
        _ => new Color(0x808080)
    };

    public static string ToCustomId(this SongVoteLevel level) => level switch
    {
        SongVoteLevel.Love => SongVoteConstants.CustomIdPrefix + "love",
        SongVoteLevel.Like => SongVoteConstants.CustomIdPrefix + "like",
        SongVoteLevel.Neutral => SongVoteConstants.CustomIdPrefix + "neutral",
        SongVoteLevel.Dislike => SongVoteConstants.CustomIdPrefix + "dislike",
        SongVoteLevel.Trash => SongVoteConstants.CustomIdPrefix + "trash",
        _ => ""
    };

    public static int ToWeight(this SongVoteLevel level) => level switch
    {
        SongVoteLevel.Love => 2,
        SongVoteLevel.Like => 1,
        SongVoteLevel.Neutral => 0,
        SongVoteLevel.Dislike => -1,
        SongVoteLevel.Trash => -2,
        _ => 0
    };

    public static SongVoteLevel? ToSongVoteLevel(this string emoji) => emoji switch
    {
        "🔥" or "love" => SongVoteLevel.Love,
        "👍" or "like" => SongVoteLevel.Like,
        "😐" or "neutral" => SongVoteLevel.Neutral,
        "👎" or "dislike" => SongVoteLevel.Dislike,
        "💩" or "trash" => SongVoteLevel.Trash,
        _ => null
    };

    public static ButtonBuilder ToButton(this SongVoteLevel level, Guid songId)
    {
        var label = level.ToDisplayName();
        var emoji = new Emoji(level.ToEmoji());
        var customId = $"{level.ToCustomId()}_{songId}";

        var style = level switch
        {
            SongVoteLevel.Love => ButtonStyle.Success,
            SongVoteLevel.Like => ButtonStyle.Primary,
            SongVoteLevel.Neutral => ButtonStyle.Secondary,
            SongVoteLevel.Dislike => ButtonStyle.Danger,
            SongVoteLevel.Trash => ButtonStyle.Danger,
            _ => ButtonStyle.Secondary
        };

        return new ButtonBuilder(label, customId, style, null, emoji);
    }
}