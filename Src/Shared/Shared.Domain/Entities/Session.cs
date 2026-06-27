using Ardalis.GuardClauses;
using Domain.Extensions;
using Domain.Implementation;
using Shared.Domain.Enums;

namespace Shared.Domain.Entities;

public class Session : Entity<long>
{
    public long CampaignId { get; private set; }

    public virtual Campaign Campaign { get; private set; } = null!;

    public int SessionNumber { get; private set; }

    public string Title { get; private set; }

    public DateTimeOffset ScheduledAt { get; private set; }

    public string? Summary { get; private set; }

    public SessionStatus Status { get; private set; }

    public static Session Create(long campaignId, int sessionNumber, string title, DateTimeOffset scheduledAt, string? summary)
    {
        return new Session
        {
            CampaignId = campaignId,
            SessionNumber = sessionNumber,
            Title = ValidTitle(title),
            ScheduledAt = scheduledAt,
            Summary = ValidSummary(summary),
            Status = SessionStatus.Scheduled,
        };
    }

    public void Update(int sessionNumber, string title, DateTimeOffset scheduledAt, string? summary)
    {
        SessionNumber = sessionNumber;
        Title = ValidTitle(title);
        ScheduledAt = scheduledAt;
        Summary = ValidSummary(summary);
    }

    public void Complete()
    {
        EnsureScheduled();

        Status = SessionStatus.Completed;
    }

    public void Cancel()
    {
        EnsureScheduled();

        Status = SessionStatus.Cancelled;
    }

    private static string ValidTitle(string title)
    {
        return Guard.Against.ValidString(title, nameof(title), maxLength: 128);
    }

    private static string? ValidSummary(string? summary)
    {
        if (string.IsNullOrWhiteSpace(summary))
        {
            return null;
        }

        return Guard.Against.ValidString(summary, nameof(summary), maxLength: 4096);
    }

    private void EnsureScheduled()
    {
        if (Status != SessionStatus.Scheduled)
        {
            throw new InvalidOperationException($"The session is '{Status}' and can no longer be modified.");
        }
    }
}
