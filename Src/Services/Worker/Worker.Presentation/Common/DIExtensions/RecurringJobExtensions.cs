using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Worker.Application.Jobs;

namespace Worker.Presentation.Common.DIExtensions;

public static class RecurringJobExtensions
{
    /// <summary>
    /// Registers the worker's recurring Hangfire jobs. Called once after the host is built.
    /// </summary>
    public static IHost RegisterRecurringJobs(this IHost host)
    {
        IRecurringJobManager recurringJobs = host.Services.GetRequiredService<IRecurringJobManager>();

        recurringJobs.AddOrUpdate<IExampleJob>("example-job", job => job.RunAsync(CancellationToken.None), Cron.Hourly());

        return host;
    }
}
