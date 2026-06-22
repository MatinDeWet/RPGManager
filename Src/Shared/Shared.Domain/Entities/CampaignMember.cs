using Domain.Implementation;
using Shared.Domain.Enums;

namespace Shared.Domain.Entities;

public class CampaignMember : Entity
{
    public long CampaignId { get; private set; }

    public virtual Campaign Campaign { get; private set; } = null!;

    public long UserId { get; private set; }

    public virtual User User { get; private set; } = null!;

    public CampaignRole Role { get; private set; }

    public static CampaignMember Create(long userId, CampaignRole role)
    {
        return new CampaignMember
        {
            UserId = userId,
            Role = role,
        };
    }

    public static CampaignMember Create(long campaignId, long userId, CampaignRole role)
    {
        return new CampaignMember
        {
            CampaignId = campaignId,
            UserId = userId,
            Role = role,
        };
    }
}
