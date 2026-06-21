namespace WebApi.Presentation.Endpoints.WorldEndpoints;

/// <summary>
/// Registers the <c>/worlds</c> endpoint group and maps its child endpoints.
/// </summary>
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
