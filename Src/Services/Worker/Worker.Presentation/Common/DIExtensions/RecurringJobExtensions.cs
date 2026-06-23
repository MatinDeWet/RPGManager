using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Worker.Application.Jobs;

namespace Worker.Presentation.Common.DIExtensions;

public static class RecurringJobExtensions
{
    public static IHost RegisterRecurringJobs(this IHost host)
    {
        IRecurringJobManager recurringJobs = host.Services.GetRequiredService<IRecurringJobManager>();

        recurringJobs.AddOrUpdate<IExampleJob>("example-job", job => job.RunAsync(CancellationToken.None), Cron.Hourly());
        recurringJobs.AddOrUpdate<IPurgeInvitationsJob>("purge-invitations", job => job.RunAsync(CancellationToken.None), Cron.Daily());

        return host;
    }
}
