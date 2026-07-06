namespace Tijdperk.Logic.Requests.Scenematic.GenerateCover;

public record GenerateCoverCallbackRequest(
    Guid JobId,
    string MainCoverPath,
    string DebounceCoverPath,
    string ProcessingCoverPath
);