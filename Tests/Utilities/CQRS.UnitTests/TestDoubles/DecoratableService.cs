namespace CQRS.UnitTests.TestDoubles;

public interface IDecoratable<T>
{
    string Describe();
}

internal sealed class ConcreteDecoratable : IDecoratable<string>
{
    public string Describe() => "concrete";
}

internal sealed class DecoratableDecorator<T>(IDecoratable<T> inner) : IDecoratable<T>
{
    public IDecoratable<T> Inner => inner;

    public string Describe() => $"decorated({inner.Describe()})";
}
