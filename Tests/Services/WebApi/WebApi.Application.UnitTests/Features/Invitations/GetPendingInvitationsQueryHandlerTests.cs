using Ardalis.Result;
using MockQueryable;
using NSubstitute;
using Shared.Domain.Entities;
using Shared.Domain.Enums;
using Shouldly;
using WebApi.Application.Features.CampaignFeatures.Invitations.GetPendingInvitations;
using WebApi.Application.Repositories.QueryRepos.SecuredRepos;
using WebApi.Application.UnitTests.TestDoubles;
using Xunit;

namespace WebApi.Application.UnitTests.Features.Invitations;

public class GetPendingInvitationsQueryHandlerTests
{
    private readonly ICampaignInvitationSecuredQueryRepo _queryRepo = Substitute.For<ICampaignInvitationSecuredQueryRepo>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static readonly DateTimeOffset Expiry = DateTimeOffset.UtcNow.AddDays(7);

    private GetPendingInvitationsQueryHandler Sut => new(_queryRepo);

    [Fact]
    public async Task Handle_ReturnsOnlyPendingInvitationsForTheCampaign()
    {
        CampaignInvitation pending = TestEntities.Invitation(1, campaignId: 1, "a@b.com", "h1", Expiry);
        CampaignInvitation accepted = TestEntities.Invitation(2, campaignId: 1, "c@d.com", "h2", Expiry, InvitationStatus.Accepted);
        CampaignInvitation otherCampaign = TestEntities.Invitation(3, campaignId: 2, "e@f.com", "h3", Expiry);
        _queryRepo.Invitations.Returns(new[] { pending, accepted, otherCampaign }.BuildMock());

        Result<IReadOnlyList<GetPendingInvitationsResponse>> result = await Sut.Handle(new GetPendingInvitationsQuery(1), Ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Select(x => x.Id).ShouldBe([1]);
    }
}
