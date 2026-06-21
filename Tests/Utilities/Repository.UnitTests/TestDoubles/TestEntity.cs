namespace Repository.UnitTests.TestDoubles;

/// <summary>
/// Simple entity used to exercise the generic repository operations.
/// </summary>
public class TestEntity
{
    public long Id { get; set; }

    public long OwnerId { get; set; }

    public string Name { get; set; } = string.Empty;
}
