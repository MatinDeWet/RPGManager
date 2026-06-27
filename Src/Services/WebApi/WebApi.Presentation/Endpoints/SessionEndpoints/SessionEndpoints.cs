namespace WebApi.Presentation.Endpoints.SessionEndpoints;

public static class SessionEndpoints
{
    public static IEndpointRouteBuilder MapSessionEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/sessions").WithTags("Sessions");

        group.MapSearchSessionsEndpoint();
        group.MapGetSessionByIdEndpoint();
        group.MapCreateSessionEndpoint();
        group.MapUpdateSessionEndpoint();
        group.MapDeleteSessionEndpoint();

        return app;
    }
}
