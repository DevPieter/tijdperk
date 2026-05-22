using Tijdperk.Common.Core.Enums.Songs;

namespace Tijdperk.Service.Bot;

public record SongHolder(
    string name,
    string author,
    string url,
    SongVoteLevel voteLevel
);