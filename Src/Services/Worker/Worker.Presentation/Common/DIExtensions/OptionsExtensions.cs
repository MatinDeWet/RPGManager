using Worker.Application.Options;

namespace Worker.Presentation.Common.DIExtensions;

public static class OptionsExtensions
{
    public static IServiceCollection AddWorkerOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<InvitationPurgeOptions>(configuration.GetSection(InvitationPurgeOptions.SectionName));

        return services;
    }
}
