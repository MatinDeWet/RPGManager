namespace Worker.Application.Jobs;

/// <summary>
/// Example background job use-case. Hangfire (wired in Worker.Presentation) schedules and invokes
/// this through DI; the interface stays free of any Hangfire types so the Application layer remains
/// framework-agnostic.
/// </summary>
public interface IExampleJob
{
    Task RunAsync(CancellationToken cancellationToken);
}
