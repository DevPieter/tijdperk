using Tijdperk.Common.Core.Enums.Common;

namespace Tijdperk.Common.Core.Models.Common;

public record ResultObject<T>(ResultType Type, T? Data, string? Message)
{
    public static ResultObject<T> Success(T? data, string? message = null) => new(ResultType.Success, data, message);
    public static ResultObject<T> Success(string? message = null) => new(ResultType.Success, default, message);

    public static ResultObject<T> InternalError(T? data, string? message = null) => new(ResultType.InternalError, data, message);
    public static ResultObject<T> InternalError(string? message = null) => new(ResultType.InternalError, default, message);

    public static ResultObject<T> Validation(T? data, string? message = null) => new(ResultType.Validation, data, message);
    public static ResultObject<T> Validation(string? message = null) => new(ResultType.Validation, default, message);

    public static ResultObject<T> NotFound(T? data, string? message = null) => new(ResultType.NotFound, data, message);
    public static ResultObject<T> NotFound(string? message = null) => new(ResultType.NotFound, default, message);

    public static ResultObject<T> Unauthorized(T? data, string? message = null) => new(ResultType.Unauthorized, data, message);
    public static ResultObject<T> Unauthorized(string? message = null) => new(ResultType.Unauthorized, default, message);

    public static ResultObject<T> Forbidden(T? data, string? message = null) => new(ResultType.Forbidden, data, message);
    public static ResultObject<T> Forbidden(string? message = null) => new(ResultType.Forbidden, default, message);

    public static ResultObject<T> Conflict(T? data, string? message = null) => new(ResultType.Conflict, data, message);
    public static ResultObject<T> Conflict(string? message = null) => new(ResultType.Conflict, default, message);

    public static ResultObject<T> BadRequest(T? data, string? message = null) => new(ResultType.BadRequest, data, message);
    public static ResultObject<T> BadRequest(string? message = null) => new(ResultType.BadRequest, default, message);

    public bool IsSuccess => Type == ResultType.Success;
    public bool IsError => Type != ResultType.Success;
}