using Microsoft.EntityFrameworkCore;
using MockQueryable.NSubstitute;
using NSubstitute;
using Repository.UnitTests.TestDoubles;
using Shouldly;
using Xunit;

namespace Repository.UnitTests.Tests;

public class QueryRepoTests
{
    private readonly DbContext _context = Substitute.For<DbContext>();

    [Fact]
    public async Task GetQueryable_ReturnsEntitiesFromContextSet()
    {
        List<TestEntity> data =
        [
            new() { Id = 1, Name = "Ada" },
            new() { Id = 2, Name = "Grace" },
        ];
        DbSet<TestEntity> set = data.BuildMockDbSet();
        _context.Set<TestEntity>().Returns(set);

        TestQueryRepo sut = new(_context);

        List<TestEntity> result = await sut.GetQueryable<TestEntity>()
            .ToListAsync(TestContext.Current.CancellationToken);

        result.ShouldBe(data);
    }

    [Fact]
    public void GetQueryable_QueriesTheRequestedEntityType()
    {
        DbSet<TestEntity> set = new List<TestEntity>().BuildMockDbSet();
        _context.Set<TestEntity>().Returns(set);

        TestQueryRepo sut = new(_context);

        _ = sut.GetQueryable<TestEntity>();

        _context.Received(1).Set<TestEntity>();
    }
}
