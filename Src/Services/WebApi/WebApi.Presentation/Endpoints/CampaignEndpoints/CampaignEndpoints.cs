namespace WebApi.Presentation.Endpoints.CampaignEndpoints;

/// <summary>
/// Registers the <c>/campaigns</c> endpoint group and maps its child endpoints.
/// </summary>
public static class CampaignEndpoints
{
    public static IEndpointRouteBuilder MapCampaignEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/campaigns").WithTags("Campaigns");

        group.MapSearchCampaignsEndpoint();
        group.MapGetCampaignByIdEndpoint();
        group.MapCreateCampaignEndpoint();
        group.MapUpdateCampaignEndpoint();
        group.MapDeleteCampaignEndpoint();

        return app;
    }
}
