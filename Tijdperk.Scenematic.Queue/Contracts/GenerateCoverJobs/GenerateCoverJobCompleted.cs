namespace Tijdperk.Scenematic.Queue.Contracts.GenerateCoverJobs;

public record GenerateCoverJobCompleted(
    Guid JobId,
    string MainCoverPath,
    string DebounceCoverPath,
    string ProcessingCoverPath
);