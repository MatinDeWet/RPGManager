namespace WebApi.Presentation.Endpoints.CampaignEndpoints;

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

        RouteGroupBuilder members = app.MapGroup("/campaigns").WithTags("Campaign Members");

        members.MapGetCampaignMembersEndpoint();
        members.MapKickCampaignMemberEndpoint();
        members.MapLeaveCampaignEndpoint();

        RouteGroupBuilder invitations = app.MapGroup("/campaigns").WithTags("Campaign Invitations");

        invitations.MapCreateInvitationEndpoint();
        invitations.MapSearchInvitationsEndpoint();
        invitations.MapRevokeInvitationEndpoint();

        return app;
    }
}
