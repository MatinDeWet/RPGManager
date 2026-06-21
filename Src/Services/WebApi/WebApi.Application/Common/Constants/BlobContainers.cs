using System.Globalization;

namespace WebApi.Application.Common.Constants;

/// <summary>
/// Naming for the year-based blob containers that hold travel-history CSV files. Names are
/// AWS S3-compatible (lowercase, hyphen-separated, alphanumeric start/end).
/// </summary>
public static class BlobContainers
{
    public const string TransactionFilePrefix = "transactionfile-";

    /// <summary>Returns the container for a given calendar year, e.g. <c>transactionfile-2026</c>.</summary>
    public static string ForYear(int year)
    {
        return TransactionFilePrefix + year.ToString(CultureInfo.InvariantCulture);
    }
}
