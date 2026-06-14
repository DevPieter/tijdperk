using System.ComponentModel.DataAnnotations;
using Tijdperk.Scenematic.Common.Enums;

namespace Tijdperk.Scenematic.Data.Models.Entities.Covers;

public class GenerateCoverJobFailureResult
{
    [Required]
    public Guid JobId { get; init; }

    [Required]
    public GenerateCoverJobFailureReason FailureReason { get; init; }

    [MaxLength(4000)]
    public string? ErrorMessage { get; init; }
}