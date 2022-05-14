namespace XO.Console.Cli;

/// <summary>
/// Extension methods for <see cref="ICommandApp"/>.
/// </summary>
public static class CommandAppExtensions
{
    /// <summary>
    /// Parses and binds a command.
    /// </summary>
    /// <param name="app">The <see cref="ICommandApp"/>.</param>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>A <see cref="CommandContext"/> containing the command and parameters instances bound from <paramref name="args"/>.</returns>
    public static CommandContext Bind(this ICommandApp app, IReadOnlyList<string> args)
    {
        var parse = app.Parse(args);
        return app.Bind(parse);
    }

    /// <summary>
    /// Parses, binds, and executes a command.
    /// </summary>
    /// <param name="app">The <see cref="ICommandApp"/>.</param>
    /// <param name="args">The command-line arguments.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that may request that command execution be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> whose result is the exit code of the command.</returns>
    public static async Task<int> ExecuteAsync(
        this ICommandApp app,
        IReadOnlyList<string> args,
        CancellationToken cancellationToken = default)
    {
        var parse = app.Parse(args);
        var binding = app.Bind(parse);

        return await app.ExecuteAsync(binding, cancellationToken)
            .ConfigureAwait(false);
    }
}
