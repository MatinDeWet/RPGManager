namespace Worker.Application.Jobs;

public interface IPurgeInvitationsJob
{
    Task RunAsync(CancellationToken cancellationToken);
}
