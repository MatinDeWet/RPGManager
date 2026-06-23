namespace WebApi.Presentation.Endpoints.InvitationEndpoints;

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
