using Repository.UnitTests.TestDoubles;
using Shouldly;
using Xunit;

namespace Repository.UnitTests.Tests;

public class LockTests
{
    private sealed class DerivedEntity : TestEntity;

    [Fact]
    public void IsMatch_ReturnsTrue_ForTheProtectedType()
    {
        TestLock sut = new();

        sut.IsMatch(typeof(TestEntity)).ShouldBeTrue();
    }

    [Fact]
    public void IsMatch_ReturnsTrue_ForDerivedType()
    {
        TestLock sut = new();

        sut.IsMatch(typeof(DerivedEntity)).ShouldBeTrue();
    }

    [Fact]
    public void IsMatch_ReturnsFalse_ForUnrelatedType()
    {
        TestLock sut = new();

        sut.IsMatch(typeof(string)).ShouldBeFalse();
    }
}
