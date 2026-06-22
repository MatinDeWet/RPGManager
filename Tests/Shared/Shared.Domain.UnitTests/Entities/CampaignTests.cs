using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shouldly;
using Xunit;

namespace Shared.Domain.UnitTests.Entities;

public class CampaignTests
{
    [Fact]
    public void Create_EnrolsCreatorAsDungeonMaster()
    {
        var campaign = Campaign.Create(worldId: 1, creatorUserId: 1, "campaign", description: null);

        CampaignMember member = campaign.Members.ShouldHaveSingleItem();
        member.UserId.ShouldBe(1);
        member.Role.ShouldBe(CampaignRole.DungeonMaster);
    }
}
