using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Presentation.Common.ExceptionHandling;

/// <summary>
/// Catch-all handler for any exception not claimed by a more specific handler. It logs the full
/// exception server-side (correlated by the request <c>traceId</c>) and returns a sanitized
/// <c>500</c> RFC 9457 response — never echoing the exception message or stack trace to the caller.
/// Registered last so the specific handlers win.
/// </summary>
internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "Unhandled exception processing {Method} {Path} (traceId: {TraceId})",
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred",
                Detail = "An unexpected error occurred while processing your request.",
            },
        });
    }
}
