using Tijdperk.Scenematic.Common.Enums;

namespace Tijdperk.Scenematic.Data.Models.DTOs.Covers;

public class UpdateGenerateCoverDto(
    GenerateCoverJobState state,
    DateTime? queuedAt,
    DateTime? startedAt,
    DateTime? completedAt,
    DateTime? failedAt
)
{
    public GenerateCoverJobState State { get; set; } = state;
    public DateTime? QueuedAt { get; set; } = queuedAt;
    public DateTime? StartedAt { get; set; } = startedAt;
    public DateTime? CompletedAt { get; set; } = completedAt;
    public DateTime? FailedAt { get; set; } = failedAt;
}