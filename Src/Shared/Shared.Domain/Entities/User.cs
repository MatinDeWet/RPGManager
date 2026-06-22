using Ardalis.GuardClauses;
using Domain.Extensions;
using Domain.Implementation;

namespace Shared.Domain.Entities;

public class User : Entity<long>
{
    public string IdentityId { get; private set; }

    public virtual ICollection<World> Worlds { get; private set; } = [];

    public virtual ICollection<CampaignMember> CampaignMemberships { get; private set; } = [];

    public virtual ICollection<CampaignInvitation> IssuedInvitations { get; private set; } = [];

    public virtual ICollection<CampaignInvitation> AcceptedInvitations { get; private set; } = [];

    public static User Create(string identityId)
    {
        return new User
        {
            IdentityId = ValidIdentityId(identityId)
        };
    }

    private static string ValidIdentityId(string identityId)
    {
        return Guard.Against.ValidString(identityId, nameof(identityId), maxLength: 256);
    }
}
