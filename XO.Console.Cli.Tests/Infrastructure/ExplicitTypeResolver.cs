using System;
using System.Collections.Immutable;

namespace XO.Console.Cli.Infrastructure;

public sealed class ExplicitTypeResolver : ITypeResolver
{
    private readonly ImmutableHashSet<Type> _allowedTypes;

    public ExplicitTypeResolver(params Type[] allowedTypes)
    {
        _allowedTypes = allowedTypes.ToImmutableHashSet();
    }

    public object? Get(Type type) => _allowedTypes.Contains(type) ? Activator.CreateInstance(type) : null;
    public T? Get<T>() => _allowedTypes.Contains(typeof(T)) ? Activator.CreateInstance<T>() : default;
}
