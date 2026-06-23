using Shared.Domain.Entities;
using Shared.Domain.Enums;

namespace WebApi.Application.UnitTests.TestDoubles;

internal static class TestCampaignMember
{
    public static CampaignMember Create(long campaignId, long userId, CampaignRole role, string? email = null)
    {
        var member = CampaignMember.Create(userId, role);

        typeof(CampaignMember).GetProperty(nameof(CampaignMember.CampaignId))!.SetValue(member, campaignId);

        if (email is not null)
        {
            typeof(CampaignMember).GetProperty(nameof(CampaignMember.User))!
                .SetValue(member, User.Create($"external-{userId}", email));
        }

        return member;
    }
}
