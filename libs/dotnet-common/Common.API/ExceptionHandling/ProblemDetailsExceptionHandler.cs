using Common.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NotFoundException = Common.Domain.Exceptions.NotFoundException;

namespace Common.API.ExceptionHandling;

/// <summary>
///     Converts well-known application exceptions into RFC 9110-compliant <see cref="ProblemDetails" /> responses.
/// </summary>
public class ProblemDetailsExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        (int statusCode, ProblemDetails? problemDetails) = exception switch
        {
            ValidationException ve => (StatusCodes.Status400BadRequest,
                new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    Detail = "One or more validation errors occurred.",
                    Extensions =
                    {
                        ["errors"] = ve.Errors
                            .GroupBy(x => x.PropertyName)
                            .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray())
                    }
                }),
            NotFoundException ne => (StatusCodes.Status404NotFound,
                new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "The specified resource was not found.",
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                    Detail = ne.Message
                }),
            UnauthorizedAccessException ue => (StatusCodes.Status401Unauthorized,
                new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Unauthorized",
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                    Detail = string.IsNullOrWhiteSpace(ue.Message)
                        ? "Authentication is required to access this resource."
                        : ue.Message
                }),
            ConflictException ce => (StatusCodes.Status409Conflict,
                new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Conflict",
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                    Detail = ce.Message
                }),
            ForbiddenAccessException fe => (StatusCodes.Status403Forbidden,
                new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Forbidden",
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    Detail = fe.Message
                }),
            _ => (-1, null)
        };

        if (problemDetails is null)
        {
            return false;
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
