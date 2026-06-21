using Microsoft.EntityFrameworkCore;
using MockQueryable.NSubstitute;
using NSubstitute;
using Repository.Enums;
using Shouldly;
using Shared.Domain.Entities;
using Shared.Persistence.Data.Contexts;
using WebApi.infrastructure.Repositories.Locks;
using WebApi.infrastructure.UnitTests.TestDoubles;
using Xunit;

namespace WebApi.infrastructure.UnitTests.Repositories.Locks;

public class UserLockTests
{
    private readonly CoreContext _context = Substitute.For<CoreContext>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Secured_ReturnsOnlyTheUserMatchingTheGivenId()
    {
        List<User> data = [TestUser.WithId(1), TestUser.WithId(2), TestUser.WithId(3)];
        DbSet<User> set = data.BuildMockDbSet();
        _context.Set<User>().Returns(set);

        UserLock sut = new(_context);

        List<User> result = await sut.Secured(2).ToListAsync(Ct);

        result.ShouldHaveSingleItem().Id.ShouldBe(2);
    }

    [Fact]
    public async Task Secured_ReturnsEmpty_WhenNoUserMatches()
    {
        List<User> data = [TestUser.WithId(1), TestUser.WithId(2)];
        DbSet<User> set = data.BuildMockDbSet();
        _context.Set<User>().Returns(set);

        UserLock sut = new(_context);

        List<User> result = await sut.Secured(99).ToListAsync(Ct);

        result.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(RepositoryOperationEnum.Read)]
    [InlineData(RepositoryOperationEnum.Insert)]
    [InlineData(RepositoryOperationEnum.Update)]
    [InlineData(RepositoryOperationEnum.Delete)]
    public async Task HasAccess_IsTrue_WhenTheUserOwnsTheRecord(RepositoryOperationEnum operation)
    {
        UserLock sut = new(_context);
        User user = TestUser.WithId(5);

        bool result = await sut.HasAccess(user, userId: 5, operation, Ct);

        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData(RepositoryOperationEnum.Read)]
    [InlineData(RepositoryOperationEnum.Insert)]
    [InlineData(RepositoryOperationEnum.Update)]
    [InlineData(RepositoryOperationEnum.Delete)]
    public async Task HasAccess_IsFalse_WhenTheUserDoesNotOwnTheRecord(RepositoryOperationEnum operation)
    {
        UserLock sut = new(_context);
        User user = TestUser.WithId(5);

        bool result = await sut.HasAccess(user, userId: 6, operation, Ct);

        result.ShouldBeFalse();
    }

    [Fact]
    public void IsMatch_IsTrue_ForUser()
    {
        UserLock sut = new(_context);

        sut.IsMatch(typeof(User)).ShouldBeTrue();
    }

    [Fact]
    public void IsMatch_IsFalse_ForUnrelatedType()
    {
        UserLock sut = new(_context);

        sut.IsMatch(typeof(string)).ShouldBeFalse();
    }
}
