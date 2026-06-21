using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Presentation.Common.ExceptionHandling;

/// <summary>
/// Translates the <see cref="UnauthorizedAccessException"/> thrown by the secured repositories when a
/// caller lacks permission for a write into an RFC 7807 <c>403 Forbidden</c> response, rather than
/// letting it surface as an unhandled 500.
/// </summary>
internal sealed class UnauthorizedAccessExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not UnauthorizedAccessException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = "You do not have permission to perform this operation.",
            },
        });
    }
}
