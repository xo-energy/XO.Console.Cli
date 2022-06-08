using Microsoft.Extensions.DependencyInjection;

namespace XO.Console.Cli;

/// <summary>
/// Implements scoped service lifetime using <see cref="ServiceProviderServiceExtensions.CreateAsyncScope(IServiceProvider)"/>.
/// </summary>
public sealed class ServiceProviderTypeResolverScope : ITypeResolverScope
{
    private readonly AsyncServiceScope _scope;
    private readonly ITypeResolver _resolver;

    /// <summary>
    /// Initializes a new instance of <see cref="ServiceProviderTypeResolverScope"/>.
    /// </summary>
    /// <param name="scope">The <see cref="AsyncServiceScope"/> that controls the scope lifetime.</param>
    public ServiceProviderTypeResolverScope(AsyncServiceScope scope)
    {
        _scope = scope;
        _resolver = new ServiceProviderTypeResolver(scope.ServiceProvider);
    }

    /// <inheritdoc/>
    public ITypeResolver TypeResolver
        => _resolver;

    /// <inheritdoc/>
    public void Dispose()
    {
        _scope.Dispose();
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        return _scope.DisposeAsync();
    }
}
