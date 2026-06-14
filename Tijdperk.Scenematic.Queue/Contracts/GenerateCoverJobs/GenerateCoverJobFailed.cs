namespace Tijdperk.Scenematic.Queue.Contracts.GenerateCoverJobs;

public record GenerateCoverJobFailed(
    Guid JobId,
    string Reason,
    string Message
);