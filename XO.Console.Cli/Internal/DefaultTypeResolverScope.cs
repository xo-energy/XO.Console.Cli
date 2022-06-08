namespace XO.Console.Cli;

internal sealed class DefaultTypeResolverScope : ITypeResolverScope
{
    private readonly ITypeResolver _resolver;

    public DefaultTypeResolverScope(ITypeResolver resolver)
    {
        _resolver = resolver;
    }

    public ITypeResolver TypeResolver
        => _resolver;

    void IDisposable.Dispose()
    {
        // pass
    }

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        return default;
    }
}
