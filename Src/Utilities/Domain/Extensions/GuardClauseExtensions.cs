using System.Text.RegularExpressions;
using Ardalis.GuardClauses;

namespace Domain.Extensions;

public static class GuardClauseExtensions
{
    /// <summary>
    /// Validates a string input with comprehensive checks including null/whitespace, length constraints, and optional regex pattern validation.
    /// </summary>
    /// <param name="guardClause">The guard clause instance.</param>
    /// <param name="input">The string input to validate.</param>
    /// <param name="parameterName">The parameter name for error messages.</param>
    /// <param name="minLength">Minimum required length (optional).</param>
    /// <param name="maxLength">Maximum allowed length (optional).</param>
    /// <param name="pattern">Optional regex pattern for format validation.</param>
    /// <param name="patternErrorMessage">Custom error message for pattern validation failures.</param>
    /// <param name="allowNullOrWhiteSpace">Whether null or empty strings are allowed (default: false).</param>
    /// <returns>The validated input string.</returns>
    public static string ValidString(
        this IGuardClause guardClause,
        string? input,
        string parameterName,
        int? minLength = null,
        int? maxLength = null,
        string? pattern = null,
        string? patternErrorMessage = null,
        bool allowNullOrWhiteSpace = false)
    {
        if (!allowNullOrWhiteSpace)
        {
            Guard.Against.NullOrWhiteSpace(input, parameterName);
        }
        else if (input is null)
        {
            return string.Empty;
        }

        if (minLength.HasValue)
        {
            Guard.Against.InvalidInput(input, parameterName,
                x => x.Length >= minLength.Value,
                $"{parameterName} must be at least {minLength.Value} characters long.");
        }

        if (maxLength.HasValue)
        {
            Guard.Against.InvalidInput(input, parameterName,
                x => x.Length <= maxLength.Value,
                $"{parameterName} cannot exceed {maxLength.Value} characters.");
        }

        if (!string.IsNullOrWhiteSpace(pattern))
        {
            Guard.Against.InvalidInput(input, parameterName,
                x => Regex.IsMatch(x, pattern),
                patternErrorMessage ?? $"{parameterName} does not match the required format.");
        }

        return input;
    }
}
