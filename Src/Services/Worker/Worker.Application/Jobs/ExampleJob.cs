using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Worker.Application.Logging;
using Worker.Application.Repositories.QueryRepos.UnsecuredRepos;

namespace Worker.Application.Jobs;

internal sealed class ExampleJob(IUnsecuredQueryRepo queryRepo, ILogger<ExampleJob> logger) : IExampleJob
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        int userCount = await queryRepo.Users.CountAsync(cancellationToken);

        logger.ExampleJobRan(userCount);
    }
}
