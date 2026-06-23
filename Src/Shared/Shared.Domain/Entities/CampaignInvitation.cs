using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Domain.Extensions;
using Domain.Implementation;
using Shared.Domain.Enums;

namespace Shared.Domain.Entities;

public class CampaignInvitation : Entity<long>
{
    private const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    public long CampaignId { get; private set; }

    public virtual Campaign Campaign { get; private set; } = null!;

    public string InviteeEmail { get; private set; } = null!;

    public string TokenHash { get; private set; } = null!;

    public long IssuedByUserId { get; private set; }

    public virtual User IssuedBy { get; private set; } = null!;

    public DateTimeOffset ExpiresAt { get; private set; }

    public InvitationStatus Status { get; private set; }

    public long? AcceptedByUserId { get; private set; }

    public virtual User? AcceptedBy { get; private set; }

    public DateTimeOffset? RespondedAt { get; private set; }

    public static CampaignInvitation Create(long campaignId, string inviteeEmail, string tokenHash, long issuedByUserId, DateTimeOffset expiresAt)
    {
        string normalizedEmail = NormalizeEmail(inviteeEmail);
        Guard.Against.InvalidFormat(normalizedEmail, nameof(inviteeEmail), EmailPattern,
            "inviteeEmail must be a valid email address.");

        return new CampaignInvitation
        {
            CampaignId = campaignId,
            InviteeEmail = normalizedEmail,
            TokenHash = Guard.Against.ValidString(tokenHash, nameof(tokenHash), maxLength: 64),
            IssuedByUserId = issuedByUserId,
            ExpiresAt = expiresAt,
            Status = InvitationStatus.Pending,
        };
    }

    public void Accept(long userId)
    {
        EnsurePending();

        Status = InvitationStatus.Accepted;
        AcceptedByUserId = userId;
        RespondedAt = DateTimeOffset.UtcNow;
    }

    public void Decline()
    {
        EnsurePending();

        Status = InvitationStatus.Declined;
        RespondedAt = DateTimeOffset.UtcNow;
    }

    public void Revoke()
    {
        EnsurePending();

        Status = InvitationStatus.Revoked;
        RespondedAt = DateTimeOffset.UtcNow;
    }

    [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase",
        Justification = "Emails are normalised to lowercase for storage and case-insensitive matching, not for a security decision.")]
    public static string NormalizeEmail(string email)
    {
        return Guard.Against.ValidString(email, nameof(email), maxLength: 256).Trim().ToLowerInvariant();
    }

    private void EnsurePending()
    {
        if (Status != InvitationStatus.Pending)
        {
            throw new InvalidOperationException($"The invitation is '{Status}' and can no longer be modified.");
        }
    }
}
