---
name: add-worker-job
description: Scaffold a recurring background job in the RPGManager Worker service (Hangfire). Use when adding a scheduled/recurring job (e.g. "add a nightly job that purges expired invitations", "send a weekly digest"). Creates the framework-agnostic IJob + handler in Worker.Application, registers it for DI, schedules it with a cron in RegisterRecurringJobs, and adds a Worker.Application.UnitTests test. Distinct from add-feature (HTTP request/response use cases).
---

# Add a recurring worker job

The Worker runs scheduled work via **Hangfire**, but the **Application layer stays free of Hangfire types** — jobs are plain interfaces invoked through DI. Hangfire only appears in `Worker.Presentation` (scheduling) and `Worker.infrastructure` (server + storage). Background jobs run with **no current-user identity**, so they read/write through the **unsecured** repos. Model on `Worker.Application/Jobs/ExampleJob`.

## 1. Job contract + handler — `Worker.Application/Jobs/`

- `I<Name>Job` — `public interface` with one `Task RunAsync(CancellationToken cancellationToken)`. **No Hangfire attributes/types.**
- `<Name>Job` — `internal sealed class` implementing it; inject what it needs.

```csharp
// IPurgeInvitationsJob.cs
namespace Worker.Application.Jobs;

public interface IPurgeInvitationsJob
{
    Task RunAsync(CancellationToken cancellationToken);
}

// PurgeInvitationsJob.cs
internal sealed class PurgeInvitationsJob(
    ICampaignInvitationUnsecuredQueryRepo queryRepo,
    IUnsecuredCommandRepo commandRepo,
    IOptions<InvitationPurgeOptions> options,
    ILogger<PurgeInvitationsJob> logger) : IPurgeInvitationsJob
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddDays(-options.Value.TerminalRetentionDays);

        List<CampaignInvitation> stale = await queryRepo.Invitations
            .Where(x => /* … */ x.DateCreated <= cutoff)
            .ToListAsync(cancellationToken);

        if (stale.Count == 0)
        {
            logger.PurgedInvitations(0);
            return;
        }

        foreach (CampaignInvitation invitation in stale)
        {
            await commandRepo.DeleteAsync(invitation, cancellationToken);
        }

        await commandRepo.SaveAsync(cancellationToken);
        logger.PurgedInvitations(stale.Count);
    }
}
```

**Repos:** the Worker's query repos are **per entity** (`I<Entity>UnsecuredQueryRepo` in `Worker.Application/Repositories/QueryRepos/UnsecuredRepos/` + impl in `Worker.infrastructure`); the command repo is the **generic** `IUnsecuredCommandRepo`. Add a missing per-entity query repo the same way the WebApi unsecured ones are shaped (see `add-secured-repo`). Auto-registered via `AddRepositories` (Scrutor) in `WorkerInfrastructureDI` — no manual DI.

**Set-based delete:** prefer load-then-`DeleteAsync`+`SaveAsync` (matches the codebase and is unit-testable). `ExecuteDeleteAsync` is more efficient but can't be exercised against MockQueryable, so avoid it unless volume demands it and you accept thinner test coverage.

## 2. LoggerMessage — `Worker.Application/Logging/WorkerLog.cs`

Add a source-generated log method to the existing `internal static partial class WorkerLog` (don't call `logger.LogInformation(...)` directly):

```csharp
[LoggerMessage(Level = LogLevel.Information, Message = "PurgeInvitationsJob ran. Deleted {Count} invitation(s).")]
public static partial void PurgedInvitations(this ILogger logger, int count);
```

## 3. Register for DI — `Worker.Application/WorkerApplicationDI.cs`

```csharp
services.AddScoped<IPurgeInvitationsJob, PurgeInvitationsJob>();
```

Scoped — Hangfire opens a scope per execution.

## 4. Schedule it — `Worker.Presentation/Common/DIExtensions/RecurringJobExtensions.cs`

Add one `AddOrUpdate<TJob>` line with a stable job id and a `Cron.*` cadence:

```csharp
recurringJobs.AddOrUpdate<IPurgeInvitationsJob>("purge-invitations", job => job.RunAsync(CancellationToken.None), Cron.Daily());
```

`Cron.Hourly()` / `Cron.Daily()` / `Cron.Weekly()` or a raw cron string. The job id must be unique and stable (re-running `AddOrUpdate` updates in place).

## 5. Config (if the job has tunables)

Put an options class in `Worker.Application/Options/` (`SectionName` const + defaults) and bind it in `Worker.Presentation/Program.cs` with `services.Configure<T>(builder.Configuration.GetSection(T.SectionName))`; add a non-secret section to the worker `appsettings.json`. Use the **`add-options`** skill — and note its caveat about `Options.Create` colliding with a `*.Options` namespace in tests.

## 6. Test — `Tests/Services/Worker/Worker.Application.UnitTests/`

If the project doesn't exist yet: create the `csproj` (xunit.v3 + Shouldly + NSubstitute + MockQueryable.NSubstitute + Microsoft.EntityFrameworkCore + Test.Sdk + coverlet, `OutputType=Exe`), add `<InternalsVisibleTo Include="Worker.Application.UnitTests" />` to `Worker.Application.csproj` (jobs are `internal`), and register it in `RPGManager.slnx` under `/Tests/Services/Worker/`.

Test the job's decision logic: stub the query repo with `list.BuildMock()`, supply `IOptions<T>` via `Microsoft.Extensions.Options.Options.Create(...)` (fully-qualified — see step 5), pass `NullLogger<T>.Instance`, and assert the command repo received `DeleteAsync` for the right rows and `SaveAsync` once (and the no-op path does neither). Build entities with DB-generated values (e.g. `DateCreated`) set via reflection.

## After scaffolding

- `dotnet build` (warnings-as-errors) and `dotnet test` the Worker test project.
- Confirm the job id appears in the Hangfire dashboard at `/hangfire` when the Worker runs.
