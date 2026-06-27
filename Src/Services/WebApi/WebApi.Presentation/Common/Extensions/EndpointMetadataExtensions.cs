using Microsoft.AspNetCore.Http;

namespace WebApi.Presentation.Common.Extensions;

/// <summary>
/// Declares the OpenAPI response metadata for endpoints whose handlers return an Ardalis
/// <c>Result</c>/<c>Result&lt;T&gt;</c> via <c>ToMinimalApiResult()</c>. Because those handlers
/// return a bare <see cref="IResult"/>, the generator cannot infer the success body type or the
/// error shapes; these helpers add a typed 200 response plus the standard
/// <c>application/problem+json</c> error responses so client generators emit typed clients.
/// </summary>
public static class EndpointMetadataExtensions
{
    /// <summary>Declares a typed 200 success body plus the standard problem responses.</summary>
    public static RouteHandlerBuilder ProducesResult<TResponse>(this RouteHandlerBuilder builder)
        => builder
            .Produces<TResponse>(StatusCodes.Status200OK)
            .ProducesStandardProblems();

    /// <summary>Declares a no-content 200 success plus the standard problem responses.</summary>
    public static RouteHandlerBuilder ProducesResult(this RouteHandlerBuilder builder)
        => builder
            .Produces(StatusCodes.Status200OK)
            .ProducesStandardProblems();

    private static RouteHandlerBuilder ProducesStandardProblems(this RouteHandlerBuilder builder)
        => builder
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
}
