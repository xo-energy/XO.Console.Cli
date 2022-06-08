namespace XO.Console.Cli;

/// <summary>
/// A factory for creating instances of <see cref="ITypeResolverScope"/>.
/// </summary>
public interface ITypeResolverScopeFactory
{
    /// <summary>
    /// Creates a new <see cref="ITypeResolverScope"/> to resolve dependencies for a command execution.
    /// </summary>
    /// <returns>An <see cref="ITypeResolverScope"/> controlling the lifetime of the scope.</returns>
    ITypeResolverScope CreateScope();
}
