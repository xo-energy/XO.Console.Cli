using Microsoft.Extensions.DependencyInjection;

namespace XO.Console.Cli;

/// <summary>
/// Implements scoped service lifetime using <see cref="ServiceProviderServiceExtensions.CreateAsyncScope(IServiceProvider)"/>.
/// </summary>
public sealed class ServiceProviderTypeResolverScope : ServiceProviderTypeResolver, ITypeResolverScope
{
    private readonly AsyncServiceScope _scope;

    /// <summary>
    /// Initializes a new instance of <see cref="ServiceProviderTypeResolverScope"/>.
    /// </summary>
    /// <param name="scope">The <see cref="AsyncServiceScope"/> that controls the scope lifetime.</param>
    public ServiceProviderTypeResolverScope(AsyncServiceScope scope)
        : base(scope.ServiceProvider)
    {
        _scope = scope;
    }

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
