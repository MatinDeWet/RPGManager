using Worker.Application.Options;

namespace Worker.Presentation.Common.DIExtensions;

public static class OptionsExtensions
{
    /// <summary>
    /// Binds the worker's strongly-typed configuration sections to the options pattern.
    /// </summary>
    public static IServiceCollection AddWorkerOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<InvitationPurgeOptions>(configuration.GetSection(InvitationPurgeOptions.SectionName));

        return services;
    }
}
