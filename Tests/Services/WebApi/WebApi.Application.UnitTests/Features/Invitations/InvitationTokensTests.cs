using Shouldly;
using WebApi.Application.Features.CampaignFeatures.Invitations.Common;
using Xunit;

namespace WebApi.Application.UnitTests.Features.Invitations;

public class InvitationTokensTests
{
    [Fact]
    public void Generate_ProducesRawTokenWhoseHashMatchesHash()
    {
        (string raw, string hash) = InvitationTokens.Generate();

        raw.ShouldNotBeNullOrWhiteSpace();
        hash.ShouldBe(InvitationTokens.Hash(raw));
    }

    [Fact]
    public void Generate_ProducesUniqueTokens()
    {
        (string first, _) = InvitationTokens.Generate();
        (string second, _) = InvitationTokens.Generate();

        first.ShouldNotBe(second);
    }

    [Fact]
    public void Hash_IsDeterministic()
    {
        InvitationTokens.Hash("token").ShouldBe(InvitationTokens.Hash("token"));
    }
}
