namespace WebApi.Application.UnitTests.TestDoubles;

internal static class TestEntity
{
    public static void SetId<T>(T entity, long id)
    {
        typeof(T).GetProperty("Id")!.SetValue(entity, id);
    }
}
