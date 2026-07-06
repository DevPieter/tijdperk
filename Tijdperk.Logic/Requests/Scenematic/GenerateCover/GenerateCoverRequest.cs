using Tijdperk.Logic.Models.Scenematic;
using Tijdperk.Logic.Models.Scenematic.GenerateCover;

namespace Tijdperk.Logic.Requests.Scenematic.GenerateCover;

public record GenerateCoverRequest(
    string Title,
    string Artist,
    string CoverImageUrl,
    string CallbackUrl,
    List<GenerateCoverVote> CoverVotes
);