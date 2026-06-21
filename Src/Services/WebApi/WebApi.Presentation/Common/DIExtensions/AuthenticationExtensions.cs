using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using WebApi.Presentation.Common.Options;

namespace WebApi.Presentation.Common.DIExtensions;

public static class AuthenticationExtensions
{
    /// <summary>
    /// Registers JWT bearer authentication against the configured OIDC provider and a fallback
    /// authorization policy that requires an authenticated user on every endpoint unless it is
    /// marked <c>[AllowAnonymous]</c>.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection section = configuration.GetSection(AuthenticationSettings.SectionName);
        services.Configure<AuthenticationSettings>(section);

        AuthenticationSettings settings = section.Get<AuthenticationSettings>() ?? new AuthenticationSettings();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = settings.Authority;
                options.Audience = settings.Audience;
                options.RequireHttpsMetadata = settings.RequireHttpsMetadata;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings.Authority,
                    ValidateAudience = settings.ValidateAudience,
                    ValidAudience = settings.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    NameClaimType = "preferred_username",
                    RoleClaimType = "roles",
                };
            });

        services.AddAuthorization(options => options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build());

        return services;
    }
}
