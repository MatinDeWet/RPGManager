namespace Repository.Enums;

/// <summary>
/// Specifies the type of repository operation being performed for security validation.
/// </summary>
public enum RepositoryOperationEnum
{
    /// <summary>
    /// No specific operation or undefined context.
    /// </summary>
    None = 0,

    /// <summary>
    /// Entity creation operation.
    /// </summary>
    Insert = 1,

    /// <summary>
    /// Entity modification operation.
    /// </summary>
    Update = 2,

    /// <summary>
    /// Entity deletion operation.
    /// </summary>
    Delete = 3,

    /// <summary>
    /// Entity read/query operation.
    /// </summary>
    Read = 4
}
