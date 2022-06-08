namespace XO.Console.Cli;

internal sealed class DefaultTypeResolverScope : ITypeResolverScope
{
    private readonly ITypeResolver _resolver;

    public DefaultTypeResolverScope(ITypeResolver resolver)
    {
        _resolver = resolver;
    }

    public object? Get(Type type)
        => _resolver.Get(type);

    public T? Get<T>()
        => _resolver.Get<T>();

    void IDisposable.Dispose()
    {
        // pass
    }

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        return default;
    }
}
