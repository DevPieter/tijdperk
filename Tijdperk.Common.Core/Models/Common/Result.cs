using Tijdperk.Common.Core.Enums.Common;

namespace Tijdperk.Common.Core.Models.Common;

public record Result(ResultType Type, string? Message)
{
    public static Result Success(string? message = null) => new(ResultType.Success, message);
    public static Result InternalError(string? message = null) => new(ResultType.InternalError, message);
    public static Result Validation(string? message = null) => new(ResultType.Validation, message);
    public static Result NotFound(string? message = null) => new(ResultType.NotFound, message);
    public static Result Unauthorized(string? message = null) => new(ResultType.Unauthorized, message);
    public static Result Forbidden(string? message = null) => new(ResultType.Forbidden, message);
    public static Result Conflict(string? message = null) => new(ResultType.Conflict, message);
    public static Result BadRequest(string? message = null) => new(ResultType.BadRequest, message);

    public bool IsSuccess => Type == ResultType.Success;
    public bool IsError => Type != ResultType.Success;
}