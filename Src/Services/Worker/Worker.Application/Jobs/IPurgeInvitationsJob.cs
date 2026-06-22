namespace Worker.Application.Jobs;

/// <summary>
/// Recurring job that deletes invitations which are terminal (accepted/declined/revoked) or past
/// their expiry and older than the configured retention window. Hangfire (wired in
/// Worker.Presentation) schedules and invokes this through DI; the interface stays free of any
/// Hangfire types so the Application layer remains framework-agnostic.
/// </summary>
public interface IPurgeInvitationsJob
{
    Task RunAsync(CancellationToken cancellationToken);
}
