namespace WebApi.Application.Common.Models;

/// <summary>
/// An inclusive, open-ended date range filter. Either bound may be omitted.
/// </summary>
/// <param name="From">Inclusive lower bound, or <c>null</c> for no lower bound.</param>
/// <param name="To">Inclusive upper bound, or <c>null</c> for no upper bound.</param>
public sealed record DateRange(DateTimeOffset? From, DateTimeOffset? To)
{
    /// <summary>Whether either bound is set.</summary>
    public bool HasValue => From.HasValue || To.HasValue;
}
