using Asp.Versioning;
using Asp.Versioning.Builder;
using WebApi.Presentation.Common;
using WebApi.Presentation.Endpoints.CampaignEndpoints;
using WebApi.Presentation.Endpoints.InvitationEndpoints;
using WebApi.Presentation.Endpoints.UserEndpoints;
using WebApi.Presentation.Endpoints.WorldEndpoints;

namespace WebApi.Presentation.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps every entity endpoint group under a URL-segment version prefix (e.g. <c>/v1/worlds</c>).
    /// A single shared <see cref="ApiVersionSet"/> (built from <see cref="ApiVersions.All"/>) is applied
    /// to the parent group and propagates to the nested groups, so each <c>Map&lt;Entity&gt;Endpoints</c>
    /// keeps its plain <c>MapGroup("/...")</c> and inherits the version automatically.
    /// </summary>
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        ApiVersionSetBuilder apiVersionSetBuilder = app.NewApiVersionSet();
        foreach (ApiVersion version in ApiVersions.All)
        {
            apiVersionSetBuilder.HasApiVersion(version);
        }

        ApiVersionSet apiVersionSet = apiVersionSetBuilder
            .ReportApiVersions()
            .Build();

        IEndpointRouteBuilder versioned = app
            .MapGroup("/v{version:apiVersion}")
            .WithApiVersionSet(apiVersionSet);

        versioned.MapUserEndpoints();
        versioned.MapWorldEndpoints();
        versioned.MapCampaignEndpoints();
        versioned.MapInvitationEndpoints();

        return app;
    }
}
