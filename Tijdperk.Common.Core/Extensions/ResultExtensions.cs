using Tijdperk.Common.Core.Enums.Common;
using Tijdperk.Common.Core.Models.Common;

namespace Tijdperk.Common.Core.Extensions;

public static class ResultExtensions
{
    public static Result ToResult<T>(this ResultObject<T> result) => result.Type switch
    {
        ResultType.Success => Result.Success(),
        ResultType.Forbidden => Result.Forbidden(result.Message),
        ResultType.InternalError => Result.InternalError(result.Message),
        ResultType.Validation => Result.Validation(result.Message),
        ResultType.NotFound => Result.NotFound(result.Message),
        ResultType.Conflict => Result.Conflict(result.Message),
        ResultType.BadRequest => Result.BadRequest(result.Message),
        _ => Result.BadRequest()
    };

    public static ResultObject<TR> Clone<T, TR>(this ResultObject<T> result)
        => new(result.Type, default, result.Message);

    public static ResultObject<TR> Clone<T, TR>(this ResultObject<T> result, TR data)
        => new(result.Type, data, result.Message);
}