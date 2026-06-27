using System.Globalization;
using Asp.Versioning;

namespace WebApi.Presentation.Common;

/// <summary>
/// Single source of truth for the API versions this service publishes. Drives the version set
/// (<c>Program.cs</c>) and the per-version OpenAPI documents (<c>OpenApiExtensions</c>). To publish a
/// new version, add one <see cref="ApiVersion"/> entry to <see cref="All"/>.
/// </summary>
internal static class ApiVersions
{
    /// <summary>
    /// The ApiExplorer group-name format. Must match the value passed to <c>AddApiExplorer</c> so the
    /// document names registered with <c>AddOpenApi</c> line up with the group names the explorer assigns.
    /// </summary>
    public const string GroupNameFormat = "'v'VVV";

    /// <summary>Every published API version, oldest first.</summary>
    public static readonly IReadOnlyList<ApiVersion> All = [new ApiVersion(1, 0)];

    /// <summary>OpenAPI document / ApiExplorer group names (e.g. <c>v1</c>), one per <see cref="All"/> entry.</summary>
    public static readonly IReadOnlyList<string> AllGroupNames = [.. All.Select(GroupName)];

    /// <summary>Formats a version the same way the ApiExplorer's <see cref="GroupNameFormat"/> does.</summary>
    public static string GroupName(ApiVersion version) =>
        version.ToString(GroupNameFormat, CultureInfo.InvariantCulture);
}
