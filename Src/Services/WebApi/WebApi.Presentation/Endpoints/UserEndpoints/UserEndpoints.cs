namespace WebApi.Presentation.Endpoints.UserEndpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/users").WithTags("Users");

        group.MapGetUserEndpoint();

        return app;
    }
}
