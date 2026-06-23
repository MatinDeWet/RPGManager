using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Domain.Extensions;
using Domain.Implementation;

namespace Shared.Domain.Entities;

public class User : Entity<long>
{
    public string IdentityId { get; private set; }

    public string Email { get; private set; }

    public virtual ICollection<World> Worlds { get; private set; } = [];

    public virtual ICollection<CampaignMember> CampaignMemberships { get; private set; } = [];

    public virtual ICollection<CampaignInvitation> IssuedInvitations { get; private set; } = [];

    public virtual ICollection<CampaignInvitation> AcceptedInvitations { get; private set; } = [];

    public static User Create(string identityId, string email)
    {
        return new User
        {
            IdentityId = ValidIdentityId(identityId),
            Email = ValidEmail(email)
        };
    }

    /// <summary>
    /// Applies the latest identity-provider details, returning <see langword="true"/> when a change was made.
    /// </summary>
    public bool Update(string email)
    {
        string validEmail = ValidEmail(email);

        if (string.Equals(Email, validEmail, StringComparison.Ordinal))
        {
            return false;
        }

        Email = validEmail;
        return true;
    }

    private static string ValidIdentityId(string identityId)
    {
        return Guard.Against.ValidString(identityId, nameof(identityId), maxLength: 256);
    }

    [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase",
        Justification = "Emails are normalised to lowercase for storage and case-insensitive matching, not for a security decision.")]
    private static string ValidEmail(string email)
    {
        return Guard.Against.ValidString(email, nameof(email), maxLength: 256).Trim().ToLowerInvariant();
    }
}
