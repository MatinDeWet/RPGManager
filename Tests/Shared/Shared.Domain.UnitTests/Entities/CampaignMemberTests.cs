using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shouldly;
using Xunit;

namespace Shared.Domain.UnitTests.Entities;

public class CampaignMemberTests
{
    [Fact]
    public void Create_SetsUserAndRole()
    {
        var member = CampaignMember.Create(userId: 5, CampaignRole.DungeonMaster);

        member.UserId.ShouldBe(5);
        member.Role.ShouldBe(CampaignRole.DungeonMaster);
    }

    [Fact]
    public void Create_WithCampaignId_SetsAllFields()
    {
        var member = CampaignMember.Create(campaignId: 3, userId: 5, CampaignRole.Player);

        member.CampaignId.ShouldBe(3);
        member.UserId.ShouldBe(5);
        member.Role.ShouldBe(CampaignRole.Player);
    }
}
