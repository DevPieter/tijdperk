using Tijdperk.Scenematic.Common.Enums;

namespace Tijdperk.Scenematic.Data.Models.DTOs.Covers;

public record SetGenerateCoverJobFailureDto(
    GenerateCoverJobFailureReason FailureReason,
    string? ErrorMessage
);