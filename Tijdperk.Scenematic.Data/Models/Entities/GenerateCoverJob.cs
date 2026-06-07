using System.ComponentModel.DataAnnotations;
using Tijdperk.Scenematic.Common.Enums;

namespace Tijdperk.Scenematic.Data.Models.Entities;

public class GenerateCoverJob
{
    public Guid Id { get; init; }

    [Required]
    public string AlbumTitle { get; init; } = null!;

    [Required]
    public string AlbumArtist { get; init; } = null!;

    [Required]
    public string AlbumCoverImageUrl { get; init; } = null!;

    [Required]
    public GenerateCoverJobState State { get; set; }

    public GenerateCoverJobFailureReason? FailureReason { get; set; }

    [MaxLength(4000)]
    public string? ErrorMessage { get; set; }

    [Required]
    public DateTime CreatedAt { get; init; }

    public DateTime? QueuedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? FailedAt { get; set; }
}