using System.Security.Claims;
using Identification.Constants;
using Identification.Implementation;
using Shouldly;
using Xunit;

namespace Identification.UnitTests.Tests;

public class IdentityInfoTests
{
    private static IdentityInfo CreateSut(params Claim[] claims)
    {
        InfoSetter infoSetter = new();
        infoSetter.SetUser(claims);
        return new IdentityInfo(infoSetter);
    }

    [Fact]
    public void GetInternalUserId_ReturnsLong_WhenClaimIsValid()
    {
        IdentityInfo sut = CreateSut(new Claim(ClaimConstants.InternalUserId, "42"));

        sut.GetInternalUserId().ShouldBe(42L);
    }

    [Fact]
    public void GetInternalUserId_Throws_WhenClaimIsMissing()
    {
        IdentityInfo sut = CreateSut();

        Should.Throw<InvalidOperationException>(() => sut.GetInternalUserId());
    }

    [Fact]
    public void GetInternalUserId_Throws_WhenClaimIsNotANumber()
    {
        IdentityInfo sut = CreateSut(new Claim(ClaimConstants.InternalUserId, "not-a-number"));

        Should.Throw<InvalidOperationException>(() => sut.GetInternalUserId());
    }

    [Fact]
    public void GetExternalUserId_ReturnsString_WhenClaimIsValid()
    {
        const string externalUserId = "external-abc-123";
        IdentityInfo sut = CreateSut(new Claim(ClaimConstants.ExternalUserId, externalUserId));

        sut.GetExternalUserId().ShouldBe(externalUserId);
    }

    [Fact]
    public void GetExternalUserId_Throws_WhenClaimIsMissing()
    {
        IdentityInfo sut = CreateSut();

        Should.Throw<InvalidOperationException>(() => sut.GetExternalUserId());
    }

    [Fact]
    public void IsAdmin_IsTrue_WhenAdminRoleClaimIsPresent()
    {
        IdentityInfo sut = CreateSut(new Claim(ClaimTypes.Role, RoleConstants.Admin));

        sut.IsAdmin().ShouldBeTrue();
    }

    [Fact]
    public void IsAdmin_IsFalse_WhenRoleClaimIsAbsent()
    {
        IdentityInfo sut = CreateSut(new Claim(ClaimTypes.Role, "SomeOtherRole"));

        sut.IsAdmin().ShouldBeFalse();
    }

    [Fact]
    public void HasRole_IsCaseInsensitive()
    {
        IdentityInfo sut = CreateSut(new Claim(ClaimTypes.Role, RoleConstants.Admin));

        sut.HasRole("admin").ShouldBeTrue();
    }

    [Fact]
    public void HasValue_ReflectsPresenceOfClaim()
    {
        IdentityInfo sut = CreateSut(new Claim(ClaimConstants.Name, "Ada"));

        sut.HasValue(ClaimConstants.Name).ShouldBeTrue();
        sut.HasValue(ClaimConstants.Email).ShouldBeFalse();
    }

    [Fact]
    public void GetValue_ReturnsEmptyString_WhenClaimIsMissing()
    {
        IdentityInfo sut = CreateSut();

        sut.GetValue(ClaimConstants.Name).ShouldBe(string.Empty);
    }
}
