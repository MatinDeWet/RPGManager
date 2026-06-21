using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Presentation.Common.ExceptionHandling;

/// <summary>
/// Translates an <see cref="ArgumentException"/> — the type the domain guard clauses
/// (<c>Guard.Against.NullOrWhiteSpace</c>/<c>InvalidInput</c>) throw for invalid input — into a
/// <c>400 Bad Request</c> RFC 9457 response. The guard messages are input-oriented and safe to
/// surface (e.g. "Name cannot exceed 100 characters."), so they form the <c>Detail</c>.
/// </summary>
/// <remarks>
/// This maps <em>all</em> <see cref="ArgumentException"/>s to 400, so a genuinely internal
/// argument bug would also be classed as a client error. Acceptable for now since guard clauses are
/// the dominant source inside request handling; a dedicated domain-validation exception thrown by
/// the guards would tighten this later. Anything else falls through to <see cref="GlobalExceptionHandler"/>.
/// </remarks>
internal sealed class ValidationExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ArgumentException argumentException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = StripParameterSuffix(argumentException.Message),
            },
        });
    }

    /// <summary>
    /// Drops the trailing <c>" (Parameter 'x')"</c> that <see cref="ArgumentException"/> appends to its
    /// message, leaving the human-readable validation reason.
    /// </summary>
    private static string StripParameterSuffix(string message)
    {
        int suffixIndex = message.IndexOf(" (Parameter '", StringComparison.Ordinal);
        return suffixIndex >= 0 ? message[..suffixIndex] : message;
    }
}
