namespace WebApi.Presentation.Endpoints.InvitationEndpoints;

/// <summary>
/// Registers the <c>/invitations</c> endpoint group and maps its child endpoints. These are the
/// invitee-facing actions, keyed by the raw single-use token rather than a campaign id.
/// </summary>
public static class InvitationEndpoints
{
    public static IEndpointRouteBuilder MapInvitationEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/invitations").WithTags("Invitations");

        group.MapAcceptInvitationEndpoint();
        group.MapDeclineInvitationEndpoint();

        return app;
    }
}
