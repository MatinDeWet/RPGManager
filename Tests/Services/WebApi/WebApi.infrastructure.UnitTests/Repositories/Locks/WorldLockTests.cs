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

public class WorldLockTests
{
    private readonly CoreContext _context = Substitute.For<CoreContext>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Secured_ReturnsOnlyWorldsOwnedByTheGivenUser()
    {
        List<World> data = [TestWorld.Owned(1, userId: 10), TestWorld.Owned(2, userId: 20), TestWorld.Owned(3, userId: 10)];
        DbSet<World> set = data.BuildMockDbSet();
        _context.Set<World>().Returns(set);

        WorldLock sut = new(_context);

        List<World> result = await sut.Secured(10).ToListAsync(Ct);

        result.Select(x => x.Id).ShouldBe([1, 3], ignoreOrder: true);
    }

    [Fact]
    public async Task Secured_ReturnsEmpty_WhenTheUserOwnsNoWorlds()
    {
        List<World> data = [TestWorld.Owned(1, userId: 10), TestWorld.Owned(2, userId: 20)];
        DbSet<World> set = data.BuildMockDbSet();
        _context.Set<World>().Returns(set);

        WorldLock sut = new(_context);

        List<World> result = await sut.Secured(99).ToListAsync(Ct);

        result.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(RepositoryOperationEnum.Read)]
    [InlineData(RepositoryOperationEnum.Insert)]
    [InlineData(RepositoryOperationEnum.Update)]
    [InlineData(RepositoryOperationEnum.Delete)]
    public async Task HasAccess_IsTrue_WhenTheUserOwnsTheWorld(RepositoryOperationEnum operation)
    {
        WorldLock sut = new(_context);
        World world = TestWorld.Owned(1, userId: 5);

        bool result = await sut.HasAccess(world, userId: 5, operation, Ct);

        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData(RepositoryOperationEnum.Read)]
    [InlineData(RepositoryOperationEnum.Insert)]
    [InlineData(RepositoryOperationEnum.Update)]
    [InlineData(RepositoryOperationEnum.Delete)]
    public async Task HasAccess_IsFalse_WhenTheUserDoesNotOwnTheWorld(RepositoryOperationEnum operation)
    {
        WorldLock sut = new(_context);
        World world = TestWorld.Owned(1, userId: 5);

        bool result = await sut.HasAccess(world, userId: 6, operation, Ct);

        result.ShouldBeFalse();
    }

    [Fact]
    public void IsMatch_IsTrue_ForWorld()
    {
        WorldLock sut = new(_context);

        sut.IsMatch(typeof(World)).ShouldBeTrue();
    }

    [Fact]
    public void IsMatch_IsFalse_ForUnrelatedType()
    {
        WorldLock sut = new(_context);

        sut.IsMatch(typeof(string)).ShouldBeFalse();
    }
}
