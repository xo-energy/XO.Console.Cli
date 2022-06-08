namespace XO.Console.Cli;

/// <summary>
/// Represents a service lifetime scoped to a command execution.
/// </summary>
public interface ITypeResolverScope : IAsyncDisposable, IDisposable, ITypeResolver
{
}
