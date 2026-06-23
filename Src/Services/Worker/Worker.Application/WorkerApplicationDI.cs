using Microsoft.Extensions.DependencyInjection;
using Worker.Application.Jobs;

namespace Worker.Application;

public static class WorkerApplicationDI
{
    public static IServiceCollection AddWorkerApplication(this IServiceCollection services)
    {
        services.AddScoped<IExampleJob, ExampleJob>();
        services.AddScoped<IPurgeInvitationsJob, PurgeInvitationsJob>();

        return services;
    }
}
