namespace XO.Console.Cli.Features;

/// <summary>
/// Exposes the real <see cref="System.Console"/>.
/// </summary>
internal sealed class SystemConsole : IConsole
{
    /// <inheritdoc/>
    public TextReader Input => System.Console.In;

    /// <inheritdoc/>
    public TextWriter Output => System.Console.Out;

    /// <inheritdoc/>
    public TextWriter Error => System.Console.Error;

    /// <inheritdoc/>
    public bool IsInputRedirected => System.Console.IsInputRedirected;

    /// <inheritdoc/>
    public bool IsOutputRedirected => System.Console.IsOutputRedirected;

    /// <inheritdoc/>
    public bool IsErrorRedirected => System.Console.IsErrorRedirected;
}
