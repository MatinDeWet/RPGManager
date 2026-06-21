namespace Repository.Contracts;

/// <summary>
/// Defines query operations.
/// </summary>
public interface IQueryRepo
{
    /// <summary>
    /// Returns a queryable collection for the specified entity type.
    /// </summary>
    /// <typeparam name="T">The entity type to query.</typeparam>
    /// <returns>An <see cref="IQueryable{T}"/> for the specified entity type.</returns>
    IQueryable<T> GetQueryable<T>() where T : class;
}
