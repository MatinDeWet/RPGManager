using Microsoft.Extensions.DependencyInjection;
using Worker.Application.Jobs;

namespace Worker.Application;

public static class WorkerApplicationDI
{
    /// <summary>
    /// Registers the worker application layer: the background-job use-cases. These are resolved per
    /// execution (Hangfire opens a scope per job), so they are registered with a scoped lifetime.
    /// </summary>
    public static IServiceCollection AddWorkerApplication(this IServiceCollection services)
    {
        services.AddScoped<IExampleJob, ExampleJob>();

        return services;
    }
}
