namespace Worker.Presentation.Common.Authorization;

public static class DashboardAuthorization
{
    /// <summary>Authorization policy gating the Hangfire dashboard endpoint.</summary>
    public const string PolicyName = "HangfireDashboard";

    /// <summary>Keycloak role a user must hold to access the Hangfire dashboard.</summary>
    public const string Role = "Hangfire";
}
