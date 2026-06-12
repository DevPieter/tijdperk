using Tijdperk.Common.Core.Enums.Common;
using Tijdperk.Common.Core.Models.Common;
using Tijdperk.Scenematic.Common.Enums;

namespace Tijdperk.Scenematic.Worker.CoverGenerator.Models.Covers;

public record ProcessCoverResult(
    ResultType Type,
    GenerateCoverJobFailureReason FailureReason,
    string? Message,
    string? MainCoverPath,
    string? DebounceCoverPath,
    string? ProcessingCoverPath
) : Result(Type, Message)
{
    public static ProcessCoverResult Success(
        string mainCoverPath,
        string debounceCoverPath,
        string processingCoverPath
    ) => new(
        ResultType.Success,
        GenerateCoverJobFailureReason.None,
        "Successfully processed cover",
        mainCoverPath,
        debounceCoverPath,
        processingCoverPath
    );

    public static ProcessCoverResult Failure(
        GenerateCoverJobFailureReason failureReason,
        string message
    ) => new(
        ResultType.InternalError,
        failureReason,
        message,
        null,
        null,
        null
    );
}