using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace XO.Console.Cli;

/// <summary>
/// Implements type resolution using <see cref="IServiceProvider"/>.
/// </summary>
public class ServiceProviderTypeResolver : ITypeResolver, ITypeResolverScopeFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="ServiceProviderTypeResolver"/>.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> from which to resolve services.</param>
    public ServiceProviderTypeResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public ITypeResolverScope CreateScope()
    {
        var scope = _serviceProvider.CreateAsyncScope();
        try
        {
            return new ServiceProviderTypeResolverScope(scope);
        }
        catch
        {
            scope.Dispose();
            throw;
        }
    }

    /// <inheritdoc/>
    public object? Get([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type? type)
    {
        if (type == null)
            return null;

        var instance = _serviceProvider.GetService(type);

        // fall back to activator if service provider doesn't have the requested type
        if (instance == null)
            instance = ActivatorUtilities.CreateInstance(_serviceProvider, type);

        return instance;
    }

    /// <inheritdoc/>
    public T? Get<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
    {
        var instance = _serviceProvider.GetService<T>();

        // fall back to activator if service provider doesn't have the requested type
        if (instance == null)
            instance = ActivatorUtilities.CreateInstance<T>(_serviceProvider);

        return instance;
    }
}
