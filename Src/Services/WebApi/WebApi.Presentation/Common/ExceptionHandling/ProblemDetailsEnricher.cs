using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace WebApi.Presentation.Common.ExceptionHandling;

/// <summary>
/// Enriches every <see cref="ProblemDetails"/> written through
/// <see cref="Microsoft.AspNetCore.Http.IProblemDetailsService"/> with RFC 9457 metadata that is safe to
/// expose: a correlation <c>traceId</c>, the request <c>instance</c>, a standard HTTP status <c>type</c>
/// link, and a default <c>title</c>. This runs for both the exception handlers and the Ardalis.Result
/// <c>ToMinimalApiResult()</c> mappings, so enrichment lives in one place.
/// </summary>
internal static class ProblemDetailsEnricher
{
    public static void Enrich(ProblemDetailsContext context)
    {
        ProblemDetails problem = context.ProblemDetails;
        HttpContext httpContext = context.HttpContext;

        int status = problem.Status ?? httpContext.Response.StatusCode;
        problem.Status = status;

        // Correlation id the caller can quote in a bug report; matches the id logged server-side.
        string? traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        if (!string.IsNullOrEmpty(traceId))
        {
            problem.Extensions["traceId"] = traceId;
        }

        problem.Instance ??= $"{httpContext.Request.Method} {httpContext.Request.Path}";
        problem.Type ??= $"https://httpstatuses.io/{status}";
        problem.Title ??= ReasonPhrases.GetReasonPhrase(status);
    }
}
