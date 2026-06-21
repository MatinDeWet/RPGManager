using Repository.Contracts;

namespace Repository.Implementation;

/// <summary>
/// Resolves the single <see cref="IProtected{T}"/> that guards a given entity type.
/// The secure repositories use this to enforce a fail-closed policy: an entity accessed through a
/// secure repository must have exactly one matching protection, otherwise access is refused.
/// </summary>
internal static class ProtectionResolver
{
    /// <summary>
    /// Returns the single protection registered for <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The entity type being protected.</typeparam>
    /// <param name="protection">All registered protection implementations.</param>
    /// <returns>The matching <see cref="IProtected{T}"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no protection is registered for <typeparamref name="T"/> (fail-closed), or when more
    /// than one protection matches (ambiguous configuration).
    /// </exception>
    public static IProtected<T> Resolve<T>(IEnumerable<IProtected> protection) where T : class
    {
        var matches = protection
            .OfType<IProtected<T>>()
            .Where(p => p.IsMatch(typeof(T)))
            .ToList();

        if (matches.Count == 0)
        {
            throw new InvalidOperationException(
                $"No protection (IProtected<{typeof(T).Name}>) is registered. Register a Lock<{typeof(T).Name}> " +
                "via AddSecuredRepositories, or use the non-secure repository for unrestricted access.");
        }

        if (matches.Count > 1)
        {
            throw new InvalidOperationException(
                $"Multiple protections (IProtected<{typeof(T).Name}>) match; entity protection must be unambiguous.");
        }

        return matches[0];
    }
}
