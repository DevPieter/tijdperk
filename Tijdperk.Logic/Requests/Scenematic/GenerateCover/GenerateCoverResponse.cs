namespace Tijdperk.Logic.Requests.Scenematic.GenerateCover;

public record GenerateCoverSuccessResult(
    string MainCoverPath,
    string DebounceCoverPath,
    string ProcessingCoverPath,
    DateTime CompletedAt
);

public record GenerateCoverFailedResult(
    string ErrorMessage,
    DateTime FailedAt
);

public record GenerateCoverCallback(
    string Url,
    string Secret,
    string Header
);

public record GenerateCoverResponse(
    Guid Id,
    int State,
    string MetaHash,
    // GenerateCoverSuccessResult? SuccessResult,
    // GenerateCoverFailedResult? FailureResult,
    GenerateCoverCallback Callback
);