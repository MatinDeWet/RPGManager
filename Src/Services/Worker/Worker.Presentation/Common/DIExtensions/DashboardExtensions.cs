using Hangfire;
using Worker.Presentation.Common.Authorization;

namespace Worker.Presentation.Common.DIExtensions;

public static class DashboardExtensions
{
    /// <summary>
    /// Maps the Hangfire dashboard at <c>/hangfire</c>, gated by the dashboard authorization policy
    /// (Keycloak login + the <see cref="DashboardAuthorization.Role"/> role). Hangfire's own filters
    /// are cleared so authorization is handled by the ASP.NET endpoint policy — an anonymous request
    /// is challenged into the OIDC login flow.
    /// </summary>
    public static WebApplication UseWorkerDashboard(this WebApplication app)
    {
        app.MapHangfireDashboard("/hangfire", new DashboardOptions { Authorization = [] })
            .RequireAuthorization(DashboardAuthorization.PolicyName);

        return app;
    }
}
