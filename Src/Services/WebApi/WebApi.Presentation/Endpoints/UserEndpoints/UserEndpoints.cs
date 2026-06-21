namespace WebApi.Presentation.Endpoints.UserEndpoints;

/// <summary>
/// Registers the <c>/users</c> endpoint group and maps its child endpoints.
/// </summary>
public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/users").WithTags("Users");

        group.MapGetUserEndpoint();

        return app;
    }
}
