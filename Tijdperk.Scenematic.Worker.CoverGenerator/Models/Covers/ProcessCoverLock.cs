using Tijdperk.Scenematic.Worker.CoverGenerator.Enums;

namespace Tijdperk.Scenematic.Worker.CoverGenerator.Models.Covers;

public class ProcessCoverLock
{
    public Guid JobId { get; init; }

    public ProcessCoverStage Stage { get; set; }
    public string? ExtraInfo { get; set; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }
}