namespace Tijdperk.Logic.Models.Scenematic.GenerateCover;

public record GenerateCoverArgs(
    string Title,
    string Artist,
    string CoverImageUrl,
    List<GenerateCoverVote> CoverVotes
);