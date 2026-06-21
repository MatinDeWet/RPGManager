using Identification.Contracts;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Repository.Contracts;
using Repository.Enums;
using Repository.Implementation;
using Repository.UnitTests.TestDoubles;
using Shouldly;
using Xunit;

namespace Repository.UnitTests.Tests;

public class SecureCommandRepoTests
{
    private const long UserId = 42;

    private readonly DbContext _context = Substitute.For<DbContext>();
    private readonly IIdentityInfo _info = Substitute.For<IIdentityInfo>();
    private readonly IProtected<TestEntity> _lock = Substitute.For<IProtected<TestEntity>>();
    private readonly TestEntity _entity = new() { Id = 1, OwnerId = UserId, Name = "Ada" };

    public SecureCommandRepoTests()
    {
        _info.GetInternalUserId().Returns(UserId);
        _lock.IsMatch(typeof(TestEntity)).Returns(true);
    }

    private TestSecureCommandRepo CreateSut(params IProtected[] protection) => new(_context, _info, protection);

    private void Grant() => _lock.HasAccess(Arg.Any<TestEntity>(), Arg.Any<long>(), Arg.Any<RepositoryOperationEnum>(), Arg.Any<CancellationToken>()).Returns(true);

    private void Deny() => _lock.HasAccess(Arg.Any<TestEntity>(), Arg.Any<long>(), Arg.Any<RepositoryOperationEnum>(), Arg.Any<CancellationToken>()).Returns(false);

    [Fact]
    public async Task InsertAsync_StagesEntity_WhenLockGrantsAccess()
    {
        Grant();
        TestSecureCommandRepo sut = CreateSut(_lock);

        await sut.InsertAsync(_entity, TestContext.Current.CancellationToken);

        _context.Received(1).Add(_entity);
    }

    [Fact]
    public async Task InsertAsync_Throws_WhenLockDeniesAccess()
    {
        Deny();
        TestSecureCommandRepo sut = CreateSut(_lock);

        await Should.ThrowAsync<UnauthorizedAccessException>(
            () => sut.InsertAsync(_entity, TestContext.Current.CancellationToken));

        _context.DidNotReceive().Add(Arg.Any<TestEntity>());
    }

    [Fact]
    public async Task InsertAsync_PassesInternalUserIdAndInsertOperation_ToLock()
    {
        Grant();
        TestSecureCommandRepo sut = CreateSut(_lock);
        CancellationToken token = TestContext.Current.CancellationToken;

        await sut.InsertAsync(_entity, token);

        await _lock.Received(1).HasAccess(_entity, UserId, RepositoryOperationEnum.Insert, token);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenLockDeniesAccess()
    {
        Deny();
        TestSecureCommandRepo sut = CreateSut(_lock);

        await Should.ThrowAsync<UnauthorizedAccessException>(
            () => sut.UpdateAsync(_entity, TestContext.Current.CancellationToken));

        _context.DidNotReceive().Update(Arg.Any<TestEntity>());
    }

    [Fact]
    public async Task UpdateAsync_UsesUpdateOperation_WhenCheckingAccess()
    {
        Grant();
        TestSecureCommandRepo sut = CreateSut(_lock);
        CancellationToken token = TestContext.Current.CancellationToken;

        await sut.UpdateAsync(_entity, token);

        await _lock.Received(1).HasAccess(_entity, UserId, RepositoryOperationEnum.Update, token);
        _context.Received(1).Update(_entity);
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenLockDeniesAccess()
    {
        Deny();
        TestSecureCommandRepo sut = CreateSut(_lock);

        await Should.ThrowAsync<UnauthorizedAccessException>(
            () => sut.DeleteAsync(_entity, TestContext.Current.CancellationToken));

        _context.DidNotReceive().Remove(Arg.Any<TestEntity>());
    }

    [Fact]
    public async Task DeleteAsync_UsesDeleteOperation_WhenCheckingAccess()
    {
        Grant();
        TestSecureCommandRepo sut = CreateSut(_lock);
        CancellationToken token = TestContext.Current.CancellationToken;

        await sut.DeleteAsync(_entity, token);

        await _lock.Received(1).HasAccess(_entity, UserId, RepositoryOperationEnum.Delete, token);
        _context.Received(1).Remove(_entity);
    }

    [Fact]
    public async Task InsertAsync_Throws_WhenNoProtectionRegistered()
    {
        TestSecureCommandRepo sut = CreateSut();

        await Should.ThrowAsync<InvalidOperationException>(
            () => sut.InsertAsync(_entity, TestContext.Current.CancellationToken));

        _context.DidNotReceive().Add(Arg.Any<TestEntity>());
    }

    [Fact]
    public async Task InsertAsync_Throws_WhenMultipleProtectionsMatch()
    {
        Grant();
        IProtected<TestEntity> second = Substitute.For<IProtected<TestEntity>>();
        second.IsMatch(typeof(TestEntity)).Returns(true);
        TestSecureCommandRepo sut = CreateSut(_lock, second);

        await Should.ThrowAsync<InvalidOperationException>(
            () => sut.InsertAsync(_entity, TestContext.Current.CancellationToken));

        _context.DidNotReceive().Add(Arg.Any<TestEntity>());
    }

    [Fact]
    public async Task InsertAsync_StillEnforcesAccess_WhenReferencedAsBaseCommandRepoType()
    {
        Deny();

        // Upcast to the non-secure base type: the override must still run (no bypass).
        CommandRepo<DbContext> sut = CreateSut(_lock);

        await Should.ThrowAsync<UnauthorizedAccessException>(
            () => sut.InsertAsync(_entity, TestContext.Current.CancellationToken));

        _context.DidNotReceive().Add(Arg.Any<TestEntity>());
    }

    [Fact]
    public async Task InsertAsync_PersistImmediately_SavesChanges_WhenGranted()
    {
        Grant();
        TestSecureCommandRepo sut = CreateSut(_lock);
        CancellationToken token = TestContext.Current.CancellationToken;

        await sut.InsertAsync(_entity, persistImmediately: true, token);

        _context.Received(1).Add(_entity);
        await _context.Received(1).SaveChangesAsync(token);
    }
}
