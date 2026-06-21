using Microsoft.EntityFrameworkCore;
using Repository.Contracts;

namespace Repository.Implementation;

/// <summary>
/// Provides basic query operations without security constraints.
/// This class enables direct access to entity queryables for scenarios where security filtering is not required.
/// </summary>
/// <typeparam name="TCtx">The type of Entity Framework DbContext.</typeparam>
public abstract class QueryRepo<TCtx> : IQueryRepo where TCtx : DbContext
{
    protected readonly TCtx _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryRepo{TCtx}"/> class.
    /// </summary>
    /// <param name="context">The Entity Framework DbContext.</param>
    protected QueryRepo(TCtx context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns a queryable collection for the specified entity type without any security filtering.
    /// </summary>
    /// <typeparam name="T">The type of entity to query.</typeparam>
    /// <returns>An <see cref="IQueryable{T}"/> for the specified entity type.</returns>
    public virtual IQueryable<T> GetQueryable<T>() where T : class
    {
        return _context.Set<T>();
    }
}
