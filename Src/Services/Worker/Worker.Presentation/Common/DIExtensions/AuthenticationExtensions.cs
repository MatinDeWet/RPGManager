using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Worker.Presentation.Common.Authorization;
using Worker.Presentation.Common.Options;

namespace Worker.Presentation.Common.DIExtensions;

public static class AuthenticationExtensions
{
    /// <summary>
    /// Registers interactive OIDC login (cookie session + Keycloak Authorization Code flow) and the
    /// dashboard authorization policy that requires the <see cref="DashboardAuthorization.Role"/> role.
    /// </summary>
    public static IServiceCollection AddDashboardAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection section = configuration.GetSection(AuthenticationSettings.SectionName);
        services.Configure<AuthenticationSettings>(section);

        AuthenticationSettings settings = section.Get<AuthenticationSettings>() ?? new AuthenticationSettings();

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
            .AddCookie()
            .AddOpenIdConnect(options =>
            {
                options.Authority = settings.Authority;
                options.ClientId = settings.ClientId;
                options.ClientSecret = settings.ClientSecret;
                options.RequireHttpsMetadata = settings.RequireHttpsMetadata;
                options.ResponseType = "code";
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.MapInboundClaims = false;
                options.Scope.Add("roles");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "preferred_username",
                    RoleClaimType = "roles",
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(DashboardAuthorization.PolicyName, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(DashboardAuthorization.Role));

        return services;
    }
}
