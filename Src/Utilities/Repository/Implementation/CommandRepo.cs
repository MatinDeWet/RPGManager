using Microsoft.EntityFrameworkCore;
using Repository.Contracts;

namespace Repository.Implementation;

/// <summary>
/// Provides basic command operations without security constraints for data modifications.
/// This class enables direct data manipulation operations for scenarios where security validation is not required.
/// </summary>
/// <typeparam name="TCtx">The type of Entity Framework DbContext.</typeparam>
public abstract class CommandRepo<TCtx> : ICommandRepo where TCtx : DbContext
{
    protected readonly TCtx _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandRepo{TCtx}"/> class.
    /// </summary>
    /// <param name="context">The Entity Framework DbContext.</param>
    protected CommandRepo(TCtx context)
    {
        _context = context;
    }

    /// <summary>
    /// Stages the entity for insertion without security validation.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="obj">The entity to insert.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public virtual Task InsertAsync<T>(T obj, CancellationToken cancellationToken) where T : class
    {
        _context.Add(obj);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stages the entity for insertion without security validation, optionally persisting immediately.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="obj">The entity to insert.</param>
    /// <param name="persistImmediately">Whether to save changes immediately.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task InsertAsync<T>(T obj, bool persistImmediately, CancellationToken cancellationToken) where T : class
    {
        await InsertAsync(obj, cancellationToken);

        if (persistImmediately)
        {
            await SaveAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Stages the entity for update without security validation.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="obj">The entity to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public virtual Task UpdateAsync<T>(T obj, CancellationToken cancellationToken) where T : class
    {
        _context.Update(obj);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stages the entity for update without security validation, optionally persisting immediately.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="obj">The entity to update.</param>
    /// <param name="persistImmediately">Whether to save changes immediately.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task UpdateAsync<T>(T obj, bool persistImmediately, CancellationToken cancellationToken) where T : class
    {
        await UpdateAsync(obj, cancellationToken);

        if (persistImmediately)
        {
            await SaveAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Stages the entity for deletion without security validation.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="obj">The entity to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public virtual Task DeleteAsync<T>(T obj, CancellationToken cancellationToken) where T : class
    {
        _context.Remove(obj);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stages the entity for deletion without security validation, optionally persisting immediately.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="obj">The entity to delete.</param>
    /// <param name="persistImmediately">Whether to save changes immediately.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task DeleteAsync<T>(T obj, bool persistImmediately, CancellationToken cancellationToken) where T : class
    {
        await DeleteAsync(obj, cancellationToken);

        if (persistImmediately)
        {
            await SaveAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Persists all staged changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public virtual async Task SaveAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
