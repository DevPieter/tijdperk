namespace Tijdperk.Common.Core.Enums.Common;

public enum ResultType
{
    /**
     * The action was successful.
     * HTTP: 200 OK
     */
    Success,

    /**
     * An unexpected error occurred on the server.
     * HTTP: 500 Internal Server Error
     */
    InternalError,

    /**
     * The request was invalid or cannot be processed.
     * HTTP: 400 Bad Request
     */
    Validation,

    /**
    * The requested resource was not found.
    * HTTP: 404 Not Found
    */
    NotFound,

    /**
     * The user/entity is not authenticated and cannot perform this action.
     * HTTP: 401 Unauthorized
     */
    Unauthorized,

    /**
     * The user/entity is authenticated but does not have permission to perform this action.
     * HTTP: 403 Forbidden
     */
    Forbidden,

    /**
    * The request could not be completed due to a conflict with the current state of the resource.
    * HTTP: 409 Conflict
    */
    Conflict,

    /**
    * The request was invalid or cannot be processed.
    * HTTP: 400 Bad Request
    */
    BadRequest
}