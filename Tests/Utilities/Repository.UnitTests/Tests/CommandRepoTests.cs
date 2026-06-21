using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Repository.UnitTests.TestDoubles;
using Xunit;

namespace Repository.UnitTests.Tests;

public class CommandRepoTests
{
    private readonly DbContext _context = Substitute.For<DbContext>();
    private readonly TestEntity _entity = new() { Id = 1, Name = "Ada" };

    private TestCommandRepo CreateSut() => new(_context);

    [Fact]
    public async Task InsertAsync_AddsEntityToContext_WithoutSaving()
    {
        TestCommandRepo sut = CreateSut();

        await sut.InsertAsync(_entity, TestContext.Current.CancellationToken);

        _context.Received(1).Add(_entity);
        await _context.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InsertAsync_PersistImmediately_SavesChanges()
    {
        TestCommandRepo sut = CreateSut();
        CancellationToken token = TestContext.Current.CancellationToken;

        await sut.InsertAsync(_entity, persistImmediately: true, token);

        _context.Received(1).Add(_entity);
        await _context.Received(1).SaveChangesAsync(token);
    }

    [Fact]
    public async Task InsertAsync_PersistImmediatelyFalse_DoesNotSave()
    {
        TestCommandRepo sut = CreateSut();

        await sut.InsertAsync(_entity, persistImmediately: false, TestContext.Current.CancellationToken);

        _context.Received(1).Add(_entity);
        await _context.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_UpdatesEntityOnContext()
    {
        TestCommandRepo sut = CreateSut();

        await sut.UpdateAsync(_entity, TestContext.Current.CancellationToken);

        _context.Received(1).Update(_entity);
        await _context.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_PersistImmediately_SavesChanges()
    {
        TestCommandRepo sut = CreateSut();
        CancellationToken token = TestContext.Current.CancellationToken;

        await sut.UpdateAsync(_entity, persistImmediately: true, token);

        _context.Received(1).Update(_entity);
        await _context.Received(1).SaveChangesAsync(token);
    }

    [Fact]
    public async Task DeleteAsync_RemovesEntityFromContext()
    {
        TestCommandRepo sut = CreateSut();

        await sut.DeleteAsync(_entity, TestContext.Current.CancellationToken);

        _context.Received(1).Remove(_entity);
        await _context.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_PersistImmediately_SavesChanges()
    {
        TestCommandRepo sut = CreateSut();
        CancellationToken token = TestContext.Current.CancellationToken;

        await sut.DeleteAsync(_entity, persistImmediately: true, token);

        _context.Received(1).Remove(_entity);
        await _context.Received(1).SaveChangesAsync(token);
    }

    [Fact]
    public async Task SaveAsync_DelegatesToContext()
    {
        TestCommandRepo sut = CreateSut();
        CancellationToken token = TestContext.Current.CancellationToken;

        await sut.SaveAsync(token);

        await _context.Received(1).SaveChangesAsync(token);
    }
}
