using Tijdperk.Scenematic.Common.Enums;

namespace Tijdperk.Scenematic.Data.Models.DTOs;

// public record UpdateGenerateCoverDto(
//     GenerateCoverJobState State,
//     GenerateCoverJobFailureReason? FailureReason,
//     string? ErrorMessage,
//     DateTime? QueuedAt,
//     DateTime? StartedAt,
//     DateTime? CompletedAt,
//     DateTime? FailedAt
// );

public class UpdateGenerateCoverDto(
    GenerateCoverJobState state,
    GenerateCoverJobFailureReason? failureReason,
    string? errorMessage,
    DateTime? queuedAt,
    DateTime? startedAt,
    DateTime? completedAt,
    DateTime? failedAt)
{
    public GenerateCoverJobState State { get; set; } = state;
    public GenerateCoverJobFailureReason? FailureReason { get; set; } = failureReason;
    public string? ErrorMessage { get; set; } = errorMessage;
    public DateTime? QueuedAt { get; set; } = queuedAt;
    public DateTime? StartedAt { get; set; } = startedAt;
    public DateTime? CompletedAt { get; set; } = completedAt;
    public DateTime? FailedAt { get; set; } = failedAt;
}