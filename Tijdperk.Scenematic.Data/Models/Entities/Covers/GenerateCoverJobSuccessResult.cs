using System.ComponentModel.DataAnnotations;

namespace Tijdperk.Scenematic.Data.Models.Entities.Covers;

public class GenerateCoverJobSuccessResult
{
    [Required]
    public Guid JobId { get; init; }

    [Required]
    public string MainCoverPath { get; init; } = null!;

    [Required]
    public string DebounceCoverPath { get; init; } = null!;

    [Required]
    public string ProcessingCoverPath { get; init; } = null!;
}