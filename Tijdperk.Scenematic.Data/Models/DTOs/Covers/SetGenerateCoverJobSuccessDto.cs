namespace Tijdperk.Scenematic.Data.Models.DTOs.Covers;

public record SetGenerateCoverJobSuccessDto(
    string MainCoverPath,
    string DebounceCoverPath,
    string ProcessingCoverPath
);