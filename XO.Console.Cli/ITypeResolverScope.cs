namespace XO.Console.Cli;

/// <summary>
/// Represents a service lifetime scoped to a command execution.
/// </summary>
public interface ITypeResolverScope : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// The <see cref="ITypeResolver"/> used to resolve dependencies from the scope.
    /// </summary>
    public ITypeResolver TypeResolver { get; }
}
