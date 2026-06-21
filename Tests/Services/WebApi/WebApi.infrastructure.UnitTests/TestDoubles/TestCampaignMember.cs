using Shared.Domain.Entities;
using Shared.Domain.Enums;

namespace WebApi.infrastructure.UnitTests.TestDoubles;

/// <summary>
/// Builds <see cref="CampaignMember"/> rows linking a user to a campaign with a role.
/// <see cref="CampaignMember.CampaignId"/> has a private setter (EF sets it via the relationship), so
/// it is assigned via reflection for tests that exercise the membership lock directly.
/// </summary>
internal static class TestCampaignMember
{
    public static CampaignMember For(long campaignId, long userId, CampaignRole role)
    {
        var member = CampaignMember.Create(userId, role);

        typeof(CampaignMember).GetProperty(nameof(CampaignMember.CampaignId))!.SetValue(member, campaignId);

        return member;
    }
}
