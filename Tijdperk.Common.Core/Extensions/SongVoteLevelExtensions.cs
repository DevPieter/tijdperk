using Tijdperk.Common.Core.Enums.Songs;

namespace Tijdperk.Common.Core.Extensions;

public static class SongVoteLevelExtensions
{
    public static int GetWeight(this SongVoteLevel level) => level switch
    {
        SongVoteLevel.Love => 2,
        SongVoteLevel.Like => 1,
        SongVoteLevel.Neutral => 0,
        SongVoteLevel.Dislike => -1,
        SongVoteLevel.Trash => -2,
        _ => 0
    };

    public static SongVoteLevel CalculateLevel(this List<SongVoteLevel> levels)
    {
        if (levels.Count == 0) return SongVoteLevel.Neutral;
        var averageScore = levels.Average(l => l.GetWeight());

        return averageScore switch
        {
            >= 1.5 => SongVoteLevel.Love,
            >= 0.5 => SongVoteLevel.Like,
            > -0.5 => SongVoteLevel.Neutral,
            > -1.5 => SongVoteLevel.Dislike,
            _ => SongVoteLevel.Trash
        };
    }
}