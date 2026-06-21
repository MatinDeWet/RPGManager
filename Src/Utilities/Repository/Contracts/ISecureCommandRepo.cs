namespace Repository.Contracts;

/// <summary>
/// Marks a command repository that always validates the current user's permissions before staging any
/// data modification. It carries the same surface as <see cref="ICommandRepo"/>, but every
/// implementation is guaranteed to be secured — there is no unvalidated path through this contract.
/// Use <see cref="ICommandRepo"/> directly when unrestricted context access is intended.
/// </summary>
/// <remarks>
/// Modification methods throw <see cref="UnauthorizedAccessException"/> when the user is not permitted to
/// perform the operation, and <see cref="InvalidOperationException"/> when the entity type has no
/// registered protection.
/// </remarks>
public interface ISecureCommandRepo : ICommandRepo
{
}
