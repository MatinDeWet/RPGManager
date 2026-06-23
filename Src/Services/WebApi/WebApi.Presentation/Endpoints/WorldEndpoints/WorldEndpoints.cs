namespace WebApi.Presentation.Endpoints.WorldEndpoints;

public static class WorldEndpoints
{
    public static IEndpointRouteBuilder MapWorldEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/worlds").WithTags("Worlds");

        group.MapSearchWorldsEndpoint();
        group.MapGetWorldByIdEndpoint();
        group.MapCreateWorldEndpoint();
        group.MapUpdateWorldEndpoint();
        group.MapDeleteWorldEndpoint();

        return app;
    }
}
