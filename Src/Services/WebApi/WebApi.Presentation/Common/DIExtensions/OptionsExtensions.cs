using WebApi.Application.Common.Options;

namespace WebApi.Presentation.Common.DIExtensions;

public static class OptionsExtensions
{
    public static IServiceCollection AddApplicationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<InvitationOptions>(configuration.GetSection(InvitationOptions.SectionName));

        return services;
    }
}
