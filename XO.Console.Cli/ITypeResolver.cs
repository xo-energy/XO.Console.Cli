namespace XO.Console.Cli;

/// <summary>
/// Represents an object that can create instances of other types.
/// </summary>
public interface ITypeResolver
{
    /// <summary>
    /// Gets an instance of the specified type.
    /// </summary>
    /// <param name="type">The type to create.</param>
    /// <returns>An instance of <paramref name="type"/>, if possible; otherwise, <c>null</c>.</returns>
    object? Get(Type type);

    /// <summary>
    /// Gets an instance of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to create.</typeparam>
    /// <returns>An instance of <typeparamref name="T"/>, if possible; otherwise, <c>default(T)</c>.</returns>
    T? Get<T>();
}
