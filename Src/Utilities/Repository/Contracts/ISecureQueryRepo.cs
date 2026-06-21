namespace Repository.Contracts;

/// <summary>
/// Marks a query repository that always applies row-level security filtering based on the current
/// user's permissions. It carries the same surface as <see cref="IQueryRepo"/>, but every
/// implementation is guaranteed to be secured — there is no unfiltered path through this contract.
/// Use <see cref="IQueryRepo"/> directly when unrestricted context access is intended.
/// </summary>
public interface ISecureQueryRepo : IQueryRepo
{
}
